using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace sqs_processor.Models
{
    public class EarningSecurityPercentage
    {

        public int Id { get; set; }
        public int SecurityId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? PercentageChange { get; set; }
        public string ReportTime { get; set; }
        public DateTime ActualEarningsDate { get; set; }

        public DateTime HistoricDate { get; set; }

    }
}
