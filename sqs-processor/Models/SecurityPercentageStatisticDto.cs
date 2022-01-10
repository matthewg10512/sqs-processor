using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Models
{
   public class SecurityPercentageStatisticDto
    {
        public int? SecurityId { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public decimal? AverageDrop { get; set; }
        public decimal? Percent5 { get; set; }
        public decimal? Percent10 { get; set; }
        public decimal? Percent15 { get; set; }
        public decimal? totalPercentSum { get; set; }
        public decimal? highLowRangeAverage { get; set; }
        public int? belowAverageCount { get; set; }

        public decimal? AvgDropLowAvg { get; set; }

        public decimal? AvgDropHighLowRangeAvg { get; set; }

    }
}
