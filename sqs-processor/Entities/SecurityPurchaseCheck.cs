using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace sqs_processor.Entities
{
    [Display(Name = "SecurityPurchaseChecks")]
    public class SecurityPurchaseCheck : IEntity
    {
        [Key]
        public int Id { get; set; }
        public int SecurityId { get; set; }
       
        public DateTime DateCreated { get; set; }
       
        public DateTime DateModified { get; set; }
        [Column(TypeName = "decimal(12,3)")]
        public decimal Shares { get; set; }
        
        [Column(TypeName = "decimal(15,2)")]
        public decimal PurchasePrice { get; set; }

    }
}
