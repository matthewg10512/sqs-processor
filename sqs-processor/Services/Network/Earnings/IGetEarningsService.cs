using sqs_processor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Services.Network.Earnings
{
    public interface IGetEarningsService
    {

        void SetURL(string urlType);
        string GetStringHtml(string symbol, string SecurityType);
        List<EarningDto> TransformData(string html, int securityId);
    }
}
    