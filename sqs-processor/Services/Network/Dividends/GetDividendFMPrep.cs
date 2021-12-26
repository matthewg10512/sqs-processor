using Newtonsoft.Json;
using sqs_processor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Services.Network.Dividends
{
   public class GetDividendFMPrep : IGetDividendsServices
    {
        private readonly IWebClientWrapper _webClient;
        private string _apiKey;
        private string _url;
        bool futureSearch;
        public GetDividendFMPrep(IWebClientWrapper webClient, string apiKey)
        {
            _webClient = webClient;
            _apiKey = apiKey;
            _url = "";
            futureSearch = false;
        }


        public void SetFutureURL()
        {
            futureSearch = true;
            DateTime setDate = DateTime.Now;
            DateTime futureDate = setDate.AddDays(45);
            _url = "https://financialmodelingprep.com/api/v3/stock_dividend_calendar?from=" + setDate.ToString("yyyy-MM-dd") + "&to=" + futureDate.ToString("yyyy-MM-dd") + "&apikey=" + _apiKey;
        }


        public string GetStringHtml(string symbol, string SecurityType)
        {

            if (!futureSearch)
            {
                _url = "https://financialmodelingprep.com/api/v3/historical-price-full/stock_dividend/" + symbol + "?apikey=" + _apiKey;
            }
            return _webClient.GetHTMLString(_url);

        }

        public List<DividendDto> TransformData(string html, int securityId)
        {
            Historical[] dividendsFromApi;
            string symbol = "";
            if (!futureSearch)
            {
                DividendFromApi details = JsonConvert.DeserializeObject<DividendFromApi>(html);
                symbol = details.symbol;
                dividendsFromApi = details.historical;
            }
            else
            {
                dividendsFromApi = JsonConvert.DeserializeObject<Historical[]>(html);
            }


            if(dividendsFromApi == null || dividendsFromApi.Length == 0)
            {
                return new List<DividendDto>();
            }
            List<DividendDto> dividends = new List<DividendDto>();
            foreach (var row in dividendsFromApi)
            {

                if (row.declarationDate == ""
                    || row.paymentDate == ""
                    || row.recordDate == ""
                    || row.paymentDate == ""
                    || row.declarationDate == null


                    )
                {
                    continue;
                }
                DateTime dateMin = System.Data.SqlTypes.SqlDateTime.MinValue.Value; ;//DateTime.Now.AddDays(-100).Date;
                DividendDto divModel = new DividendDto()
                {
                    AnnouncementDate = row.declarationDate != null ? DateTime.Parse(row.declarationDate) : dateMin,

                    Amount = Decimal.Round((decimal)(row.dividend.HasValue ? row.dividend : 0), 2),
                    Yield = Decimal.Round((decimal)(row.adjDividend.HasValue ? row.adjDividend : 0), 2),
                    ExDividendDate = row.paymentDate != null ? DateTime.Parse(row.paymentDate) : dateMin,
                    RecordDate = row.recordDate != null ? DateTime.Parse(row.recordDate) : dateMin,
                    PayableDate = row.paymentDate != null ? DateTime.Parse(row.paymentDate) : dateMin,
                    SecurityId = securityId,
                    symbol = symbol != "" ? symbol : row.symbol

                };

                dividends.Add(divModel);
                //_dividends.Execute(divModel);
                // _earnings.Save();


                //annouce date
                //  frequency
                //                    amount
                //                  yield
                //                ex dividend date
                //              record date
                //            payable date

            }
            return dividends;
        }
    }
}
