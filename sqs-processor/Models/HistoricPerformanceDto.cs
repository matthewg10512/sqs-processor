using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace sqs_processor.Models
{
    public class HistoricPerformanceDto
    {
        public int Id { get; set; }
        public int SecurityId { get; set; }
        public DateTime DateCalculated { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? WeekAgoPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MonthAgoPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? QuaterAgoPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? YearAgoPrice { get; set; }

    }
}
