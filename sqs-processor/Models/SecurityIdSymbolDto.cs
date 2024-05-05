using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace sqs_processor.Models
{
    public class SecurityIdSymbolDto
    {

        public int Id { get; set; }
        public string Symbol { get; set; }
       
    }
}
