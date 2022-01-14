using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace sqs_processor.Entities
{
    [Display(Name = "StockScreenerSearchDetails")]
    public  class StockScreenerSearchDetail
    {
        [Key]
        public int id { get; set; }
        public string SearchValue { get; set; }
        public int StockScreenerId { get; set; }
        public int ScreenerCriteriaId { get; set; }


    }
}
