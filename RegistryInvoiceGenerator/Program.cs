using DinkToPdf;
using QRCoder;
using System;
using System.Configuration;
using System.Drawing;
using System.IO;

namespace RegistryInvoiceGenerator
{
    class Program
    {
        static int Main(string[] args)
        {
            // Загружаем параметры
            var consoleArgsParser = new ConsoleArgsParser();
            var invoiceInfo1 = consoleArgsParser.ParseToInvoiceInfo(args);
            if (invoiceInfo1 == null) return -9;  // Не задан лицевой счет
            var invoiceInfo2 = consoleArgsParser.ParseToInvoiceInfo(args, "-2");

            // Инициализируем временную директорию
            var tmpDirectory = Path.Combine(Path.GetTempPath(), "registry-invoice-generator");
            if (!Directory.Exists(tmpDirectory))
                Directory.CreateDirectory(tmpDirectory);

            // Формируем QR-код
            var qrAccount1FileName = Guid.NewGuid() + ".bmp";
            var qrAccount1FileNameFull = Path.Combine(tmpDirectory, qrAccount1FileName);
            var qr = new Qr();
            if (!qr.QrSave(qr.QrGenerate(qr.GetQrInvoiceContent(invoiceInfo1)), qrAccount1FileNameFull)) return -1; // Код -1: Ошибка при сохранении qr-кода

            string qrAccount2FileName = null;
            string qrAccount2FileNameFull = null;
            if (invoiceInfo2 != null)
            {
                qrAccount2FileName = Guid.NewGuid() + ".bmp";
                qrAccount2FileNameFull = Path.Combine(tmpDirectory, qrAccount2FileName);
                qr = new Qr();
                if (!qr.QrSave(qr.QrGenerate(qr.GetQrInvoiceContent(invoiceInfo2)), qrAccount2FileNameFull)) return -1; // Код -1: Ошибка при сохранении qr-кода
            }

            // Формируем html-извещение
            var tmpHtmlFileName = Path.Combine(tmpDirectory, Guid.NewGuid() + ".html");
            var htmlToPdfConverter = new HtmlToPdfConverter();
            var htmlContent = htmlToPdfConverter.GetHtmlTemplateContent(invoiceInfo2 == null ? 1 : 2);
            htmlContent = htmlToPdfConverter.GenerateHtmlContent(htmlContent, invoiceInfo1, invoiceInfo2, qrAccount1FileName, qrAccount2FileName);

            var invoiceTitleFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "invoice-title.png");
            var calcCenterFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "calc-center.png");
            if (!htmlToPdfConverter.HtmlSave(htmlContent, tmpHtmlFileName, new string[] { invoiceTitleFileName, calcCenterFileName })) return -2; // Код -2: Ошибка сохранения html-файла

            // Конвертируем html в pdf
            var tmpPdfFileName = Guid.NewGuid() + ".pdf";
            var tmpPdfFileNameFull = Path.Combine(tmpDirectory, tmpPdfFileName);
            if (!htmlToPdfConverter.ConvertHtmlToPdf(tmpHtmlFileName, tmpPdfFileNameFull)) return -3; // Код -3: Ошибка конвертации html в pdf

            // Отправляем файл по электронной почте
            var smtpHost = ConfigurationManager.AppSettings["smtpHost"];
            var smtpPort = int.Parse(ConfigurationManager.AppSettings["smtpPort"]);
            var smtpFrom = ConfigurationManager.AppSettings["smtpFrom"];
            var smtpSender = new SmtpSender(smtpHost, smtpPort, smtpFrom);
            if (string.IsNullOrWhiteSpace(invoiceInfo1.Email) && string.IsNullOrWhiteSpace(invoiceInfo1.MoveToFileName)) return -6; // Не задано целевое назначение квитанции
            if (!string.IsNullOrWhiteSpace(invoiceInfo1.Email))
                if (!smtpSender.SendMail(invoiceInfo1.Email, "Счет извещение на оплату за наем жилого помещения", invoiceInfo1.MessageBody ?? "", tmpPdfFileNameFull)) return -4; // Код -4: Ошибка отправки сообщения
            if (!string.IsNullOrWhiteSpace(invoiceInfo1.MoveToFileName))
            {
                var fileInfo = new FileInfo(invoiceInfo1.MoveToFileName);
                if (!fileInfo.Directory.Exists) return -7; // Некорректно указана директория назначения файла
                try
                {
                    File.Copy(tmpPdfFileNameFull, invoiceInfo1.MoveToFileName, true);
                } catch
                {
                    return -8; // Ошибка копирования файла
                }
            }
            // Удаляем временные файлы
            try
            {
                File.Delete(qrAccount1FileNameFull);
                if (qrAccount2FileNameFull != null)
                    File.Delete(qrAccount2FileNameFull);
                File.Delete(tmpHtmlFileName);
                File.Delete(tmpPdfFileNameFull);
            } catch
            {
                return -5; // Код -5: Ошибка удаления временных файлов
            }

            return 0;
        }
    }
}
