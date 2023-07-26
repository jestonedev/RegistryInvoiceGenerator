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
        public string GetHtmlTemplateContent(int invoicePerPageCount)
        {
            var htmlFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, invoicePerPageCount == 1 ? "InvoiceTemplate1.html" : "InvoiceTemplate2.html");
            return File.ReadAllText(htmlFileName);
        }

        public string GenerateHtmlContent(string content, InvoiceInfo invoiceInfo1, InvoiceInfo invoiceInfo2, string qrFileName1, string qrFileName2)
        {
            var info = new Dictionary<string, Tuple<InvoiceInfo, string>> {
                { "", new Tuple<InvoiceInfo, string>(invoiceInfo1, qrFileName1) }
            };
            if (invoiceInfo2 != null)
            {
                info.Add("-2", new Tuple<InvoiceInfo, string>(invoiceInfo2, qrFileName2));
            }
            foreach (var invoiceInfoPair in info)
            {
                var invoiceInfo = invoiceInfoPair.Value.Item1;
                var qrFileName = invoiceInfoPair.Value.Item2;
                content = content.Replace("{qr"+invoiceInfoPair.Key+"}", qrFileName);
                content = content.Replace("{address" + invoiceInfoPair.Key + "}", invoiceInfo.Address);
                content = content.Replace("{account" + invoiceInfoPair.Key + "}", invoiceInfo.Account);
                content = content.Replace("{account-gis-zkh" + invoiceInfoPair.Key + "}", invoiceInfo.AccountGisZkh);
                content = content.Replace("{tenant" + invoiceInfoPair.Key + "}", invoiceInfo.Tenant);
                content = content.Replace("{prev-date" + invoiceInfoPair.Key + "}", new DateTime(invoiceInfo.OnDate.Year, invoiceInfo.OnDate.Month, 1).AddDays(-1).ToString("dd.MM.yyyy"));
                content = content.Replace("{date" + invoiceInfoPair.Key + "}", invoiceInfo.OnDate.ToString("MMMM yyyy", CultureInfo.GetCultureInfo("ru-RU")).ToLower());
                content = content.Replace("{on-date-payments" + invoiceInfoPair.Key + "}", new DateTime(invoiceInfo.OnDate.Year, invoiceInfo.OnDate.Month, 21).ToString("dd.MM.yyyy"));
                content = content.Replace("{balance-input" + invoiceInfoPair.Key + "}", invoiceInfo.BalanceInput.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")));
                content = content.Replace("{charging" + invoiceInfoPair.Key + "}", invoiceInfo.Charging.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")));
                content = content.Replace("{charging-tenancy" + invoiceInfoPair.Key + "}", invoiceInfo.ChargingTenancy.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")));
                content = content.Replace("{charging-penalty" + invoiceInfoPair.Key + "}", invoiceInfo.ChargingPenalty.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")));
                content = content.Replace("{recalc-tenancy" + invoiceInfoPair.Key + "}", invoiceInfo.RecalcTenancy.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")));
                content = content.Replace("{recalc-penalty" + invoiceInfoPair.Key + "}", invoiceInfo.RecalcTenancy.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")));
                content = content.Replace("{payed" + invoiceInfoPair.Key + "}", invoiceInfo.Payed.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")));
                content = content.Replace("{balance-output" + invoiceInfoPair.Key + "}", invoiceInfo.BalanceOutput.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")));
                content = content.Replace("{total-area" + invoiceInfoPair.Key + "}", invoiceInfo.TotalArea.ToString("N1", CultureInfo.GetCultureInfo("ru-RU")));
                content = content.Replace("{total-area-n2" + invoiceInfoPair.Key + "}", invoiceInfo.TotalArea.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")));
                content = content.Replace("{prescribed" + invoiceInfoPair.Key + "}", invoiceInfo.Prescribed.ToString());
                content = content.Replace("{tariff" + invoiceInfoPair.Key + "}", invoiceInfo.TotalArea == 0 ? "0" :
                    Math.Round(invoiceInfo.ChargingTenancy / (decimal)invoiceInfo.TotalArea, 3).ToString("N3", CultureInfo.GetCultureInfo("ru-RU")));
                content = content.Replace("{total-charging" + invoiceInfoPair.Key + "}", (invoiceInfo.ChargingTenancy + invoiceInfo.RecalcTenancy).ToString("N2", CultureInfo.GetCultureInfo("ru-RU")));
                content = content.Replace("{total-penalty" + invoiceInfoPair.Key + "}", (invoiceInfo.ChargingPenalty + invoiceInfo.RecalcPenalty).ToString("N2", CultureInfo.GetCultureInfo("ru-RU")));
                content = content.Replace("{penalty-display" + invoiceInfoPair.Key + "}", (invoiceInfo.ChargingPenalty + invoiceInfo.RecalcPenalty) != 0 ? "table-row" : "none");
            }
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
            } catch(Exception e)
            {
                Console.WriteLine(e.Message);
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
