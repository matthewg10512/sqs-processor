using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace sqs_processor.Models
{
    public class StockSplitHistoryDto
    {

        public int Id { get; set; }
        public int SecurityId { get; set; }
        
        public DateTime SplitDate { get; set; }
        [Column(TypeName = "decimal(18,6)")]
        public decimal SplitAmount { get; set; }
        [Column(TypeName = "decimal(18,6)")]
        public decimal ReverseSplitAmount { get; set; }
    }
}
