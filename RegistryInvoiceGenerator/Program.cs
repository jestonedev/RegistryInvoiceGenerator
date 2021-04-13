using DinkToPdf;
using QRCoder;
using System;
using System.Drawing;
using System.IO;

namespace RegistryInvoiceGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: передавать информацию для генерации документа через args (включая имя pdf-файла - tmpFileName)

            var tmpDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tmp");
            if (!Directory.Exists(tmpDirectory))
                Directory.CreateDirectory(tmpDirectory);
            
            // Generate qr-code
            var qrContent = @"ST00011|Name=УФК по Иркутской области (КУМИ г.Братска)|PersonalAcc=03100643000000013400|BankName=Отд. Иркутск Банка России//УФК по Ирк.обл.г.Иркутск|BIC=012520101|CorrespAcc=40102810145370000026|Sum=88689|Purpose=Оплата коммунальных услуг|PayeeINN=3803201800|lastName=Дюпина|firstName=Ольга|middleName=Ивановна|PayerAddress=Видимская ул., д.2, кв.2|PersAcc=10169173|PaymPeriod=январь 2021|ServiceName=2222|category=Коммунальные услуги|";
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            var qr = qrCode.GetGraphic(20);
            var tmpQrFileName = Guid.NewGuid() + ".bmp";
            var outQrFile = Path.Combine(tmpDirectory, tmpQrFileName);
            qr.Save(outQrFile);

            // Copy template files to /tmp with modifications
            var htmlFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InvoiceTemplate.html");
            var htmlTmpFileName = Path.Combine(tmpDirectory, Guid.NewGuid() + ".html");
            var htmlContent = File.ReadAllText(htmlFileName);

            // Change html content
            htmlContent = htmlContent.Replace("{qr}", tmpQrFileName);

            using (var sw = new StreamWriter(htmlTmpFileName))
            {
                sw.Write(htmlContent);
            }
            File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "invoice-title.png"), Path.Combine(tmpDirectory, "invoice-title.png"), true);
            File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "calc-center.png"), Path.Combine(tmpDirectory, "calc-center.png"), true);

            // Convert html to pdf
            var tmpFileName = Guid.NewGuid() + ".pdf";
            var outFile = Path.Combine(tmpDirectory, tmpFileName);
            var converter = new BasicConverter(new PdfTools());
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                ImageQuality = 100,

                Out = outFile
            },
                Objects = {
                new ObjectSettings() {
                        Page = htmlTmpFileName,
                    }
                }
            };
            converter.Convert(doc);

            File.Delete(tmpQrFileName);
        }
    }
}
