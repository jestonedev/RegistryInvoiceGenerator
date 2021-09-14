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
            var invoiceInfo = consoleArgsParser.ParseToInvoiceInfo(args);

            // Инициализируем временную директорию
            var tmpDirectory = Path.Combine(Path.GetTempPath(), "registry-invoice-generator");
            if (!Directory.Exists(tmpDirectory))
                Directory.CreateDirectory(tmpDirectory);

            // Формируем QR-код
            var tmpQrFileName = Guid.NewGuid() + ".bmp";
            var tmpQrFileNameFull = Path.Combine(tmpDirectory, tmpQrFileName);
            var qr = new Qr();
            if (!qr.QrSave(qr.QrGenerate(qr.GetQrInvoiceContent(invoiceInfo)), tmpQrFileNameFull)) return -1; // Код -1: Ошибка при сохранении qr-кода

            // Формируем html-извещение
            var tmpHtmlFileName = Path.Combine(tmpDirectory, Guid.NewGuid() + ".html");
            var htmlToPdfConverter = new HtmlToPdfConverter();
            var htmlContent = htmlToPdfConverter.GetHtmlTemplateContent();
            htmlContent = htmlToPdfConverter.GenerateHtmlContent(htmlContent, invoiceInfo, tmpQrFileName);

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
            if (string.IsNullOrWhiteSpace(invoiceInfo.Email) && string.IsNullOrWhiteSpace(invoiceInfo.MoveToFileName)) return -6; // Не задано целевое назначение квитанции
            if (!string.IsNullOrWhiteSpace(invoiceInfo.Email))
                if (!smtpSender.SendMail(invoiceInfo.Email, "Счет извещение на оплату за наем жилого помещения", invoiceInfo.MessageBody ?? "", tmpPdfFileNameFull)) return -4; // Код -4: Ошибка отправки сообщения
            if (!string.IsNullOrWhiteSpace(invoiceInfo.MoveToFileName))
            {
                var fileInfo = new FileInfo(invoiceInfo.MoveToFileName);
                if (!fileInfo.Directory.Exists) return -7; // Некорректно указана директория назначения файла
                try
                {
                    File.Copy(tmpPdfFileNameFull, invoiceInfo.MoveToFileName, true);
                } catch
                {
                    return -8; // Ошибка копирования файла
                }
            }
            // Удаляем временные файлы
            try
            {
                File.Delete(tmpQrFileNameFull);
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
