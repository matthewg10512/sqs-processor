
using Newtonsoft.Json;
using sqs_processor.Entities;
using sqs_processor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Services.Network.StockSplits
{
    public class GetStockSplitHistoryFromFMP : IGetStockSplitHistory
    {
        private string _apiKey;
        private readonly IWebClientWrapper _webClient;
        public GetStockSplitHistoryFromFMP(IWebClientWrapper webClient, string apiKey)
        {
            _webClient = webClient;
            _apiKey = apiKey;
        }

        public string GetStringHtml(SecurityIdSymbolDto security)
        {
            string url = "https://financialmodelingprep.com/api/v3/historical-price-full/stock_split/" + security.Symbol + "?apikey=" + _apiKey;
            return _webClient.GetHTMLString(url);
        }

        public List<StockSplitHistoryDto> TransformData(string html, SecurityIdSymbolDto security)
        {
            StockSplitFromApi securitiesFromApi;
            List<StockSplitHistoryDto> stockSplits = new List<StockSplitHistoryDto>();

            if (html == "")
            {
                return stockSplits;
            }
            securitiesFromApi = JsonConvert.DeserializeObject<StockSplitFromApi>(html);

            if (securitiesFromApi.historical != null && securitiesFromApi.historical.Length > 0)
            {
                HistoricalSplit[] historicalSplitApi = securitiesFromApi.historical;

                foreach (var row in historicalSplitApi)
                {

                    StockSplitHistoryDto stockSplit = new StockSplitHistoryDto()
                    {

                        SplitDate = DateTime.Parse(row.date),
                        SecurityId = security.Id,
                        SplitAmount = row.numerator,
                        ReverseSplitAmount = row.denominator

                    };
                    stockSplits.Add(stockSplit);

                    //}
                }

            }
            return stockSplits;
        }
    }
}
