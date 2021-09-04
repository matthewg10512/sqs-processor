using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.ResourceParameters
{
    public class AutoSecurityTradesResourceParameters
    {

        public DateTime rangePurchaseDateStart { get; set; }
        public DateTime rangePurchaseDateEnd { get; set; }
        public DateTime rangeSellDateStart { get; set; }
        public DateTime rangeSellDateEnd { get; set; }
        public bool? positionSold { get; set; }

        public int securityId { get; set; }


    }
}
