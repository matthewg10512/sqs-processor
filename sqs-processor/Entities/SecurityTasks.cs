using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Entities
{
    public class SecurityTasks
    {
        public int Id { get; set; }
        public string TaskName { get; set; }
        public string TaskUrl { get; set; }
        public DateTime? LastTaskRun { get; set; }

    }
}
