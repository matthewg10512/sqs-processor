using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace sqs_processor.Models
{
    public class HistoricalPriceCloseHistoricDateDto
    {
        public int SecurityId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Close { get; set; }
        public DateTime HistoricDate { get; set; }
    }
}
