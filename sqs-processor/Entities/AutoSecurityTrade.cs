using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace sqs_processor.Entities
{
    [Display(Name = "AutoSecurityTrades")]
    public class AutoSecurityTrade
    {
        [Key]
        public int Id { get; set; }
        public int? SecurityId { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime? SellDate { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? PurchasePrice { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? SellPrice { get; set; }
        public int? SharesBought { get; set; }
        public int? PercentageLevel { get; set; }
    }
}
