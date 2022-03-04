using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Entities
{
   public class AuctionCategorySite
    {
        public int Id { get; set; }
        public int AuctionSiteId { get; set; }
        public int SiteCategoryId { get; set; }
        public string SiteCategoryName { get; set; }

    }
}
