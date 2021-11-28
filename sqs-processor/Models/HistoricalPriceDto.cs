using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace sqs_processor.Models
{
    public class HistoricalPriceDto
    {

        public int Id { get; set; }
        public int StockId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal open { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal close { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal high { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal low { get; set; }

        public int volume { get; set; }
        public DateTime HistoricDate { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? PercentChange { get; set; }



    }
}
