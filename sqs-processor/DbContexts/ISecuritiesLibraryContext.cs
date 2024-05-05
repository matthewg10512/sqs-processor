using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Entity;
using sqs_processor.Entities;

namespace sqs_processor.DbContexts
{
    public interface ISecuritiesLibraryContext
    {
        // IDbSet<Dividend> Dividends { get; set; }
        IDbSet<HistoricPerformance> HistoricPerformances { get; set; }
        IDbSet<HistoricalPrice> HistoricalPrices { get; set; }
        IDbSet<Security> Securities { get; set; }
        IDbSet<AutoSecurityTrade> AutoSecurityTrades { get; set; }
        IDbSet<tempSecurityAlerts> tempSecurityAlerts { get; set; }
        DbSet<BullBearRun> BullBearRuns { get; set; }
        DbSet<CurrentBullBearRun> CurrentBullBearRuns { get; set; }
        DbSet<SecurityTask> SecurityTasks { get; set; }
        IDbSet<Dividend> Dividends { get; set; }
        IDbSet<AuctionItem> AuctionItems { get; set; }
        IDbSet<AuctionSite> AuctionSites { get; set; }
        IDbSet<AuctionSearchWord> AuctionSearchWords { get; set; }
        IDbSet<AuctionSearchSiteRun> AuctionSearchSiteRuns { get; set; }

        IDbSet<AuctionSiteCategoryWord> AuctionSiteCategoryWords { get; set; }
        IDbSet<AuctionCategorySite> AuctionCategorySites { get; set; }
        IDbSet<AuctionPageLoadCheck> AuctionPageLoadChecks { get; set; }

        
        IDbSet<AuctionScriptStep> AuctionScriptSteps { get; set; }
        IDbSet<Earning> Earnings { get; set; }
        IDbSet<SecurityAlertType> SecurityAlertTypes { get; set; }
        IDbSet<SecurityAlert> SecurityAlerts { get; set; }
        IDbSet<PriorPurchaseEstimate> PriorPurchaseEstimates { get; set; }
        IDbSet<PeakRangeDetail> PeakRangeDetails { get; set; }
        IDbSet<CurrentPeakRange> CurrentPeakRanges { get; set; }
        IDbSet<StockSplitHistory> StockSplitHistories { get; set; }

        IDbSet<StockScreenerAlertType> StockScreenerAlertTypes { get; set; }
        IDbSet<TradingHoliday> TradingHolidays { get; set; }
        
        IDbSet<StockScreenerAlertsHistory> StockScreenerAlertsHistory { get; set; }
        IDbSet<StockScreenerSearchDetail> StockScreenerSearchDetails { get; set; }
        IDbSet<StockScreener> StockScreeners { get; set; }
        IDbSet<ScreenerCriteria> ScreenerCriterias { get; set; }

        IDbSet<SecurityAnalytic> SecurityAnalytics { get; set; }

        //  IDbSet<Earning> Earnings { get; set; }
        //List<HistoricalPrice> DapperHistoricalPrices { get; set; }
        void Save();
        DbSet<SecurityPercentageStatistic> SecurityPercentageStatistics { get; set; }
        DbSet<SecurityPercentageStatisticHistory> SecurityPercentageStatisticsHistory { get; set; }

        
    }
}
