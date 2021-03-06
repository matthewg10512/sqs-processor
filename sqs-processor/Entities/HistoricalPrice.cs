using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace sqs_processor.Entities
{
    [Display(Name = "HistoricalPrices")]
    public class HistoricalPrice : IEntity
    {
        [Key]
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
