using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Models
{
   public class AuctionSearchSiteRunDto
    {
        public int Id { get; set; }
        public int AuctionSiteId { get; set; }
        public int AuctionSearchWordId { get; set; }
        public DateTime DateSearch { get; set; }
    }
}
