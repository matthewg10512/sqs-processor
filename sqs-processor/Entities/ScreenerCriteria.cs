using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace sqs_processor.Entities
{
    [Display(Name = "ScreenerCriterias")]
    public  class ScreenerCriteria
    {
        [Key]
        public int id { get; set; }
        public string CriteriaName { get; set; }
        public string Description { get; set; }
        public string JSONObjectName { get; set; }
        public string ObjectType { get; set; }
    }
}
