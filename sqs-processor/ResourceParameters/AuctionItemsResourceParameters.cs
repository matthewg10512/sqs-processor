using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.ResourceParameters
{
    public class AuctionItemsResourceParameters
    {

        public DateTime? AuctionEndDateRangeMin { get; set; }
        public DateTime? AuctionEndDateRangeMax { get; set; }

        public decimal? ItemPriceMin { get; set; }
        public decimal? ItemPriceMax { get; set; }
        public int? TotalBidsMin { get; set; }
        public int? TotalBidsMax { get; set; }

        public string ProductName { get; set; }
        public int? AuctionSiteId { get; set; }
        public int? AuctionSearchWordId { get; set; }

        public DateTime? DateModifiedRangeMin { get; set; }
        public DateTime? DateModifiedRangeMax { get; set; }

        public DateTime? DateCreatedRangeMin { get; set; }
        public DateTime? DateCreatedRangeMax { get; set; }


        public bool? AuctionEndProcessed { get; set; }

    }
}
