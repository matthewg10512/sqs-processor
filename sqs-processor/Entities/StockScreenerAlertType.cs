using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace sqs_processor.Entities
{
    [Display(Name = "StockScreenerAlertTypes")]
    public class StockScreenerAlertType
    {
        [Key]
       public int id { get; set; }
        public int frequency{ get; set; }
        public string awsSNSURL{ get; set; }
        public string AlertType { get; set; }
        
    }
}
