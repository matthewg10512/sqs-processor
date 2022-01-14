using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace sqs_processor.Models
{
    public class PriorPurchaseEstimateDto
    {
        public int Id { get; set; }
        public int SecurityId { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateModified { get; set; }
        [Column(TypeName = "decimal(12,3)")]
        public decimal Shares { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal PurchasePrice { get; set; }


        public DateTime FirstPurchaseDate { get; set; }
        public int PurchaseFrequency { get; set; }

    }
}
