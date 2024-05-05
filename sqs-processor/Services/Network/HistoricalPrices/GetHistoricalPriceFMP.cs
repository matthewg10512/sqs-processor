using Newtonsoft.Json;
using sqs_processor.Entities;
using sqs_processor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Services.Network.HistoricalPrices
{
    public class GetHistoricalPriceFMP : IGetHistoricalPricesService
    {

        public DateTime startRange { get; set; }
        public DateTime endRange { get; set; }

        private readonly IWebClientWrapper _webClient;
        private string _apiKey;
        private string _url;
        public GetHistoricalPriceFMP(IWebClientWrapper webClient, string apiKey)
        {
            _webClient = webClient;
            _url = "";
            _apiKey = apiKey;
        }

        public string GetStringHtml(SecurityIdSymbolDto security)
        {
            /*
            DateTime setDate = DateTime.Now;
            DateTime backSearchDate = DateTime.Now.AddDays(-30);
            string beforeDate = "2010-03-12";
            if (!security.preferred)
            {
                beforeDate = DateTime.Now.AddDays(-2500).ToString("yyyy-MM-dd");
            }
            */
            _url = "https://financialmodelingprep.com/api/v3/historical-price-full/" + security.Symbol + "?from=" + startRange.ToString("yyyy-MM-dd") + "&to=" + endRange.ToString("yyyy-MM-dd") + "&apikey=" + _apiKey;


            return _webClient.GetHTMLString(_url);

        }

        //https://financialmodelingprep.com/api/v3/historical-price-full/AAPL?from=2010-03-12&to=2021-03-12&apikey=35189ec3acb016ecbc2aef1cfb2a2c22


        public List<HistoricalPriceforUpdateDto> TransformData(string html, int securityId)
        {


            Historical[] historicalPriceFromApi;
            //var details = JsonConvert.DeserializeObject<HistoricalPricesFromApi>(html).historical; 
            List<HistoricalPriceforUpdateDto> historicalPrices = new List<HistoricalPriceforUpdateDto>();
            if (html == "")
            {
                return historicalPrices;
                    
            }
            historicalPriceFromApi = JsonConvert.DeserializeObject<HistoricalPricesFromApi>(html).historical;


            
            if(historicalPriceFromApi == null)
            {
                return historicalPrices;
            }
            foreach (var row in historicalPriceFromApi)
            {

                if (row.date == ""
                    || row.open == 0
                    || row.high == 0
                    || row.close == 0
                    || row.volume == 0
                    || !(row.changePercent.HasValue)
                    || !(row.changeOverTime.HasValue)

                    )
                {
                    continue;
                }
                DateTime dateMin = System.Data.SqlTypes.SqlDateTime.MinValue.Value; ;//DateTime.Now.AddDays(-100).Date;
                HistoricalPriceforUpdateDto divModel = new HistoricalPriceforUpdateDto()
                {
                    HistoricDate = row.date != null ? DateTime.Parse(row.date) : dateMin,
                    Open = Decimal.Round((decimal)row.open, 2),
                    High = Decimal.Round((decimal)row.high, 2),
                    Low = Decimal.Round((decimal)row.low, 2),
                    Close = Decimal.Round((decimal)row.close, 2),
                    Volume = (int)row.volume,
                    SecurityId = securityId,
                    PercentChange = Decimal.Round((decimal)row.changePercent, 2)

                };




                historicalPrices.Add(divModel);


            }
            return historicalPrices;

        }

    }
}
