using sqs_processor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Services.Network
{
    interface IGetSecurityService
    {
        string GetStringHtml(string symbol);
        List<SecurityForUpdateDto> TransformData(string html, string symbol);
    }
}
