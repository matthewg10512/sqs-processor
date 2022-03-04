using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Entities
{
    public class AuctionScriptStep
    {

         public int Id { get; set; }
        public int AuctionSiteId { get; set; }
        public int ActionGroupType { get; set; }
        public string JsCode { get; set; }
        public int StepOrder { get; set; }
        public int ScriptAction { get; set; }
        public int ThreadSleep { get; set; }


    }
}
