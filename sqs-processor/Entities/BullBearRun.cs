using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sqs_processor.Entities
{
    [Display(Name = "BullBearRuns")]
    public class BullBearRun
    {
        [Key]
        public int Id { get;set; }
        public int SecurityId { get; set; }

        public int RunType { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }

        public DateTime RunStartDate { get; set; }

        public DateTime RunEndDate { get; set; }
        public DateTime HighDate { get; set; }
        public DateTime LowDate { get; set; }


        [Column(TypeName = "decimal(18,2)")]
        public decimal? StartRunPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? EndRunPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? LowPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? HighPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal PercentRangeCheck { get; set; }

    }
}
