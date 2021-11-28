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

        public DbSet<AutoSecurityTrade> AutoSecurityTrades { get; set; }
        public DbSet<HistoricalPrice> HistoricalPrices { get; set; }

        public DbSet<Dividend> Dividends { get; set; }
        public DbSet<Security> Securities { get; set; }

        public DbSet<Earning> Earnings { get; set; }

        public DbSet<SecurityTask> SecurityTasks { get; set; }

        public DbSet<tempSecurityAlerts> tempSecurityAlerts { get; set; }
        public DbSet<SecurityPercentageStatistic> SecurityPercentageStatistics { get; set; }
        public DbSet<SecurityAlertType> SecurityAlertTypes { get; set; }
        public DbSet<SecurityPurchaseCheck> SecurityPurchaseChecks { get; set; }

        public DbSet<PeakRangeDetail> PeakRangeDetails { get; set; }
        public DbSet<SecurityAlert> SecurityAlerts { get; set; }

        public DbSet<CurrentPeakRange> CurrentPeakRanges { get; set; }

        public void Save()
        {
            throw new NotImplementedException();



        }


    }
}
