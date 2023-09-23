using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace sqs_processor.Models
{
    public class HistoricalPriceOpenHistoricDateDto
    {
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Open { get; set; }
        public DateTime HistoricDate { get; set; }
    }
}
