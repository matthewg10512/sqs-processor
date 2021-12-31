using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.ResourceParameters
{
    public class HistoricalPricesResourceParameters
    {

        public decimal? openLow { get; set; }
        public DateTime? HistoricDateLow { get; set; }

        public DateTime? HistoricDateHigh { get; set; }

    }
}
