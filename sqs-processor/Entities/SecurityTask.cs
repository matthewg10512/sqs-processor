using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace sqs_processor.Entities
{
    [Display(Name = "SecurityTasks")]
    public class SecurityTask
    {
        [Key]
        public int Id { get; set; }
        public string TaskName { get; set; }
        public string TaskUrl { get; set; }
        public DateTime? LastTaskRun { get; set; }

    }
}
