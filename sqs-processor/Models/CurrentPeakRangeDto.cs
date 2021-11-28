using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace sqs_processor.Models
{
    public class CurrentPeakRangeDto
    {
        public int SecurityId { get; set; }
        public string RangeName { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public int RangeLength { get; set; }
        public DateTime RangeDateStart { get; set; }
        [Column(TypeName = "decimal(5,2)")]
        public decimal PeakRangeCurrentPercentage { get; set; }
    }
}
