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

     
        public IEnumerable<Earning> GetEarnings(int securityId);

        public IEnumerable<EarningSecurityPercentage> GetEarningSecurityPercentage(int securityId);


        public List<EarningDto> GetEarnings(List<EarningDto> earnings);
        public IEnumerable<Earning> GetEarnings(EarningsResourceParameters earningsResourceParameters);



        public IEnumerable<Tuple<Earning, Security>> GetSecuritiesEarnings(IEnumerable<Earning> earnings);

        public IEnumerable<AutoSecurityTrade> GetSecurityTradeHistory(AutoSecurityTradesResourceParameters securityTradeHistoryResourceParameters);
        public IEnumerable<Tuple<AutoSecurityTrade, Security>> GetSecurityTradeHistorySecurities(IEnumerable<AutoSecurityTrade> securityTradeHistory);




        public void UpdateEarnings(List<EarningDto> earnings);
        public IEnumerable<Security> GetPreferredSecurities();
        public IEnumerable<Security> GetSecurities(SecuritiesResourceParameters securitiesResourceParameters);


       
        bool Save();

        public void UpdateSecurity(Security security);

        public List<StockPurchaseOption> GetPotentialBuys();
        public void UpdateEarnings(List<EarningDto> earnings, Security security);


        public void UpsertSecurityAnalytics(List<SecurityAnalyticDto> securityAnalytics);

        public void UpsertStockSplitHistory(List<StockSplitHistoryDto> stockSplits);
        
        public void UpsertHistoricalPrices(List<HistoricalPriceforUpdateDto> historicalPrices);
        public List<Security> GetCurrentPeakRanges();
        public List<HistoricalPrice> GetHistoricalPrices(int securityId, HistoricalPricesResourceParameters historicalPriceResourceParameters);
        public HistoricalPrice GetLastHistoricalPrice(int securityId);
        public HistoricalPrice GetFirstHistoricalPrice(int securityId);
        public void UpdatePercentageChangeHistoricPrice(int securityId);

        public void UpsertCurrentPeakRanges(List<CurrentPeakRangeDto> currentPeakRanges);
        public void UpsertPeakRangeDetails(List<PeakRangeDetailDto> peakRangeDetails);
        public List<HistoricalPriceCloseHistoricDateDto> GetHistoricalPricesCloseHistoricDate(int securityId, HistoricalPricesResourceParameters historicalPriceResourceParameters);
        public List<HistoricalPriceOpenHistoricDateDto> GetHistoricalPricesOpenHistoricDate(int securityId, HistoricalPricesResourceParameters historicalPriceResourceParameters);
        public void UpsertPriorPurchaseEstimates(List<PriorPurchaseEstimateDto> priorPurchaseEstimates);

        public void UpdateSecurities(List<SecurityForUpdateDto> securities);

        SecurityTask GetTasks(string taskName);
        void UpdateTasks(SecurityTask task);

        SecurityPercentageStatistic PercentageChangeGetTasks(string taskName);
        void PercentageChangeUpdateTasks(SecurityPercentageStatistic task);

        void UpsertSecurityPercentageStatistics(List<SecurityPercentageStatisticDto> securityPercentageStatistics);
        void UpsertSecurityPercentageStatisticsHistory(List<SecurityPercentageStatisticHistory> securityPercentageStatisticsHistory);
        List<SecurityPercentageStatisticHistory> GetSecurityPercentageStatisticsHistory(List<SecurityPercentageStatistic> securityPercentageStatistics);
        List<AutoSecurityTrade> GetRecommendedSecurityTrades(string securityTradeType);

        public List<StockPurchaseOption> GetStockScreenerResults(StockScreenerSearchResourceParameters stockPurOptResourceParams);
        public void UpdateSecurityTradeHistory(AutoSecurityTrade securityTradeHistory);
        bool SecurityTradesExists(AutoSecurityTrade securityTradeHistory);
        void AddSecurityTradeHistory(AutoSecurityTrade securityTradeHistory);

        public List<AutoSecurityTrade> ProcessAutoSecurityTrades(List<AutoSecurityTrade> securityTrades);

        public StockScreenerAlertType GetStockScreenerAlertType(int stockScreenerAlertTypeId);

        public List<StockScreenerAlertType> GetStockScreenerAlertTypes();

        public List<StockScreenerAlertsHistoryDto> GetNewStockScreenerAlertsHistory(List<Security> securities, int stockScreenerAlertTypeId);

        public void AddStockScreenerAlertsHistoryRecords(List<StockScreenerAlertsHistoryDto> stockScreenerAlertsHistoryRecords);

        public string ConvertStringScreenerAlertTypeMessage(List<StockScreenerAlertsHistoryDto> stockScreenerAlertsHistoryRecords, StockScreenerAlertType screenAlertsType);
        public bool IsMarketClosed(DateTime currentDate);

        public List<StockScreener> GetStockScreeners();
        public StockScreenerSearchResourceParameters GetStockScreenerSearchDetails(int stockScreenId);
        public IEnumerable<StockSplitHistory> GetStockSplitHistories(int securityId, StockSplitHistoriesResourceParameters stockSplitHistoryResourceParameters);


        public List<Security> SecurityAlertCheck(SecurityAlertType securityAlertType);
        public List<Security> GetCurrentSecurityPercentage();
        public string ConvertStringSecurityAlertCheck(List<Security> securities);

        public SecurityAlertType GetSecurityAlertType(int id);

        public bool SecurityAlertTradesExists(SecurityAlert securityAlert);
        public List<Security> ProcessSecurityAlerts(List<Security> securities, SecurityAlertType securityAlertType);


        public void UpsertSecurityProfile(List<SecurityForUpdateDto> securities);


    }
}
