using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace sqs_processor.Entities
{
    [Display(Name = "SecurityAnalytics")]
    public class SecurityAnalytic
    {
        [Key]
        public int Id { get; set; }
        public int SecurityId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MovingAverageDay10 { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal MovingAverageDay20 { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal MovingAverageDay30 { get; set; }
        [Column(TypeName = "decimal(18,2)")] 
        public decimal MovingAverageDay50 { get; set; }
        [Column(TypeName = "decimal(18,2)")] 
        public decimal MovingAverageDay100 { get; set; }
        [Column(TypeName = "decimal(18,2)")] 
        public decimal MovingAverageDay200 { get; set; }
        [Column(TypeName = "decimal(18,2)")] 
        public decimal MovingAverageYear1 { get; set; }
        [Column(TypeName = "decimal(18,2)")] 
        public decimal MovingAverageYear2 { get; set; }

        [Column(TypeName = "decimal(18,2)")] 
        public decimal MaxPriceDay10 { get; set; }

        [Column(TypeName = "decimal(18,2)")] 
        public decimal MaxPriceDay20 { get; set; }
        [Column(TypeName = "decimal(18,2)")] 
        public decimal MaxPriceDay30 { get; set; }
        [Column(TypeName = "decimal(18,2)")] 
        public decimal MaxPriceDay50 { get; set; }
        [Column(TypeName = "decimal(18,2)")] 
        public decimal MaxPriceDay100 { get; set; }
        [Column(TypeName = "decimal(18,2)")] 
        public decimal MaxPriceDay200 { get; set; }
        [Column(TypeName = "decimal(18,2)")] 
        public decimal MaxPriceYear1 { get; set; }
        [Column(TypeName = "decimal(18,2)")] 
        public decimal MaxPriceYear2 { get; set; }

        [Column(TypeName = "decimal(18,2)")] 
        public decimal MinPriceDay10 { get; set; }
        [Column(TypeName = "decimal(18,2)")] 
        public decimal MinPriceDay20 { get; set; }
        [Column(TypeName = "decimal(18,2)")] 
        public decimal MinPriceDay30 { get; set; }
        [Column(TypeName = "decimal(18,2)")] 
        public decimal MinPriceDay50 { get; set; }
        [Column(TypeName = "decimal(18,2)")] 
        public decimal MinPriceDay100 { get; set; }
        [Column(TypeName = "decimal(18,2)")] 
        public decimal MinPriceDay200 { get; set; }
        [Column(TypeName = "decimal(18,2)")] 
        public decimal MinPriceYear1 { get; set; }
        [Column(TypeName = "decimal(18,2)")] 
        public decimal MinPriceYear2 { get; set; }
        public DateTime LastModified { get; set; }
        public DateTime LatestDateChecked { get; set; }

    }
}
