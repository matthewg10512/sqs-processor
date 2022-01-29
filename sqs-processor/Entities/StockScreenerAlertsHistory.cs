using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace sqs_processor.Entities
{
    [Display(Name = "StockScreenerAlertsHistory")]
    public class StockScreenerAlertsHistory
    {
        [Key]
        public int id { get; set; }
        public int SecurityId { get; set; }
        public int StockScreenerId { get; set; }
        public DateTime DateRecorded { get; set; }
        public decimal alertPrice { get; set; }
        public decimal alertPercent { get; set; }
    }
}
