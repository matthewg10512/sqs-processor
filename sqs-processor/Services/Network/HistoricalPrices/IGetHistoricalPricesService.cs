using sqs_processor.Entities;
using sqs_processor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Services.Network.HistoricalPrices
{
    public interface IGetHistoricalPricesService
    {
        public DateTime startRange { get; set; }
        public DateTime endRange { get; set; }
        string GetStringHtml(Security security);
        List<HistoricalPriceforUpdateDto> TransformData(string html, int securityId);
    }
}
