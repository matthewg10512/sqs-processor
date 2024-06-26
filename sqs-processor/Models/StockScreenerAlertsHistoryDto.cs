﻿using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Models
{
    public class StockScreenerAlertsHistoryDto
    {
        public int id { get; set; }
        public int SecurityId { get; set; }
        public int StockScreenerId { get; set; }
        public DateTime DateRecorded { get; set; }
        public decimal AlertPrice { get; set; }
        public decimal AlertPercent { get; set; }
    }
}
