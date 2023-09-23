using Microsoft.EntityFrameworkCore;
using sqs_processor.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.DbContexts
{
    public class SecuritiesLibraryContext : DbContext
    {



        public SecuritiesLibraryContext(DbContextOptions<SecuritiesLibraryContext> options)
           : base(options)
        {

        }

        public DbSet<AuctionSearchWord> AuctionSearchWords { get; set; }
        public DbSet<AuctionSite> AuctionSites { get; set; }
        public DbSet<AuctionItem> AuctionItems { get; set; }
        public DbSet<AuctionSearchSiteRun> AuctionSearchSiteRuns { get; set; }
        public DbSet<AutoSecurityTrade> AutoSecurityTrades { get; set; }
        public DbSet<HistoricalPrice> HistoricalPrices { get; set; }

        public DbSet<AuctionCategorySite> AuctionCategorySites { get; set; }
        public DbSet<Dividend> Dividends { get; set; }
        public DbSet<Security> Securities { get; set; }

        public DbSet<Earning> Earnings { get; set; }
        public DbSet<StockScreenerAlertType> StockScreenerAlertTypes { get; set; }
        public DbSet<SecurityTask> SecurityTasks { get; set; }

        public DbSet<StockScreenerAlertsHistory> StockScreenerAlertsHistory { get; set; }

        public DbSet<tempSecurityAlerts> tempSecurityAlerts { get; set; }
        public DbSet<SecurityPercentageStatistic> SecurityPercentageStatistics { get; set; }
        public DbSet<SecurityAlertType> SecurityAlertTypes { get; set; }
        public DbSet<PriorPurchaseEstimate> PriorPurchaseEstimates { get; set; }

        public DbSet <StockSplitHistory> StockSplitHistories { get; set; }
        public DbSet<PeakRangeDetail> PeakRangeDetails { get; set; }
        public DbSet<SecurityAlert> SecurityAlerts { get; set; }

        public DbSet<CurrentPeakRange> CurrentPeakRanges { get; set; }

        public DbSet<TradingHoliday> TradingHolidays { get; set; }
        public DbSet<StockScreenerSearchDetail> StockScreenerSearchDetails { get; set; }
        public DbSet<StockScreener> StockScreeners { get; set; }
        public DbSet<ScreenerCriteria> ScreenerCriterias { get; set; }

        public DbSet<AuctionScriptStep> AuctionScriptSteps { get; set; }

        public DbSet<AuctionSiteCategoryWord> AuctionSiteCategoryWords { get; set; }

        public DbSet<AuctionPageLoadCheck> AuctionPageLoadChecks { get; set; }
        public DbSet<SecurityAnalytic> SecurityAnalytics { get; set; }
        public void Save()
        {
            throw new NotImplementedException();



        }


    }
}
