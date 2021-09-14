using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryInvoiceGenerator
{
    public class InvoiceInfo
    {
        public string Address { get; set; }        
        public string Account { get; set; }
        public string Tenant { get; set; }
        public DateTime OnDate { get; set; }
        public decimal BalanceInput { get; set; }
        public decimal Charging { get; set; }
        public decimal Payed { get; set; }
        public decimal Recalc { get; set; }
        public decimal BalanceOutput { get; set; }
        public float TotalArea { get; set; }
        public int Prescribed { get; set; }
        public string Email { get; set; }
        public string MoveToFileName { get; set; }
        public string MessageBody { get; set; }
    }
}
