using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Models
{
    public class PeakRangeDetailDto
    {
        public int SecurityId { get; set; }
        public string RangeName { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public int RangeCount { get; set; }
        public int RangeLength { get; set; }
        public int MaxRangeLength { get; set; }
        public DateTime MaxRangeDateStart { get; set; }
        public DateTime MaxRangeDateEnd { get; set; }



    }
}
