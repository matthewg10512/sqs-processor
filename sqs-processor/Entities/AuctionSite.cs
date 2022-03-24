using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Entities
{
   public class AuctionSite
    {
        public int Id { get; set; }
        public string SiteName { get; set; }
        public string SearchURL { get; set; }
        public string JsCode { get; set; }
        public string SearchWordReplace { get; set; }
        public string PageReplace { get; set; }
        public int InitialUrlLaunchTime { get; set; }
        public int PageNextLaunchTime { get; set; }
        public int ScrapingType { get; set; }
        public string JsCodeSinglePage { get; set; }
        public int StartPaging { get; set; }
        public string PrependProductSiteUrl { get; set; }

    }
}
