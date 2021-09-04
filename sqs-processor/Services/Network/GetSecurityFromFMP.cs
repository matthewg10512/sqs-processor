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
                    DayHigh = (decimal)(row.dayHigh.HasValue ? row.dayHigh.Value : 0),
                    DayLow = (decimal)(row.dayLow.HasValue ? row.dayLow.Value : 0),
                    YearHigh = (decimal)(row.yearHigh.HasValue ? row.yearHigh.Value : 0),
                    YearLow = (decimal)(row.yearLow.HasValue ? row.yearLow.Value : 0),
                    CurrentPrice = (decimal)(row.price.HasValue ? row.price.Value : 0),
                    EarningsDate = row.earningsAnnouncement,
                    Volume = (int)(row.volume.HasValue ? row.volume.Value : 0),
                    PriorDayOpen = (decimal)(row.previousClose.HasValue ? row.previousClose.Value : 0),
                    LastModified = DateTime.Now,
                    SecurityType = row.exchange,
                    PercentageChange = (decimal)(row.changesPercentage.HasValue ? row.changesPercentage.Value : 0),
                };
                securities.Add(security);
                
            }
            return securities;
        }
    }
}
