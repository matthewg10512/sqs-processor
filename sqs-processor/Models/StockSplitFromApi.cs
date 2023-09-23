using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace sqs_processor.Models
{
    

        public class StockSplitFromApi
        {
            public string symbol { get; set; }
            public HistoricalSplit[] historical { get; set; }
        }

        public class HistoricalSplit
        {
            public string date { get; set; }
            public string label { get; set; }
        [Column(TypeName = "decimal(18,6)")]
        public decimal numerator { get; set; }
        [Column(TypeName = "decimal(18,6)")]
        public decimal denominator { get; set; }
        }

    
}
