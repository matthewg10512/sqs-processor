using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Entities
{
   public class AuctionSiteCategoryWord
    {
        public int Id { get; set; }

        public int AuctionCategoryId { get; set; }

        public int AuctionSearchWordId { get; set; }
    }
}
