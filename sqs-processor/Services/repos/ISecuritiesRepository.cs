using sqs_processor.Entities;
using sqs_processor.Models;
using sqs_processor.ResourceParameters;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Services.repos
{
    public interface ISecuritiesRepository
    {

        Security GetSecurity(int securityId);

        bool SecurityExists(int securityId);

        IEnumerable<Dividend> GetDividends(int securityId);

        Dividend GetDividend(int securityId, int dividendId);
        public IEnumerable<Earning> GetEarnings(int securityId);

        public IEnumerable<EarningSecurityPercentage> GetEarningSecurityPercentage(int securityId);


        public List<EarningDto> GetEarnings(List<EarningDto> earnings);
        public IEnumerable<Earning> GetEarnings(EarningsResourceParameters earningsResourceParameters);
        public IEnumerable<Dividend> GetDividends(DividendsResourceParameters dividendsResourceParameters);



        public IEnumerable<Tuple<Earning, Security>> GetSecuritiesEarnings(IEnumerable<Earning> earnings);

        public IEnumerable<AutoSecurityTrade> GetSecurityTradeHistory(AutoSecurityTradesResourceParameters securityTradeHistoryResourceParameters);
        public IEnumerable<Tuple<AutoSecurityTrade, Security>> GetSecurityTradeHistorySecurities(IEnumerable<AutoSecurityTrade> securityTradeHistory);


        public IEnumerable<Tuple<Dividend, Security>> GetSecuritiesDividends(IEnumerable<Dividend> dividends);


        public void UpdateEarnings(List<EarningDto> earnings);
        public IEnumerable<Security> GetPreferredSecurities();
        public IEnumerable<Security> GetSecurities(SecuritiesResourceParameters securitiesResourceParameters);


        public List<DividendDto> GetDividends(List<DividendDto> dividends);
        bool Save();

        public void UpdateSecurity(Security security);

        public void UpdateDividends(List<DividendDto> dividends);
        public void UpdateDividends(List<DividendDto> dividends, Security security);
        public void UpdateEarnings(List<EarningDto> dividends, Security security);


        public void UpsertHistoricalPrices(List<HistoricalPriceforUpdateDto> historicalPrices, Security security);

        public IEnumerable<HistoricalPrice> GetHistoricalPrices(int securityId, HistoricalPricesResourceParameters historicalPriceResourceParameters);



        public void UpdateSecurities(List<SecurityForUpdateDto> securities);

        SecurityTasks GetTasks(string taskName);
        void UpdateTasks(SecurityTasks task);

        SecurityPercentageStatistics PercentageChangeGetTasks(string taskName);
        void PercentageChangeUpdateTasks(SecurityPercentageStatistics task);
        List<AutoSecurityTrade> GetRecommendedSecurityTrades(string securityTradeType);

        public void UpdateSecurityTradeHistory(AutoSecurityTrade securityTradeHistory);
        bool SecurityTradesExists(AutoSecurityTrade securityTradeHistory);
        void AddSecurityTradeHistory(AutoSecurityTrade securityTradeHistory);

        public List<AutoSecurityTrade> ProcessAutoSecurityTrades(List<AutoSecurityTrade> securityTrades);

        public List<Security> SecurityAlertCheck(SecurityAlertType securityAlertType);

        public string ConvertStringSecurityAlertCheck(List<Security> securities);

        public SecurityAlertType GetSecurityAlertType(int id);

        public bool SecurityAlertTradesExists(SecurityAlert securityAlert);
        public List<Security> ProcessSecurityAlerts(List<Security> securities, SecurityAlertType securityAlertType);

    }
}
