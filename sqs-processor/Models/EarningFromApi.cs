using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Models
{
    public class EarningFromApi
    {
        public string date { get; set; }
        public string symbol { get; set; }
        public float? eps { get; set; }
        public float? epsEstimated { get; set; }
        public string time { get; set; }
        public float? revenue { get; set; }
        public float? revenueEstimated { get; set; }
    }

}
