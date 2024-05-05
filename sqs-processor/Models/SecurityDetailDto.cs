using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace sqs_processor.Models
{
    public class SecurityDetailDto
    {

        public int Id { get; set; }
        public string Description { get; set; }

        public string Symbol { get; set; }
        public string SecurityType { get; set; }


    }
}
