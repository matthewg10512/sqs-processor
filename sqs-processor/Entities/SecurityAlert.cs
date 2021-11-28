using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace sqs_processor.Entities
{
    [Display(Name = "SecurityAlerts")]
    public  class SecurityAlert
    {
        [Key]
        public int Id { get; set; }

        public int SecurityId { get; set; }
        public int alertType { get; set; }
        public DateTime dateRecorded { get; set; }

    }
}
