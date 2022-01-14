using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace sqs_processor.Entities
{
    [Display(Name = "StockScreeners")]
    public class StockScreener
    {

        [Key]
        public int id { get; set; }
        public string Name { get; set; }
    }
}
