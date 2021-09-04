using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace sqs_processor.Models
{
    public class HistoricalPriceforUpdateDto
    {
        /*
        public int SecurityId { get; set; }

        public decimal Open { get; set; }
        public decimal Close { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public int Volume { get; set; }
        public DateTime HistoricDate { get; set; }
        */

        public int Id { get; set; }
        public int SecurityId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Open { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Close { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? High { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Low { get; set; }
        public int Volume { get; set; }
        public DateTime HistoricDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PercentChange { get; set; }


    }
}
