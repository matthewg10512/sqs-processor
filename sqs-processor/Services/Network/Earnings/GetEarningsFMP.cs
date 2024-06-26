﻿using Newtonsoft.Json;
using sqs_processor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Services.Network.Earnings
{
    class GetEarningsFMP : IGetEarningsService
    {
        private readonly IWebClientWrapper _webClient;
        private string _apiKey;
        public string _url;
        public GetEarningsFMP(IWebClientWrapper webClient, string apiKey)
        {
            _webClient = webClient;
            _url = "";
            _apiKey = apiKey;
        }

        public string GetStringHtml(string symbol, string SecurityType)
        {

            if (_url == "")
            {
                _url = "https://financialmodelingprep.com/api/v3/historical/earning_calendar/" + symbol + "?apikey=" + _apiKey;
            }

            return _webClient.GetHTMLString(_url);

        }

        public void SetURL(string urlType)
        {
            if (urlType == "")
            {
                _url = "";
                return;
            }
            DateTime setDate = DateTime.Now;
            DateTime futureDate = setDate.AddDays(45);
            _url = "https://financialmodelingprep.com/api/v3/earning_calendar?from=" + setDate.ToString("yyyy-MM-dd") + "&to=" + futureDate.ToString("yyyy-MM-dd") + "&apikey=" + _apiKey;
        }


        public List<EarningDto> TransformData(string html, int securityId)
        {
            List<EarningDto> earnings = new List<EarningDto>();
            if (html == "")
            {
                return earnings;
            }
            var earningsFromApi = JsonConvert.DeserializeObject<List<EarningFromApi>>(html);

            

            foreach (var row in earningsFromApi)
            {

                if (row.date == ""
                    || row.symbol == ""
                    )
                {
                    continue;
                }
                EarningDto divModel = new EarningDto()
                {
                    ActualEarningsDate = DateTime.Parse(row.date),
                    EPSEstimate = Decimal.Round((decimal)(row.epsEstimated.HasValue ? row.epsEstimated : 0), 2),
                    ReportedEPS = Decimal.Round((decimal)(row.eps.HasValue ? row.eps : 0), 2),
                    ReportTime = row.time,
                    ActualRevenue = Decimal.Round((decimal)(row.revenue.HasValue ? row.revenue : 0), 2),
                    RevenueEstimate = Decimal.Round((decimal)(row.revenueEstimated.HasValue ? row.revenueEstimated: 0), 2),
                    SecurityId = securityId,
                    symbol = row.symbol

                };

                earnings.Add(divModel);

            }
            return earnings;

        }
    }
}
