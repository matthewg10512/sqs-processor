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
        DbSet<SecurityTasks> SecurityTasks { get; set; }
        IDbSet<Dividend> Dividends { get; set; }
        IDbSet<Earning> Earnings { get; set; }
        //  IDbSet<HistoricalPrice> HistoricalPrices { get; set; }
        //  IDbSet<Earning> Earnings { get; set; }
        //List<HistoricalPrice> DapperHistoricalPrices { get; set; }
        void Save();
        DbSet<SecurityPercentageStatistics> SecurityPercentageStatistics { get; set; }

    }
}
