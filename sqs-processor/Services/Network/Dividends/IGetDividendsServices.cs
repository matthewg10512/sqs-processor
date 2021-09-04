using sqs_processor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Services.Network.Dividends
{
    public interface IGetDividendsServices
    {
        void SetFutureURL();
        string GetStringHtml(string symbol, string SecurityType);
        List<DividendDto> TransformData(string html, int securityId);
    }
}
