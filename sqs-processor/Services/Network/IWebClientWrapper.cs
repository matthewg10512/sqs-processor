using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Services.Network
{
    public interface IWebClientWrapper
    {

        string GetCSVStringAsync(string url);
        string GetHTMLString(string url);
    }
}
