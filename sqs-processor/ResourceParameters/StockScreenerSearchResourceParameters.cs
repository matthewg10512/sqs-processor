using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.ResourceParameters
{
    public class StockScreenerSearchResourceParameters
    {

        public int? priorPurchaseEstimateSharesRangeLow { get; set; } // 60
        public int? securityVolumeRangeLow { get; set; } //100000
        public DateTime? securityLastModifiedRangeLow { get; set; }//Prior days

        public decimal? securitypercentChangeRangeHigh { get; set; } //0
        public decimal? securitypercentChangeRangeLow { get; set; } //0


        public decimal? priorPurchaseEstimateYearlyPercentRangeLow { get; set; }

        

        public string percentDropType { get; set; }
        public string calculatedPercentDropType { get; set; }

        public decimal? percentFrom52WeekLowRangeLow { get; set; }
        public decimal? percentFrom52WeekLowRangeHigh { get; set; }

        public decimal? percentFrom52WeekHighRangeLow { get; set; }
        public decimal? percentFrom52WeekHighRangeHigh { get; set; }

    }
}
