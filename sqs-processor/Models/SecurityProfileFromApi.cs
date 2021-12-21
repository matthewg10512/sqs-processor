using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Models
{
    class SecurityProfileFromApi
    {
        public string symbol { get; set; }
        public string description { get; set; }
        public DateTime? ipoDate { get; set; }
    }
}
