using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Entities
{
    public class SecurityPercentageStatistics
    {
        public int Id { get; set; }
        public int? SecurityId { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public decimal? AverageDrop { get; set; }
        public decimal? Percent5 { get; set; }
        public decimal? Percent10 { get; set; }
        public decimal? Percent15 { get; set; }


    }
}
