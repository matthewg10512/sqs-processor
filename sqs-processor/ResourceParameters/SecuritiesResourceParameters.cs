using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.ResourceParameters
{
    public class SecuritiesResourceParameters
    {

        public DateTime? lastModifiedPrior { get; set; }
        public string symbol { get; set; }
        public string sector { get; set; }
        public string industry { get; set; }
        public bool? preferred { get; set; }
        public string searchQuery { get; set; }

        public string filterType { get; set; }

        public string perChangeLow { get; set; }
        public string perChangeHigh { get; set; }


        public decimal? perFrom52WeekLow { get; set; }
        public decimal? perFrom52WeekHigh { get; set; }
        public int? minVolume { get; set; }





    }
}
