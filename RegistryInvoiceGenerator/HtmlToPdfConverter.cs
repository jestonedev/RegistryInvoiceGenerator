using DinkToPdf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace RegistryInvoiceGenerator
{
    public class HtmlToPdfConverter
    {
        public string GetHtmlTemplateContent()
        {
            var htmlFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InvoiceTemplate.html");
            return File.ReadAllText(htmlFileName);
        }

        public string GenerateHtmlContent(string content, InvoiceInfo invoiceInfo, string qrFileName)
        {
            content = content.Replace("{qr}", qrFileName);
            content = content.Replace("{address}", invoiceInfo.Address);
            content = content.Replace("{account}", invoiceInfo.Account);
            content = content.Replace("{tenant}", invoiceInfo.Tenant);
            content = content.Replace("{prev-date}", new DateTime(invoiceInfo.OnDate.Year, invoiceInfo.OnDate.Month, 1).AddDays(-1).ToString("dd.MM.yyyy"));
            content = content.Replace("{date}", invoiceInfo.OnDate.ToString("MMMM yyyy", CultureInfo.GetCultureInfo("ru-RU")).ToLower());
            content = content.Replace("{balance-input}", invoiceInfo.BalanceInput.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")));
            content = content.Replace("{charging}", invoiceInfo.Charging.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")));
            content = content.Replace("{recalc}", invoiceInfo.Recalc.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")));
            content = content.Replace("{payed}", invoiceInfo.Payed.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")));
            content = content.Replace("{balance-output}", invoiceInfo.BalanceOutput.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")));
            content = content.Replace("{total-area}", invoiceInfo.TotalArea.ToString("N1", CultureInfo.GetCultureInfo("ru-RU")));
            content = content.Replace("{total-area-2}", invoiceInfo.TotalArea.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")));
            content = content.Replace("{prescribed}", invoiceInfo.Prescribed.ToString());
            var payment = invoiceInfo.BalanceOutput + invoiceInfo.Payed - invoiceInfo.Recalc - invoiceInfo.BalanceInput;
            content = content.Replace("{payment}", payment.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")));
            content = content.Replace("{tariff}", Math.Round(payment/(decimal)invoiceInfo.TotalArea, 3).ToString("N3", CultureInfo.GetCultureInfo("ru-RU")));
            return content;
        }

        public bool ConvertHtmlToPdf(string htmlFileName, string pdfFileName)
        {
            try
            {
                var converter = new BasicConverter(new PdfTools());
                var doc = new HtmlToPdfDocument()
                {
                    GlobalSettings = {
                        ColorMode = ColorMode.Color,
                        Orientation = Orientation.Portrait,
                        PaperSize = PaperKind.A4,
                        ImageQuality = 100,

                        Out = pdfFileName
                    },
                    Objects = {
                        new ObjectSettings() {
                            Page = htmlFileName,
                        }
                    }
                };
                converter.Convert(doc);
                return true;
            } catch
            {
                return false;
            }
        }

        public bool HtmlSave(string htmlContent, string htmlFileName, string[] additionalFiles)
        {
            using (var sw = new StreamWriter(htmlFileName))
            {
                sw.Write(htmlContent);
            }
            var fileInfo = new FileInfo(htmlFileName);
            foreach (var file in additionalFiles)
            {
                var additionalFileInfo = new FileInfo(file);
                var newFileName = Path.Combine(fileInfo.DirectoryName, additionalFileInfo.Name);
                if (!File.Exists(newFileName))
                    File.Copy(file, newFileName);
            }
            return true;
        }
    }
}
