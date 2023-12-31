﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryInvoiceGenerator
{
    public class InvoiceInfo
    {
        public string Address { get; set; }
        public string PostIndex { get; set; }
        public string Account { get; set; }
        public string AccountGisZkh { get; set; }
        public string Tenant { get; set; }
        public DateTime OnDate { get; set; }
        public decimal BalanceInput { get; set; }
        public decimal Charging { get { return ChargingTenancy + ChargingPenalty; } }
        public decimal Payed { get; set; }
        public decimal BalanceOutput { get; set; }
        public float TotalArea { get; set; }
        public float Tariff { get; set; }
        public int Prescribed { get; set; }
        public string Email { get; set; }
        public string MoveToFileName { get; set; }
        public string MessageBody { get; set; }
        public decimal ChargingTenancy { get; internal set; }
        public decimal ChargingPenalty { get; internal set; }
        public decimal RecalcTenancy { get; internal set; }
        public decimal RecalcPenalty { get; internal set; }
    }
}
