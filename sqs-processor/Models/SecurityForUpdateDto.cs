using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace sqs_processor.Models
{
    public class SecurityForUpdateDto
    {
        public int Id { get; set; }
        public string SecurityType { get; set; }
        public string Name { get; set; }

        public string Symbol { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Dividend { get; set; }
        public DateTime? DividendDate { get; set; }
        public DateTime? EarningsDate { get; set; }
        public DateTime? LastModified { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentPrice { get; set; }
        public int? IPOYear { get; set; }
        public String Sector { get; set; }
        public String Industry { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? YearLow { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? YearHigh { get; set; }
        public int? Volume { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DayLow { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? DayHigh { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PriorDayOpen { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? PercentageChange { get; set; }

        public bool preferred { get; set; }

        public bool excludeHistorical { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime IPODate { get; set; }

    }
}
