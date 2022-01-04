using sqs_processor.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Models
{
   public class StockPurchaseOption
    {

        public Security Security { get; set; }
        public SecurityPercentageStatistic SecurityPercentageStatistic { get; set; }
        public CurrentPeakRange CurrentPeakRange { get; set; }

        public SecurityPurchaseCheck SecurityPurchaseCheck { get; set; }

        


    }
}
