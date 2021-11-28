using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace sqs_processor.Entities
{

    [Display(Name = "Earnings")]
    public class Earning
    {
        [Key]
        public int Id { get; set; }
        public int SecurityId { get; set; }
        public DateTime ActualEarningsDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EPSEstimate { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal ReportedEPS { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal GAAPEPS { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal RevenueEstimate { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal ActualRevenue { get; set; }

        public string ReportTime { get; set; }


    }
}
