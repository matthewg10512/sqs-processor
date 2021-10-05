using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Entities
{
    public class SecurityAlertType
    {
        public int Id { get; set; }
        public string alertName { get; set; }

        public int frequency { get; set; }
        public decimal PercentageCheck { get; set; }
        public bool preferred { get; set; }
        public string awsSNSURL { get; set; }


    }
}
