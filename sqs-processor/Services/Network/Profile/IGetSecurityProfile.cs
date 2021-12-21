using sqs_processor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Services.Network.Profile
{
   public interface IGetSecurityProfile
    {
        string GetStringHtml(string symbol);
        List<SecurityForUpdateDto> TransformData(string html, string symbol, string exchange);
    }
}
