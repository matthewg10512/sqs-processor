using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.ResourceParameters
{
    public class DividendsResourceParameters
    {
        public int securityId { get; set; }
        public DateTime exDividendDate { get; set; }
        public DateTime rangeExDividendDateStart { get; set; }
        public DateTime rangeExDividendDateEnd { get; set; }
        public string searchQuery { get; set; }
    }
}
