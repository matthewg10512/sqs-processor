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
        IDbSet<HistoricalPrice> HistoricalPrices { get; set; }
        IDbSet<Security> Securities { get; set; }
        IDbSet<AutoSecurityTrade> AutoSecurityTrades { get; set; }
        IDbSet<tempSecurityAlerts> tempSecurityAlerts { get; set; }
        DbSet<SecurityTask> SecurityTasks { get; set; }
        IDbSet<Dividend> Dividends { get; set; }
        IDbSet<Earning> Earnings { get; set; }
        IDbSet<SecurityAlertType> SecurityAlertTypes { get; set; }
        IDbSet<SecurityAlert> SecurityAlerts { get; set; }
        IDbSet<PriorPurchaseEstimate> PriorPurchaseEstimates { get; set; }
        IDbSet<PeakRangeDetail> PeakRangeDetails { get; set; }
        IDbSet<CurrentPeakRange> CurrentPeakRanges { get; set; }


        IDbSet<StockScreenerSearchDetail> StockScreenerSearchDetails { get; set; }
        IDbSet<StockScreener> StockScreeners { get; set; }
        IDbSet<ScreenerCriteria> ScreenerCriterias { get; set; }



        //  IDbSet<Earning> Earnings { get; set; }
        //List<HistoricalPrice> DapperHistoricalPrices { get; set; }
        void Save();
        DbSet<SecurityPercentageStatistic> SecurityPercentageStatistics { get; set; }
        DbSet<SecurityPercentageStatisticHistory> SecurityPercentageStatisticsHistory { get; set; }

        
    }
}
