using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Entities
{
   public class AuctionPageLoadCheck
    {
        public int  Id { get; set; }
        public int AuctionSiteId { get; set; }
        public string WordCheck { get; set; }
        public int WordGrouping { get; set; }
    }
}
