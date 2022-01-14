﻿using System;
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


        public decimal? priorPurchaseEstimateYearlyPercentRangeLow { get; set; }

        

        public string percentDropType { get; set; }
        public string calculatedPercentDropType { get; set; }
    }
}