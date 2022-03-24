using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Models
{
    public class SiteAuctionItemDto
    {

        public string ProductName { get; set; }
        public string ItemUrl { get; set; }
        public string TotalBids { get; set; }
        public string TimeLeft { get; set; }
        public string ItemPrice { get; set; }
        public string ItemShipping { get; set; }
        public string ImageUrl { get; set; }
        public string AuctionEnded { get; set; }
        public string BuyNow { get; set; }
    }
}
