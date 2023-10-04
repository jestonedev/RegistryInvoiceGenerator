using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace RegistryInvoiceGenerator
{
    public class ConsoleArgsParser
    {
        public InvoiceInfo ParseToInvoiceInfo(string[] args, string argsPostfix = "")
        {
            var invoiceInfo = new InvoiceInfo();
            foreach(var arg in args)
            {
                var argParts = arg.Split("=");
                if (argParts.Length != 2) continue;
                var argName = argParts[0];
                if (!argName.EndsWith(argsPostfix)) continue;
                argName = argName.Substring(0, argName.Length - argsPostfix.Length);
                switch (argName)
                {
                    case "--address":
                        invoiceInfo.Address = argParts[1];
                        break;
                    case "--account":
                        invoiceInfo.Account = argParts[1];
                        break;
                    case "--account-gis-zkh":
                        invoiceInfo.AccountGisZkh = argParts[1];
                        break;
                    case "--tenant":
                        invoiceInfo.Tenant = argParts[1];
                        break;
                    case "--on-date":
                        var dateParts = argParts[1].Split(".");
                        if (dateParts.Length != 3) continue;
                        var date = DateTime.Now;
                        if (!int.TryParse(dateParts[0], out int day))
                        {
                            day = 1;
                        }
                        if (!int.TryParse(dateParts[1], out int month))
                        {
                            month = date.Month;
                        }
                        if (!int.TryParse(dateParts[2], out int year))
                        {
                            year = date.Year;
                        }
                        invoiceInfo.OnDate = new DateTime(year, month, day);
                        break;
                    case "--balance-input":
                        if (!decimal.TryParse(argParts[1], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out decimal balanceInput))
                        {
                            balanceInput = 0;
                        }
                        invoiceInfo.BalanceInput = balanceInput;
                        break;
                    case "--charging-tenancy":
                        if (!decimal.TryParse(argParts[1], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out decimal chargingTenancy))
                        {
                            chargingTenancy = 0;
                        }
                        invoiceInfo.ChargingTenancy = chargingTenancy;
                        break;
                    case "--charging-penalty":
                        if (!decimal.TryParse(argParts[1], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out decimal chargingPenalty))
                        {
                            chargingTenancy = 0;
                        }
                        invoiceInfo.ChargingPenalty = chargingPenalty;
                        break;
                    case "--payed":
                        if (!decimal.TryParse(argParts[1], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out decimal payed))
                        {
                            payed = 0;
                        }
                        invoiceInfo.Payed = payed;
                        break;
                    case "--recalc-tenancy":
                        if (!decimal.TryParse(argParts[1], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out decimal recalcTenancy))
                        {
                            recalcTenancy = 0;
                        }
                        invoiceInfo.RecalcTenancy = recalcTenancy;
                        break;
                    case "--recalc-penalty":
                        if (!decimal.TryParse(argParts[1], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out decimal recalcPenalty))
                        {
                            recalcPenalty = 0;
                        }
                        invoiceInfo.RecalcPenalty = recalcPenalty;
                        break;
                    case "--balance-output":
                        if (!decimal.TryParse(argParts[1], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out decimal balanceOutput))
                        {
                            balanceOutput = 0;
                        }
                        invoiceInfo.BalanceOutput = Math.Max(balanceOutput, 0);
                        break;
                    case "--total-area":
                        if (!float.TryParse(argParts[1], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out float totalArea))
                        {
                            totalArea = 0;
                        }
                        invoiceInfo.TotalArea = totalArea;
                        break;
                    case "--tariff":
                        if (!float.TryParse(argParts[1], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out float tariff))
                        {
                            totalArea = 0;
                        }
                        invoiceInfo.Tariff = tariff;
                        break;
                    case "--prescribed":
                        if (!int.TryParse(argParts[1], out int prescribed))
                        {
                            prescribed = 0;
                        }
                        invoiceInfo.Prescribed = prescribed;
                        break;
                    case "--email":
                        invoiceInfo.Email = argParts[1];
                        break;
                    case "--move-to-filename":
                        invoiceInfo.MoveToFileName = argParts[1];
                        break;
                    case "--message":
                        invoiceInfo.MessageBody = argParts[1];
                        break;
                }
            }
            if (invoiceInfo.Account == null) return null;
            return invoiceInfo;
        }
    }
}
