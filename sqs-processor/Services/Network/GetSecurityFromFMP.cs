using Newtonsoft.Json;
using sqs_processor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Services.Network
{
   public  class GetSecurityFromFMP : IGetSecurityService
    {
        private string _apiKey;
        private readonly IWebClientWrapper _webClient;
        public GetSecurityFromFMP(IWebClientWrapper webClient, string apiKey)
        {
            _webClient = webClient;
            _apiKey = apiKey;
        }


        public string GetStringHtml(string symbol)
        {

            string url = "https://financialmodelingprep.com/api/v3/quotes/" + symbol + "?apikey=" + _apiKey;
            return _webClient.GetHTMLString(url);
        }


        public List<SecurityForUpdateDto> TransformData(string html, string symbol)
        {
            SecurityFromApi[] securitiesFromApi;

            securitiesFromApi = JsonConvert.DeserializeObject<SecurityFromApi[]>(html);



            List<SecurityForUpdateDto> securities = new List<SecurityForUpdateDto>();
            foreach (var row in securitiesFromApi)
            {
                SecurityForUpdateDto security = new SecurityForUpdateDto()
                {
                    Name = row.name,
                    Symbol = row.symbol,
                    DayHigh = (decimal)(row.dayHigh.HasValue ? Math.Round(row.dayHigh.Value,2) : 0),
                    DayLow = (decimal)(row.dayLow.HasValue ? Math.Round(row.dayLow.Value, 2) : 0),
                    YearHigh = (decimal)(row.yearHigh.HasValue ? Math.Round(row.yearHigh.Value, 2) : 0),
                    YearLow = (decimal)(row.yearLow.HasValue ? Math.Round(row.yearLow.Value, 2) : 0),
                    CurrentPrice = (decimal)(row.price.HasValue ? Math.Round(row.price.Value, 2) : 0),
                    EarningsDate = row.earningsAnnouncement,
                    Volume = (int)(row.volume.HasValue ? row.volume.Value : 0),
                    PriorDayOpen = (decimal)(row.previousClose.HasValue ? Math.Round(row.previousClose.Value,2) : 0),
                    LastModified = DateTime.Now,
                    SecurityType = row.exchange,
                    PercentageChange = (decimal)(row.changesPercentage.HasValue ? Math.Round(row.changesPercentage.Value,2) : 0),
                };
                securities.Add(security);
                
            }
            return securities;
        }
    }
}
