using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Models
{
    public class SecurityFromApi
    {
        public string symbol { get; set; }
        public string name { get; set; }
        public float? price { get; set; }
        public float? changesPercentage { get; set; }
        public float? change { get; set; }
        public float? dayLow { get; set; }
        public float? dayHigh { get; set; }
        public float? yearHigh { get; set; }
        public float? yearLow { get; set; }
        public float? marketCap { get; set; }
        public float? priceAvg50 { get; set; }
        public float? priceAvg200 { get; set; }
        public long? volume { get; set; }
        public int? avgVolume { get; set; }
        public string exchange { get; set; }
        public float? open { get; set; }
        public float? previousClose { get; set; }
        public float? eps { get; set; }
        public float? pe { get; set; }
        public DateTime? earningsAnnouncement { get; set; }
        public long? sharesOutstanding { get; set; }
        public int timestamp { get; set; }
    }
}
