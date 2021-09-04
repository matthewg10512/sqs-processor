using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace sqs_processor.Entities
{
    public class tempSecurityAlerts
    {
        public int Id { get; set; }
        public int securityId { get; set; }
        public int securityAlertType { get; set; }

        public bool? preferred {get;set;}
        public string symbol { get; set; }
        public string stockName { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? percentageChange { get; set; }

    }
}
