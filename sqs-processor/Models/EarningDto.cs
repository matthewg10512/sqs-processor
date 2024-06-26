﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace sqs_processor.Models
{
    public class EarningDto
    {

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
        public string symbol { get; set; }
    }
}
