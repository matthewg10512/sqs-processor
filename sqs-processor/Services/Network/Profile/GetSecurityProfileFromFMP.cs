using Newtonsoft.Json;
using sqs_processor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Services.Network.Profile
{
    class GetSecurityProfileFromFMP : IGetSecurityProfile
    {

        private string _apiKey;
        private readonly IWebClientWrapper _webClient;
        public GetSecurityProfileFromFMP(IWebClientWrapper webClient, string apiKey)
        {
            _webClient = webClient;
            _apiKey = apiKey;
        }

        public string GetStringHtml(string symbol)
        {
            string url = "https://financialmodelingprep.com/api/v3/profile/" + symbol + "?apikey=" + _apiKey;
            return _webClient.GetHTMLString(url);
        }

        public List<SecurityForUpdateDto> TransformData(string html, string symbol, string exchange)
        {
            SecurityProfileFromApi[] securitiesFromApi;
            List<SecurityForUpdateDto> securities = new List<SecurityForUpdateDto>();

            if (html == "")
            {
                return securities;
            }
            securitiesFromApi = JsonConvert.DeserializeObject<SecurityProfileFromApi[]>(html);



            
            foreach (var row in securitiesFromApi)
            {
                SecurityForUpdateDto security = new SecurityForUpdateDto()
                {
                    Symbol = row.symbol,
                    Description = row.description,
                    IPODate = row.ipoDate.HasValue ? row.ipoDate.Value : DateTime.Now,
                    SecurityType = exchange,
                    LastModified = DateTime.Now,
                    IsEtf = row.isEtf
                    

                };
                securities.Add(security);

            }
            return securities;
        }
    }
}
