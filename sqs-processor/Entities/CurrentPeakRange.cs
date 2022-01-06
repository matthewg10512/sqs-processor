using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace sqs_processor.Entities
{
    [Display(Name = "CurrentPeakRanges")]
    public class CurrentPeakRange
    {
        [Key]
        public int Id { get; set; }
        public int SecurityId { get; set; }
        public string RangeName { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public int RangeLength { get; set; }
        public DateTime RangeDateStart { get; set; }
        [Column(TypeName = "decimal(5,2)")]
        public decimal PeakRangeCurrentPercentage { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? LastOpenHigh { get; set; }
    }
}
