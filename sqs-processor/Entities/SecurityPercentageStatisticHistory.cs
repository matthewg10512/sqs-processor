using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace sqs_processor.Entities
{
    [Display(Name = "SecurityPercentageStatisticsHistory")]
    public class SecurityPercentageStatisticHistory
    {

        [Key]
        public int Id { get; set; }
        public int? SecurityId { get; set; }
        public DateTime? DateCalculated { get; set; }
        public decimal? AverageDrop { get; set; }
        public decimal? Percent5 { get; set; }
        public decimal? Percent10 { get; set; }
        public decimal? Percent15 { get; set; }
        public decimal? totalPercentSum { get; set; }
        public decimal? highLowRangeAverage { get; set; }

    }
}
