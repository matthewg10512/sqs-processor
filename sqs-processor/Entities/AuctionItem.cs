using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace sqs_processor.Entities
{
    [Display(Name = "AuctionItems")]
    public class AuctionItem
    {
        [Key]
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string Url { get; set; }
        public int TotalBids { get; set; }
        public DateTime? AuctionEndDate { get; set; }
        public decimal ItemPrice { get; set; }
        public decimal? ItemShipping { get; set; }
        public int AuctionSiteId { get; set; }
        public int AuctionSearchWordId { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public string ImageUrl { get; set; }
        public bool AuctionEndProcessed { get; set; }
        public bool BuyNow { get; set; }
    }
}
