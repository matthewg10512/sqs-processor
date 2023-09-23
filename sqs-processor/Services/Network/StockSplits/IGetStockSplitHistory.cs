using sqs_processor.Entities;
using sqs_processor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Services.Network.StockSplits
{
   public interface IGetStockSplitHistory
    {
        string GetStringHtml(Security security);
        List<StockSplitHistoryDto> TransformData(string html, Security security);
    }
}
