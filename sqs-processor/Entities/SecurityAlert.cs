using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Entities
{
  public  class SecurityAlert
    {
        public int Id { get; set; }

        public int SecurityId { get; set; }
        public int alertType { get; set; }
        public DateTime dateRecorded { get; set; }

    }
}
