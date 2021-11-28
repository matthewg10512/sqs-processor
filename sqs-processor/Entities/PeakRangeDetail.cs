using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace sqs_processor.Entities
{
    [Display(Name = "PeakRangeDetails")]
    public class PeakRangeDetail
    {

        [Key]
        public int Id { get; set; }
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
