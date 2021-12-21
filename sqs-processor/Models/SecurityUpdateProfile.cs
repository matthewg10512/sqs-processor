using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Models
{
   public class SecurityUpdateProfile
    {
        public int Id { get; set; }
        public string Symbol { get; set; }
        public string Description { get; set; }
        public DateTime? IPODate { get; set; }
    }
}
