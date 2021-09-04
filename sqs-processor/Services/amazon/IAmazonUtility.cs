using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace sqs_processor.Services.amazon
{
    interface IAmazonUtility
    {

         Task SendSNSMessage(string snsURL, string message);
    }
}
