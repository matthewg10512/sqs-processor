using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Entities
{
   public class TradingHoliday
    {
        public int id { get; set; }
        public DateTime HolidayDate { get; set; }
    }
}
