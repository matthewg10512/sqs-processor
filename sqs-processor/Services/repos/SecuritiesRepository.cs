using AutoMapper;
using Microsoft.EntityFrameworkCore;
using sqs_processor.DbContexts;
using sqs_processor.Entities;
using sqs_processor.Models;
using sqs_processor.ResourceParameters;
using sqs_processor.Services.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace sqs_processor.Services.repos
{

    public class SecurityPercentStatJoin
    {

        public Security security { get; set; }

        public SecurityPercentageStatistic secPerStats { get; set; }
        public decimal? PercentDrop { get; set; }
        public decimal? StatPercentDrop { get; set; }
    }


    public class SecuritiesRepository : ISecuritiesRepository, IDisposable
    {
       // private readonly IConfiguration _config;

        private readonly SecuritiesLibraryContext _context;
        private readonly IMapper _mapper;
        private readonly IUtility _utility;
        public SecuritiesRepository(SecuritiesLibraryContext context, 
            //IConfiguration config,
            IMapper mapper,IUtility utility)
        {

            _context = context ?? throw new ArgumentNullException(nameof(context));
           // _config = config ?? throw new ArgumentNullException(nameof(config));
            _mapper = mapper;
            _utility = utility;
        }

      
        public void Dispose()
        {
            //  throw new NotImplementedException();
        }

        public bool SecurityExists(int securityId)
        {
            if (securityId == 0)
            {
                throw new ArgumentNullException(nameof(securityId));
            }

            return _context.Securities.Any(a => a.Id == securityId);
        }

      




        public Security GetSecurity(int securityId)
        {
            if (securityId == 0)
            {
                throw new ArgumentNullException(nameof(securityId));
            }

            // return _context.Authors.FirstOrDefault(a => a.Id == authorId);

            return _context.Securities.FirstOrDefault(x => x.Id == securityId);
        }


        public void UpdateSecurity(Security security)
        {
            _context.Update(security);
        }

        public IEnumerable<Security> GetPreferredSecurities()
        {
            return _context.Securities.Where(x => x.preferred == true).ToList();

        }

        public IEnumerable<Earning> GetEarnings(EarningsResourceParameters earningsResourceParameters)
        {
            if (earningsResourceParameters.actualEarningsDate == DateTime.MinValue
                && earningsResourceParameters.rangeStartEarningsDate == DateTime.MinValue
                && earningsResourceParameters.rangeEndEarningsDate == DateTime.MinValue
                && string.IsNullOrWhiteSpace(earningsResourceParameters.searchQuery)
                && earningsResourceParameters.securityId == 0

                    )
            {
                return _context.Earnings.ToList<Earning>();
            }


            var collection = _context.Earnings as IQueryable<Earning>;



            if (earningsResourceParameters.actualEarningsDate != DateTime.MinValue)
            {

                collection = collection.Where(a => a.ActualEarningsDate == earningsResourceParameters.actualEarningsDate);
            }

            if (earningsResourceParameters.rangeStartEarningsDate != DateTime.MinValue)
            {

                collection = collection.Where(a => a.ActualEarningsDate != null && a.ActualEarningsDate >= earningsResourceParameters.rangeStartEarningsDate);
            }

            if (earningsResourceParameters.rangeEndEarningsDate != DateTime.MinValue)
            {

                collection = collection.Where(a => a.ActualEarningsDate != null && a.ActualEarningsDate <= earningsResourceParameters.rangeEndEarningsDate);
            }


            if (earningsResourceParameters.securityId != 0)
            {

                collection = collection.Where(a => a.SecurityId == earningsResourceParameters.securityId);
            }


            return collection.ToList();


            /*
            //return _context.Earnings.Where(x => x.ActualEarningsDate > earningsResourceParameters.actualEarningsDate).Join(_context.PreferredSecurities, x => x.StockId, security => security.Id, (x, security) => new { x, security }).Select(x =>  x.x }).ToList();
            return _context.Earnings.Where(x => x.ActualEarningsDate > earningsResourceParameters.actualEarningsDate)
               // .Join(_context.PreferredSecurities, x => x.StockId, security => security.StockId, (x, security) => new { x, security })
                .Join(_context.Securities.Where(x=>x.preferred==true), x => x.SecurityId, security => security.Id , (x,security) => new { x,security}).Select(x =>  new Tuple<Earning,Security>(x.x , x.security ) ).ToList();
            */
        }


        public IEnumerable<EarningSecurityPercentage> GetEarningSecurityPercentage(int securityId)
        {
            List<EarningSecurityPercentage> earningPercentage = new List<EarningSecurityPercentage>();
            var cteInfo = _context.HistoricalPrices.Where(x => x.SecurityId == securityId).Join(_context.Earnings,
                 x => x.SecurityId, y => y.SecurityId, (query1, query2) => new { query1, query2 }
                ).Where(x => x.query1.SecurityId == securityId && (
                EF.Functions.DateDiffDay(x.query1.HistoricDate, x.query2.ActualEarningsDate) == 0
                ||
                EF.Functions.DateDiffDay(x.query1.HistoricDate, x.query2.ActualEarningsDate) == -1
                // x.query1.HistoricDate == x.query2.ActualEarningsDate
                //||
                //System.Data.Entity.DbFunctions.AddDays(x.query1.HistoricDate,1) == x.query2.ActualEarningsDate
                )


                ).ToList().OrderBy(x => x.query1.HistoricDate).GroupBy(x => x.query2.ActualEarningsDate).Select(g => new { g, count = g.Count() })
               .SelectMany(t => t.g.Select(b => b).Zip(Enumerable.Range(1, t.count), (j, i) => new {
                   j.query2.ActualEarningsDate,
                   j.query1.HistoricDate,
                   j.query2.ReportTime,
                   j.query2.Id,
                   j.query1.SecurityId,
                   j.query1.PercentChange,
                   rn = i
               })).ToList();

            for (int i = 0; i < cteInfo.Count; i++)
            {
                if (cteInfo[i].ReportTime == "amc")
                {
                    if (cteInfo[i].rn == 2)
                    {
                        earningPercentage.Add(new EarningSecurityPercentage()
                        {
                            Id = cteInfo[i].Id,
                            SecurityId = cteInfo[i].SecurityId,
                            PercentageChange = cteInfo[i].PercentChange,
                            ReportTime = cteInfo[i].ReportTime,
                            ActualEarningsDate = cteInfo[i].ActualEarningsDate,
                            HistoricDate = cteInfo[i].HistoricDate
                        });
                    }
                }
                else
                {
                    if (cteInfo[i].rn == 1)
                    {
                        earningPercentage.Add(new EarningSecurityPercentage()
                        {
                            Id = cteInfo[i].Id,
                            SecurityId = cteInfo[i].SecurityId,
                            PercentageChange = cteInfo[i].PercentChange,
                            ReportTime = cteInfo[i].ReportTime,
                            ActualEarningsDate = cteInfo[i].ActualEarningsDate,
                            HistoricDate = cteInfo[i].HistoricDate
                        });
                    }
                }

            }
            /*
            var o = beatles.OrderBy(x => x.id).GroupBy(x => x.inst)
               .Select(g => new { g, count = g.Count() })
               .SelectMany(t => t.g.Select(b => b)
                                   .Zip(Enumerable.Range(1, t.count), (j, i) => new { j.inst, j.name, rn = i }));

            */
            return earningPercentage;
        }

        public IEnumerable<Earning> GetEarnings(int securityId)
        {
            if (securityId == 0)
            {
                throw new ArgumentNullException(nameof(securityId));
            }


            return _context.Earnings.Where(x => x.SecurityId == securityId).ToList();
        }


        public List<EarningDto> GetEarnings(List<EarningDto> earnings)
        {


            var security = _context.Securities.Select(x => new Security { Symbol = x.Symbol, Id = x.Id });

            earnings = earnings.Join(security, x => x.symbol, y => y.Symbol, (query1, query2) => new { query1, query2 }).Select(x => new EarningDto
            {

                SecurityId = x.query2.Id,
                symbol = x.query1.symbol,
                ActualEarningsDate = x.query1.ActualEarningsDate,
                ActualRevenue = x.query1.ActualRevenue,
                EPSEstimate = x.query1.EPSEstimate,
                ReportedEPS = x.query1.ReportedEPS,
                ReportTime = x.query1.ReportTime,
                RevenueEstimate = x.query1.RevenueEstimate,



            }).ToList();
            // .Join(historicalPrice, x => x.Id, y => y.StockId, (query1, query2) => new { query1, query2 })
            //           .Where(o => currentDay == o.query2.HistoricDate).Select(x => x.query1);


            /*
                        for (int i =0;i< earnings.Count; i++)
                        {
                            var security = _context.Securities.FirstOrDefault(x => x.Symbol == earnings[i].symbol);
                            if(security != null)
                            {
                                earnings[i].StockId = security.Id;
                            }
                        }
                        earnings = earnings.Where(x => x.StockId > 0).ToList();
            */
            return earnings;
        }



       
        public List<StockPurchaseOption> GetPotentialBuys()
        {
            var lastModified = DateTime.Now.AddDays(-3);
            var details = _context.Securities.Join(_context.SecurityPercentageStatistics, x => x.Id, y => y.SecurityId, (security, secPerStats) => new { security, secPerStats })
                 .Where(x => x.security.LastModified > lastModified &&
                 x.secPerStats.AverageDrop != 0 &&
                 x.security.Volume > 100000 &&
                 x.security.PercentageChange < 0 &&
                 ((x.security.DayLow - x.security.PriorDayOpen) / x.security.PriorDayOpen) * 100 < x.secPerStats.AverageDrop * (decimal) 1.5
                 //x.security.PercentageChange < x.secPerStats.AverageDrop * (decimal)1.5
                 )
                 //.ToList()
                 .Join(_context.CurrentPeakRanges, x => x.security.Id, y => y.SecurityId, (s, curPeakRange) => new { s, curPeakRange })
                 .Join(_context.PriorPurchaseEstimates, x => x.s.security.Id, y => y.SecurityId, (s, spc) => new { s, spc }).Where(x=>x.spc.Shares > 60 
                 && ( (((x.spc.Shares *  x.s.s.security.CurrentPrice) - x.spc.PurchasePrice) / x.spc.PurchasePrice) * 100) / (x.spc.Shares / 12) > 10


                 ).Select(x=> new StockPurchaseOption {
                 Security = x.s.s.security,
                 SecurityPercentageStatistic = x.s.s.secPerStats,
                 CurrentPeakRange = x.s.curPeakRange,
                 PriorPurchaseEstimate = x.spc

                 }).ToList();
            return details;
        }



        public Earning GetEarning(int securityId, DateTime ActualEarningsDate)
        {

            if (securityId == 0)
            {
                throw new ArgumentNullException(nameof(securityId));
            }


            return _context.Earnings
              .Where(c => c.SecurityId == securityId && c.ActualEarningsDate == ActualEarningsDate).FirstOrDefault();
        }

        public bool Save()
        {

            return (_context.SaveChanges() >= 0);
        }


        private bool SearchAllSecurities(SecuritiesResourceParameters securitiesResourceParameters)
        {


            return (string.IsNullOrWhiteSpace(securitiesResourceParameters.industry)
                && string.IsNullOrWhiteSpace(securitiesResourceParameters.sector)
                && string.IsNullOrWhiteSpace(securitiesResourceParameters.symbol)
                && string.IsNullOrWhiteSpace(securitiesResourceParameters.searchQuery)
                && !securitiesResourceParameters.preferred.HasValue
                && !securitiesResourceParameters.lastModifiedPrior.HasValue
                && string.IsNullOrWhiteSpace(securitiesResourceParameters.searchQuery)
                && string.IsNullOrWhiteSpace(securitiesResourceParameters.filterType)
                && string.IsNullOrWhiteSpace(securitiesResourceParameters.perChangeHigh)
                && string.IsNullOrWhiteSpace(securitiesResourceParameters.perChangeLow)
                && !securitiesResourceParameters.perFrom52WeekHigh.HasValue
                && !securitiesResourceParameters.perFrom52WeekLow.HasValue
                && !securitiesResourceParameters.minVolume.HasValue
                && !securitiesResourceParameters.lastModifiedRangeStart.HasValue
                );
        }


        public IEnumerable<Security> GetSecurities(SecuritiesResourceParameters securitiesResourceParameters)
        {
            if (SearchAllSecurities(securitiesResourceParameters)
                    )
            {
                return _context.Securities.ToList<Security>();
            }
            var collection = _context.Securities as IQueryable<Security>;



            if (securitiesResourceParameters.lastModifiedRangeStart.HasValue)
            {
                collection = collection.Where(a => a.LastModified > securitiesResourceParameters.lastModifiedRangeStart.Value);
                
                
            }
            if (securitiesResourceParameters.lastModifiedPrior.HasValue)
            {

                collection = collection.Where(a => a.LastModified < securitiesResourceParameters.lastModifiedPrior.Value);
            }


            if (securitiesResourceParameters.preferred.HasValue)
            {

                collection = collection.Where(a => a.preferred == securitiesResourceParameters.preferred.Value);
            }

            if (!string.IsNullOrWhiteSpace(securitiesResourceParameters.industry))
            {
                securitiesResourceParameters.industry = securitiesResourceParameters.industry.Trim();
                collection = collection.Where(a => a.Industry == securitiesResourceParameters.industry);
            }

            if (!string.IsNullOrWhiteSpace(securitiesResourceParameters.symbol))
            {
                securitiesResourceParameters.symbol = securitiesResourceParameters.symbol.Trim();
                collection = collection.Where(a => a.Symbol == securitiesResourceParameters.symbol);
            }

            if (!string.IsNullOrWhiteSpace(securitiesResourceParameters.sector))
            {
                securitiesResourceParameters.sector = securitiesResourceParameters.symbol.Trim();
                collection = collection.Where(a => a.Sector == securitiesResourceParameters.sector);
            }


            if (securitiesResourceParameters.perFrom52WeekLow.HasValue)
            {
                decimal reduction = 1 - securitiesResourceParameters.perFrom52WeekLow.Value;
                collection = collection.Where(a => a.YearLow > reduction * a.CurrentPrice);
            }

            if (securitiesResourceParameters.perFrom52WeekHigh.HasValue)
            {
                decimal reduction = 1 - securitiesResourceParameters.perFrom52WeekHigh.Value;
                collection = collection.Where(a => a.YearHigh > reduction * a.CurrentPrice);
            }

            if (securitiesResourceParameters.minVolume.HasValue)
            {
                int volume = securitiesResourceParameters.minVolume.Value;
                collection = collection.Where(a => a.Volume > volume);
            }


            if (!string.IsNullOrWhiteSpace(securitiesResourceParameters.filterType))
            {


                switch (securitiesResourceParameters.filterType.ToLower())
                {


                    case "needhistoricalpriceupdated":
                        var currentDay = DateTime.Now;
                        currentDay = new DateTime(currentDay.Year, currentDay.Month, currentDay.Day, 0, 0, 0);
                        if (currentDay.DayOfWeek == DayOfWeek.Saturday)
                        {
                            currentDay = currentDay.AddDays(-1);
                        }
                        if (currentDay.DayOfWeek == DayOfWeek.Sunday)
                        {
                            currentDay = currentDay.AddDays(-2);
                        }
                        if (!string.IsNullOrWhiteSpace(securitiesResourceParameters.searchQuery) && securitiesResourceParameters.searchQuery.ToLower() == "all")
                        {
                            currentDay = currentDay.AddDays(5);//future date where we won't have historic dates
                            securitiesResourceParameters.searchQuery = "";
                        }
                        bool excludeHistorical = false;
                        collection = collection.Where(x => x.excludeHistorical == excludeHistorical);
                        var historicalPrice = _context.HistoricalPrices as IQueryable<HistoricalPrice>;

                        //var exclusions = collection.Join(historicalPrice, x => x.Id, y => y.SecurityId, (query1, query2) => new { query1, query2 })
                        //  .Where(o => currentDay < o.query2.HistoricDate).Select(x => x.query1).ToList();

                        // collection = collection.Except(exclusions);

                        break;


                }
            }


            if (!string.IsNullOrWhiteSpace(securitiesResourceParameters.perChangeLow) &&
                !string.IsNullOrWhiteSpace(securitiesResourceParameters.perChangeHigh)
                )
            {

                decimal perChangeLow;
                decimal.TryParse(securitiesResourceParameters.perChangeLow, out perChangeLow);
                decimal perChangeHigh;
                decimal.TryParse(securitiesResourceParameters.perChangeHigh, out perChangeHigh);
                if (perChangeLow == 0)
                {
                    perChangeLow = -5;
                }

                if (perChangeHigh == 0)
                {
                    perChangeHigh = 5;
                }

                collection = collection.Where(x => x.PercentageChange <= perChangeHigh && x.PercentageChange >= perChangeLow);

            }

            if (!string.IsNullOrWhiteSpace(securitiesResourceParameters.searchQuery))
            {

                collection = collection.Where(a => a.Symbol.Contains(securitiesResourceParameters.searchQuery)
                || a.Name.Contains(securitiesResourceParameters.searchQuery)
                || a.Industry.Contains(securitiesResourceParameters.searchQuery)
                );
            }

            return collection.ToList();
        }



        public List<HistoricalPrice> GetHistoricalPrices(int securityId, HistoricalPricesResourceParameters historicalPriceResourceParameters)
        {
            var collection = _context.HistoricalPrices as IQueryable<HistoricalPrice>;


            collection = collection.Where(x => x.SecurityId == securityId);
            if (historicalPriceResourceParameters.HistoricDateLow.HasValue && historicalPriceResourceParameters.HistoricDateHigh.HasValue)
            {
                collection = collection.Where(x => x.HistoricDate >= historicalPriceResourceParameters.HistoricDateLow && x.HistoricDate <= historicalPriceResourceParameters.HistoricDateHigh);
            }
            if (historicalPriceResourceParameters.openLow.HasValue)
            {
                collection = collection.Where(x => x.Open > historicalPriceResourceParameters.openLow);
            }
            return collection.ToList();
        }

        public HistoricalPrice GetHistoricalPricesRange(int securityId)
        {
            var collection = _context.HistoricalPrices as IQueryable<HistoricalPrice>;


            return collection.Where(x => x.SecurityId == securityId).Select(x=> new HistoricalPrice {Id = x.Id, HistoricDate = x.HistoricDate  }).OrderByDescending(x=>x.HistoricDate).FirstOrDefault();
        }
        
        /*
        public HistoricalPrice GetLastHistoricalPrices(int securityId)
        {
            var collection = _context.HistoricalPrices as IQueryable<HistoricalPrice>;


            return collection.Where(x => x.SecurityId == securityId).Select(x => new HistoricalPrice { Id = x.Id, HistoricDate = x.HistoricDate }).OrderByDescending(x => x.HistoricDate).FirstOrDefault();
        }
        */


        public void UpdateEarnings(List<EarningDto> earnings, Security security)
        {

            if (earnings.Count == 0)
            {
                return;
            }

            List<Earning> currentHistoricalEarnings = _context.Earnings.Where(x => x.SecurityId == earnings[0].SecurityId).ToList();

            UpdateEarnings(earnings, currentHistoricalEarnings);



        }

        public void UpdateEarnings(List<EarningDto> earnings)
        {
            if (earnings.Count == 0)
            {
                return;
            }

            var earningsRecs = earnings.GroupBy(x => x.SecurityId).Select(g => g.Key).ToList();

            List<Earning> currentHistoricalEarnings = _context.Earnings.Where(x=> earningsRecs.Contains(x.SecurityId)).ToList();
            UpdateEarnings(earnings, currentHistoricalEarnings);
        }




        private List<Earning> GetUpdateEarnings(List<EarningDto> earnings, List<Earning> currentEarnings)
        {

            List<Earning> earningsChanges = earnings.Join(currentEarnings, x => x.SecurityId,
              y => y.SecurityId, (query1, query2) => new { query1, query2 }).Where(o => o.query1.ActualEarningsDate == o.query2.ActualEarningsDate

              ).Select(x => new Earning
              {

                  SecurityId = x.query1.SecurityId,
                  Id = x.query2.Id,
                  ActualEarningsDate = x.query1.ActualEarningsDate,
                  EPSEstimate = x.query1.EPSEstimate,
                  ReportedEPS = x.query1.ReportedEPS,
                  GAAPEPS = x.query1.GAAPEPS,
                  RevenueEstimate = x.query1.RevenueEstimate,
                  ActualRevenue = x.query1.ActualRevenue,
                  ReportTime = x.query1.ReportTime


              }).ToList();


            return earningsChanges;
        }

        private void UpdateEarnings(List<EarningDto> earnings, List<Earning> currentEarnings)
        {




            var newRecords = earnings.Join(currentEarnings, x => x.SecurityId,
          y => y.SecurityId, (query1, query2) => new { query1, query2 }).Where(o => o.query1.ActualEarningsDate == o.query2.ActualEarningsDate && o.query1.SecurityId == o.query2.SecurityId
          ).Select(x => x.query1).ToList();


            List<EarningDto> curNewEarnings = earnings.Except(newRecords).ToList();
            List<Earning> newEarnings = _mapper.Map<List<Earning>>(curNewEarnings).ToList();

            if (newEarnings.Count > 0)
            {
                earningsAdd(newEarnings);
            }



            List<Earning> earningsinDB = GetUpdateEarnings(earnings, currentEarnings);


            var updatedAlready = earningsinDB.Join(currentEarnings, x => x.SecurityId,
                y => y.SecurityId, (query1, query2) => new { query1, query2 }).Where(o => o.query1.ActualEarningsDate == o.query2.ActualEarningsDate
                && o.query1.EPSEstimate == o.query2.EPSEstimate
                && o.query1.ReportedEPS == o.query2.ReportedEPS
                && o.query1.RevenueEstimate == o.query2.RevenueEstimate
                && o.query1.ActualRevenue == o.query2.ActualRevenue
                && o.query1.ReportTime == o.query2.ReportTime

                ).Select(x => x.query1).ToList();


            earningsinDB = earningsinDB.Except(updatedAlready).ToList();


            if (earningsinDB.Count > 0)
            {
                earningsUpdate(earningsinDB);
            }



        }





       




      
        /// <summary>
        /// Gets the dividends from the database compared to what dividends are in there
        /// </summary>
        /// <param name="dividends"></param>
        /// <param name="currentDividends"></param>
        /// <returns></returns>
        private List<PriorPurchaseEstimate> GetPriorPurchaseEstimateList(List<PriorPurchaseEstimateDto> priorPurchaseEstimates, List<PriorPurchaseEstimate> currentPriorPurchaseEstimates)
        {
       

            List<PriorPurchaseEstimate> cuurentPriorPurchaseEstimates = priorPurchaseEstimates.Join(currentPriorPurchaseEstimates, x => x.SecurityId,
                y => y.SecurityId, (query1, query2) => new { query1, query2 }).Select(x => new PriorPurchaseEstimate
                {
                    SecurityId = x.query1.SecurityId,
                    Id = x.query2.Id,
                    PurchasePrice = x.query1.PurchasePrice,
                    DateCreated = x.query2.DateCreated,
                    DateModified = x.query1.DateModified,
                    Shares = x.query1.Shares,
                    PurchaseFrequency = x.query1.PurchaseFrequency,
                    FirstPurchaseDate = x.query1.FirstPurchaseDate

                }).ToList();


            return cuurentPriorPurchaseEstimates;

        }

        public StockScreenerSearchResourceParameters GetStockScreenerSearchDetails(int stockScreenId)
        {
            StockScreenerSearchResourceParameters stockScreenResourceParams = new StockScreenerSearchResourceParameters();

           var criterias = _context.StockScreenerSearchDetails.Where(x => x.StockScreenerId == stockScreenId).Join(_context.ScreenerCriterias, x => x.ScreenerCriteriaId,
                y => y.id, (query1, query2) => new { query1, query2 }).Select(x => new { Value = x.query1.SearchValue, 
                    
                    ObjectName = x.query2.JSONObjectName,
                ObjectType = x.query2.ObjectType
                }).ToList();

            foreach(var criteria in criterias)
            {

                try
                {
                    PropertyInfo propertyInfo = stockScreenResourceParams.GetType().GetProperty(criteria.ObjectName);
                    // make sure object has the property we are after
                    if (propertyInfo != null)
                    {
                        switch (criteria.ObjectType)
                        {
                            case "int":
                                if (criteria.Value == "" || criteria.Value == "null")
                                {
                                    propertyInfo.SetValue(stockScreenResourceParams, null, null);
                                }
                                else
                                {
                                    propertyInfo.SetValue(stockScreenResourceParams, Int32.Parse(criteria.Value), null);
                                }
                                break;
                            case "decimal":

                                if (criteria.Value == "" || criteria.Value == "null")
                                {
                                    propertyInfo.SetValue(stockScreenResourceParams, null, null);
                                }
                                else
                                {
                                    propertyInfo.SetValue(stockScreenResourceParams, decimal.Parse(criteria.Value), null);
                                }
                                break;

                            case "bool":

                                if (criteria.Value == "" || criteria.Value == "null")
                                {
                                    propertyInfo.SetValue(stockScreenResourceParams, null, null);
                                }
                                else
                                {
                                    propertyInfo.SetValue(stockScreenResourceParams, bool.Parse(criteria.Value), null);
                                }
                                break;
                            case "string":
                                propertyInfo.SetValue(stockScreenResourceParams, criteria.Value, null);
                                break;
                        }
                        // propertyInfo.PropertyType.Name

                    }
                }
                catch(Exception ex)
                {

                }
                //percentFrom52WeekLowRangeHigh
              
            }
            stockScreenResourceParams.securityLastModifiedRangeLow = DateTime.Now.AddDays(-3);

            return stockScreenResourceParams;

        }




        public List<StockPurchaseOption> GetStockScreenerResults(StockScreenerSearchResourceParameters stockPurOptResourceParams)
        {
            // var lastModified = DateTime.Now.AddDays(-3);

            var securityRecs = _context.Securities as IQueryable<Security>;

            if (stockPurOptResourceParams.securityVolumeRangeLow.HasValue)
            {
                securityRecs = securityRecs.Where(x => x.Volume > stockPurOptResourceParams.securityVolumeRangeLow);
            }

            if (stockPurOptResourceParams.ipoDateRangeStart.HasValue)
            {
                securityRecs = securityRecs.Where(x => x.IPODate >= stockPurOptResourceParams.ipoDateRangeStart);
            }

            
            if (stockPurOptResourceParams.securityLastModifiedRangeLow.HasValue)
            {
                securityRecs = securityRecs.Where(x => x.LastModified > stockPurOptResourceParams.securityLastModifiedRangeLow);
            }
            if (stockPurOptResourceParams.securitypercentChangeRangeHigh.HasValue)
            {
                securityRecs = securityRecs.Where(x => x.PercentageChange < stockPurOptResourceParams.securitypercentChangeRangeHigh);
            }
            if (stockPurOptResourceParams.securitypercentChangeRangeLow.HasValue)
            {
                securityRecs = securityRecs.Where(x => x.PercentageChange > stockPurOptResourceParams.securitypercentChangeRangeLow);
            }
            if (stockPurOptResourceParams.percentFrom52WeekLowRangeLow.HasValue)
            {
                securityRecs = securityRecs.Where(x => ((x.CurrentPrice - x.YearLow) / x.YearLow) * 100 < stockPurOptResourceParams.percentFrom52WeekLowRangeLow);
            }
            if (stockPurOptResourceParams.percentFrom52WeekLowRangeHigh.HasValue)
            {
                securityRecs = securityRecs.Where(x => ((x.CurrentPrice - x.YearLow) / x.YearLow) * 100 > stockPurOptResourceParams.percentFrom52WeekLowRangeHigh);
            }
            if (stockPurOptResourceParams.percentFrom52WeekHighRangeLow.HasValue)
            {
                securityRecs = securityRecs.Where(x => ((x.CurrentPrice - x.YearHigh) / x.YearHigh) * 100 < stockPurOptResourceParams.percentFrom52WeekHighRangeLow);
            }

            if (stockPurOptResourceParams.percentFrom52WeekHighRangeHigh.HasValue)
            {
                securityRecs = securityRecs.Where(x => ((x.CurrentPrice - x.YearHigh) / x.YearHigh) * 100 > stockPurOptResourceParams.percentFrom52WeekHighRangeHigh);
            }
            if (stockPurOptResourceParams.onlyPreferred.HasValue && stockPurOptResourceParams.onlyPreferred.Value == true)
            {
                securityRecs = securityRecs.Where(x => x.preferred == true);
            }

            IQueryable<SecurityPercentStatJoin> securityPercentStatJoin =
                securityRecs.Join(_context.SecurityPercentageStatistics, x => x.Id,
                y => y.SecurityId, (security, secPerStats) => new SecurityPercentStatJoin
                { security = security, secPerStats = secPerStats });

            var info = securityPercentStatJoin.ToList();


            securityPercentStatJoin = SecurityPercentStatJoin(securityPercentStatJoin, stockPurOptResourceParams);



            /*   .Where(x => x.security.LastModified > lastModified &&
             x.secPerStats.AverageDrop != 0 &&
             x.security.Volume > 100000 &&
             x.security.PercentageChange < 0 &&
             ((x.security.DayLow - x.security.PriorDayOpen) / x.security.PriorDayOpen) * 100 < x.secPerStats.AverageDrop * (decimal)1.5
             //x.security.PercentageChange < x.secPerStats.AverageDrop * (decimal)1.5
             )
          */

            //.ToList()
            var details = securityPercentStatJoin
               .Join(_context.CurrentPeakRanges, x => x.security.Id, y => y.SecurityId, (s, curPeakRange) => new { s, curPeakRange })
               .Join(_context.PriorPurchaseEstimates, x => x.s.security.Id, y => y.SecurityId, (s, priorPurEst) => new { s, priorPurEst });

            if (stockPurOptResourceParams.priorPurchaseEstimateSharesRangeLow.HasValue)
            {
                details = details.Where(x => x.priorPurEst.Shares > stockPurOptResourceParams.priorPurchaseEstimateSharesRangeLow);

            }
            if (stockPurOptResourceParams.priorPurchaseEstimateYearlyPercentRangeLow.HasValue)
            {
                details = details.Where(x =>
                    ((((x.priorPurEst.Shares * x.s.s.security.CurrentPrice) -
                    x.priorPurEst.PurchasePrice) / x.priorPurEst.PurchasePrice) * 100) / (x.priorPurEst.Shares / 12) >
                    stockPurOptResourceParams.priorPurchaseEstimateYearlyPercentRangeLow
                    );
            }


            var stockPurchaseOptions = details.Join(_context.PeakRangeDetails, x => x.s.s.security.Id, y => y.SecurityId, (s, peakRangeDetails) => new { s, peakRangeDetails })
                  .ToList().
                  GroupBy(
                  x => x.s.s.s.security.Id)
                   .Select(g => new StockPurchaseOption
                   {
                       Security = g.Select(x => x.s.s.s.security).FirstOrDefault(),
                       SecurityPercentageStatistic = g.Select(x => x.s.s.s.secPerStats).FirstOrDefault(),
                       CurrentPeakRange = g.Select(x => x.s.s.curPeakRange).FirstOrDefault(),
                       PriorPurchaseEstimate = g.Select(x => x.s.priorPurEst).FirstOrDefault(),

                       PeakRangeDetail = g.Select(x => x.peakRangeDetails).ToList()//,
                                                                                   //PeakRangeDetail = g.Where(x=>x.s.Id == x.peakRanges.SecurityId).Select(o => o.peakRanges).ToList()
                 }).ToList();

            /*


        var stockPurchaseOptions = details.Select(x => new StockPurchaseOption
            {
                Security = x.s.s.security,
                SecurityPercentageStatistic = x.s.s.secPerStats,
                CurrentPeakRange = x.s.curPeakRange,
                PriorPurchaseEstimate = x.priorPurEst,
                //PeakRangeDetail = x.peakRangeDetails

            }).ToList();
            */

            return stockPurchaseOptions;
        }



        private IQueryable<SecurityPercentStatJoin> SecurityPercentStatJoin(IQueryable<SecurityPercentStatJoin> securityPercentStatJoin, StockScreenerSearchResourceParameters stockPurOptResourceParams)
        {
            
            if(stockPurOptResourceParams.percentDropType =="" || stockPurOptResourceParams.calculatedPercentDropType == ""
                    ||
                stockPurOptResourceParams.percentDropType == null || stockPurOptResourceParams.calculatedPercentDropType == null
                )
            {
                return securityPercentStatJoin;
            }
            
            switch (stockPurOptResourceParams.percentDropType)
            {
                case "current":
                    securityPercentStatJoin = securityPercentStatJoin.Select(x => new SecurityPercentStatJoin
                    {
                        secPerStats = x.secPerStats,
                        security = x.security,
                        PercentDrop = ((x.security.CurrentPrice - x.security.PriorDayOpen) / x.security.PriorDayOpen) * 100
                    });
                    break;
                case "daylow":
                    securityPercentStatJoin = securityPercentStatJoin.Select(x => new SecurityPercentStatJoin
                    {
                        secPerStats = x.secPerStats,
                        security = x.security,
                        PercentDrop = ((x.security.DayLow - x.security.PriorDayOpen) / x.security.PriorDayOpen) * 100
                    });
                    break;

            }

                
            switch (stockPurOptResourceParams.calculatedPercentDropType)
            {

                case "average":
                    securityPercentStatJoin = securityPercentStatJoin.Select(x => new SecurityPercentStatJoin
                    {
                        secPerStats = x.secPerStats,
                        security = x.security,
                        PercentDrop = x.PercentDrop,
                        StatPercentDrop = x.secPerStats.AverageDrop
                    });
                    break;
                case "averagetimesoneandhalfpercent":
                    securityPercentStatJoin = securityPercentStatJoin.Select(x => new SecurityPercentStatJoin
                    {
                        secPerStats = x.secPerStats,
                        security = x.security,
                        PercentDrop = x.PercentDrop,
                        StatPercentDrop = x.secPerStats.AverageDrop * (decimal)1.5
                    });
                    break;

                case "averagedroplowaverage":
                    securityPercentStatJoin = securityPercentStatJoin.Select(x => new SecurityPercentStatJoin
                    {
                        secPerStats = x.secPerStats,
                        security = x.security,
                        PercentDrop = x.PercentDrop,
                        StatPercentDrop = x.secPerStats.AvgDropLowAvg
                    });
                    break;

                case "percentile5":
                    securityPercentStatJoin = securityPercentStatJoin.Select(x => new SecurityPercentStatJoin
                    {
                        secPerStats = x.secPerStats,
                        security = x.security,
                        PercentDrop = x.PercentDrop,
                        StatPercentDrop = x.secPerStats.Percent5
                    });
                    break;
                case "percentile10":
                    securityPercentStatJoin = securityPercentStatJoin.Select(x => new SecurityPercentStatJoin
                    {
                        secPerStats = x.secPerStats,
                        security = x.security,
                        PercentDrop = x.PercentDrop,
                        StatPercentDrop = x.secPerStats.Percent10
                    });
                    break;
                case "percentile15":
                    securityPercentStatJoin = securityPercentStatJoin.Select(x => new SecurityPercentStatJoin
                    {
                        secPerStats = x.secPerStats,
                        security = x.security,
                        PercentDrop = x.PercentDrop,
                        StatPercentDrop = x.secPerStats.Percent15
                    });
                    break;
            }

            

            securityPercentStatJoin = securityPercentStatJoin.Where(x =>
                        x.PercentDrop < x.StatPercentDrop);

            


            return securityPercentStatJoin;

        }

        public List<StockScreener> GetStockScreeners()
        {
         return   _context.StockScreeners.ToList();
        }

        public List<StockScreenerAlertsHistoryDto> GetNewStockScreenerAlertsHistory(List<Security> securities, int stockScreenerId)
        {

            DateTime currentDate = DateTime.Now;
            var currentDay = currentDate.Date;

            var securityList = securities.Select(x => x.Id ).ToList();
            var currentRecords = _context.StockScreenerAlertsHistory.Where(x => x.StockScreenerId == stockScreenerId && securityList.Contains(x.SecurityId) && x.DateRecorded > currentDay).ToList();
            var updatedAlready = securities.Join(currentRecords, x => x.Id,
                y => y.SecurityId, (s, current) => new { s, current }).Select(x=>x.s).ToList();
           var newRecords = securities.Except(updatedAlready).ToList();

            return newRecords.Select(x => new StockScreenerAlertsHistoryDto { 
                DateRecorded = DateTime.UtcNow, 
                SecurityId = x.Id, 
                StockScreenerId = stockScreenerId
            ,AlertPercent = (decimal)x.PercentageChange,
             AlertPrice = x.CurrentPrice
            
            }).ToList();

        }


        public StockScreenerAlertType GetStockScreenerAlertType(int stockScreenerAlertTypeId)
        {
            return _context.StockScreenerAlertTypes.Where(x=> x.id == stockScreenerAlertTypeId).FirstOrDefault();
        }

        public List<StockScreenerAlertType> GetStockScreenerAlertTypes()
        {
            return _context.StockScreenerAlertTypes.ToList();
        }

       

        public bool IsMarketClosed(DateTime currentDate)
        {
            if(currentDate.Hour < 14){//time is before 9am
                return true;
            }
            else if(currentDate.Hour == 14 && currentDate.Minute < 30)//time is 9am  est and the minutes is under 30
            {
                return true;
            }
            if (currentDate.Hour > 21 )//time is 4pm EST
            {
                return true;
            }            
            else if(currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday )// day is not a saturday or sunday
            {
                return true;
            }
            else
            {
                
                var date = currentDate.Date;
                int holidayFind =  _context.TradingHolidays.Where(x => x.HolidayDate == date).ToList().Count;
                if (holidayFind>0)//if a holiday is found for the day
                {
                    return true;
                }
                return false;
            }


        }


        /*
            TimeZoneInfo timeZone = TimeZoneInfo.Local;

            DateTime convertedTime = currentDate;
            TimeSpan offset;

            if (currentDate.Kind == DateTimeKind.Local && !timeZone.Equals(TimeZoneInfo.Local))
                convertedTime = TimeZoneInfo.ConvertTime(currentDate, TimeZoneInfo.Local, timeZone);
            else if (currentDate.Kind == DateTimeKind.Utc && !timeZone.Equals(TimeZoneInfo.Utc))
                convertedTime = TimeZoneInfo.ConvertTime(currentDate, TimeZoneInfo.Utc, timeZone);
            offset = timeZone.GetUtcOffset(currentDate);
            */


        public void AddStockScreenerAlertsHistoryRecords(List<StockScreenerAlertsHistoryDto> stockScreenerAlertsHistoryRecords)
        {

            IEnumerable<StockScreenerAlertsHistory> stockScreenerAlertsHistoryRecordsAdd = _mapper.Map<List<StockScreenerAlertsHistory>>(stockScreenerAlertsHistoryRecords);
            _utility.AddRecords(stockScreenerAlertsHistoryRecordsAdd, _context);
        }

        public void UpsertPriorPurchaseEstimates(List<PriorPurchaseEstimateDto> priorPurchaseEstimates)
        {
       

           

            List<PriorPurchaseEstimate> currentPriorPurchaseEstimates = _context.PriorPurchaseEstimates.ToList();



            List<PriorPurchaseEstimate> existingPriorPurchaseEstimate = GetPriorPurchaseEstimateList(priorPurchaseEstimates, currentPriorPurchaseEstimates);



            
            var updatedAlready = existingPriorPurchaseEstimate.Join(currentPriorPurchaseEstimates, x => x.SecurityId,
   y => y.SecurityId, (query1, query2) => new { query1, query2 }).Where(o => o.query1.Shares == o.query2.Shares
   //&& o.query1.DateModified == o.query2.DateModified
   //&& o.query1.DateCreated == o.query2.DateCreated
   && o.query1.PurchasePrice == o.query2.PurchasePrice
   && o.query1.Shares == o.query2.Shares
   && o.query1.PurchaseFrequency == o.query2.PurchaseFrequency
   && o.query1.FirstPurchaseDate == o.query2.FirstPurchaseDate



   ).Select(x => x.query1).ToList();



            existingPriorPurchaseEstimate = existingPriorPurchaseEstimate.Except(updatedAlready).ToList();


            _utility.UpdateRecords(existingPriorPurchaseEstimate, _context);


            /*Will find any records that are new and add them to the DB*/
            var newRecords = priorPurchaseEstimates.Join(currentPriorPurchaseEstimates, x => x.SecurityId,
             y => y.SecurityId, (query1, query2) => new { query1, query2 }).Select(x => x.query1);


           var newSecurityPurchases = priorPurchaseEstimates.Except(newRecords).ToList();
            var securityCheckAdd = _mapper.Map<List<PriorPurchaseEstimate>>(newSecurityPurchases).Cast<object>();

            _utility.AddRecords(securityCheckAdd,_context);
            
        }
        






        private List<HistoricalPrice> GetHistoricalPrices(List<HistoricalPriceforUpdateDto> historicalPrices, List<HistoricalPrice> currentHistoricalPrices)
        {

            List<HistoricalPrice> historicChanges = historicalPrices.Join(currentHistoricalPrices, x => x.SecurityId,
                y => y.SecurityId, (query1, query2) => new { query1, query2 }).Where(o => o.query1.HistoricDate == o.query2.HistoricDate

                ).Select(x => new HistoricalPrice
                {
                    High = x.query1.High,
                    Id = x.query2.Id,
                    Low = x.query1.Low,
                    Open = x.query1.Open,
                    Close = x.query1.Close,
                    Volume = x.query1.Volume,
                    SecurityId = x.query1.SecurityId,
                    HistoricDate = x.query1.HistoricDate,
                    PercentChange = x.query1.PercentChange



                }).ToList();


            return historicChanges;

        }


        public void UpsertHistoricalPrices(List<HistoricalPriceforUpdateDto> historicalPrices)
        {

            if (historicalPrices.Count == 0)
            {
                return;
            }

            List<HistoricalPrice> currentHistoricalPrices = new List<HistoricalPrice>();
            int currentSecurityId=0;
            historicalPrices = historicalPrices.OrderBy(x => x.SecurityId).ThenBy(x => x.HistoricDate).ToList();
            foreach (var historicalPrice in historicalPrices)
            {
                if (currentSecurityId == 0 || currentSecurityId != historicalPrice.SecurityId) {
                    currentSecurityId = historicalPrice.SecurityId;
                    currentHistoricalPrices.AddRange(_context.HistoricalPrices
                .Where(x => x.SecurityId == currentSecurityId && x.HistoricDate >= historicalPrice.HistoricDate).ToList());
                }
                
            }
            


            List<HistoricalPrice> pricesInDb = GetHistoricalPrices(historicalPrices, currentHistoricalPrices);

            var updatedAlready = pricesInDb.Join(currentHistoricalPrices, x => x.SecurityId,
                y => y.SecurityId, (query1, query2) => new { query1, query2 }).Where(o => o.query1.HistoricDate == o.query2.HistoricDate
                && o.query1.High == o.query2.High
                && o.query1.Low == o.query2.Low
                && o.query1.Open == o.query2.Open
                && o.query1.Close == o.query2.Close
                && o.query1.PercentChange == o.query2.PercentChange
                ).Select(x => x.query1);


            pricesInDb = pricesInDb.Except(updatedAlready).ToList();
            historicPriceUpdate(pricesInDb);



            /*Will find any records that are new and add them to the DB*/
            var newRecords = historicalPrices.Join(currentHistoricalPrices, x => x.SecurityId,
             y => y.SecurityId, (query1, query2) => new { query1, query2 }).Where(o => o.query1.HistoricDate == o.query2.HistoricDate
             ).Select(x => x.query1);


            historicalPrices = historicalPrices.Except(newRecords).ToList();
            List<HistoricalPrice> historicalPricesAdd = _mapper.Map<List<HistoricalPrice>>(historicalPrices).ToList();

            if (historicalPricesAdd.Count > 0)
            {
                historicPriceAdd(historicalPricesAdd);
            }

            currentHistoricalPrices = new List<HistoricalPrice>();

        }

        
        private void historicPriceUpdate(List<HistoricalPrice> historicalPrices)
        {
            //   _context.BulkUpdate(historicalPrices);
            _utility.UpdateRecords(historicalPrices,_context);
        }

        private void historicPriceAdd(List<HistoricalPrice> historicalPrices)
        {

            //  _context.BulkInsert(historicalPrices);
            _utility.AddRecords(historicalPrices,_context);

          

        }

        

        private void earningsUpdate(List<Earning> earnings)
        {
            //  _context.BulkUpdate(earnings);

            _utility.UpdateRecords(earnings,_context);


        }


        private void earningsAdd(List<Earning> earnings)
        {
            //   _context.BulkInsert(earnings);
            _utility.AddRecords(earnings,_context);

        }


        public HistoricalPrice GetHistoricPrice(int securityId, DateTime historicDate)
        {
            if (securityId == 0)
            {
                throw new ArgumentNullException(nameof(securityId));
            }

            // return _context.Authors.FirstOrDefault(a => a.Id == authorId);

            return _context.HistoricalPrices.FirstOrDefault(x => x.SecurityId == securityId && x.HistoricDate == historicDate);
        }






        private List<Security> GetSecurities(List<SecurityForUpdateDto> securities)
        {


            List<Security> security = _context.Securities.ToList();
            
            security = securities.Join(security, x => x.Symbol, y => y.Symbol, (query1, query2) => new { query1, query2 }).Where(x=> x.query1.SecurityType == x.query2.SecurityType)
                .Select(x => new Security
            {

                Id = x.query2.Id,
                Symbol = x.query1.Symbol,
                
                Name = x.query1.Name == null ? x.query2.Name : x.query1.Name,
                DayHigh = Decimal.Round(x.query1.DayHigh.Value, 2),
                DayLow = Decimal.Round(x.query1.DayLow.Value, 2),
                YearHigh = Decimal.Round(x.query1.YearHigh.Value, 2),
                YearLow = Decimal.Round(x.query1.YearLow.Value, 2),
                CurrentPrice = Decimal.Round(x.query1.CurrentPrice, 2),

                EarningsDate = x.query1.EarningsDate,
                Volume = x.query1.Volume,
                PriorDayOpen = Decimal.Round(x.query1.PriorDayOpen.Value, 2),
                LastModified = x.query1.LastModified,
                SecurityType = x.query1.SecurityType,
                Industry = x.query2.Industry,
                Sector = x.query2.Sector,
                PercentageChange = x.query1.PercentageChange,
                preferred = x.query2.preferred,
                excludeHistorical = x.query2.excludeHistorical,
                IPOYear = x.query2.IPOYear,
                Dividend = x.query2.Dividend,
                DividendDate = x.query2.DividendDate,
                IPODate = x.query2.IPODate,
                    Description = x.query2.Description

                }).ToList();
  
            return security;
        }




        public void UpdateSecurities(List<SecurityForUpdateDto> securities)
        {


            List<Security> dbSecurities = GetSecurities(securities);

            var securityTable = _context.Securities.ToList();




            var updatedAlready = dbSecurities.Join(securityTable, x => x.Id,
              y => y.Id, (query1, query2) => new { query1, query2 }).Where(o =>
              o.query1.DayHigh == o.query2.DayHigh
              && o.query1.DayLow == o.query2.DayLow
              && o.query1.Volume == o.query2.Volume
              && o.query1.YearHigh == o.query2.YearHigh
              && o.query1.YearLow == o.query2.YearLow


               && o.query1.PriorDayOpen == o.query2.PriorDayOpen
                && o.query1.CurrentPrice == o.query2.CurrentPrice
                && o.query1.EarningsDate == o.query2.EarningsDate
                && o.query1.SecurityType == o.query2.SecurityType
                && o.query1.PercentageChange == o.query2.PercentageChange

              ).Select(x => x.query1).ToList();


           var  updatedbSecurities = dbSecurities.Except(updatedAlready).ToList();



            List<Security> update = new List<Security>();
            foreach (Security security in updatedbSecurities)
            {
                if (securityTable.Where(x => x.Symbol == security.Symbol
              ).Count() > 0)
                {
                    update.Add(security);
                }
            }
            BulkSaveUpdate(update);




            /*Will find any records that are new and add them to the DB*/
            var existingRecords = securities.Join(dbSecurities, x => x.Symbol,
             y => y.Symbol, (query1, query2) => new { query1, query2 }).Where(o => o.query1.SecurityType == o.query2.SecurityType
             ).Select(x => x.query1).ToList();


            securities = securities.Except(existingRecords).ToList();
            List<Security> newRecords = _mapper.Map<List<Security>>(securities).ToList();



            _utility.AddRecords(newRecords,_context);




        }


        public void BulkSaveUpdate(List<Security> securities)
        {
            _utility.UpdateRecords(securities,_context);

        }

        public IEnumerable<Tuple<Earning, Security>> GetSecuritiesEarnings(IEnumerable<Earning> earnings)
        {
            return earnings.Join(_context.Securities
                , x => x.SecurityId,
              y => y.Id, (query1, query2) => new { query1, query2 }).Select(x => new Tuple<Earning, Security>(x.query1, x.query2)).ToList();
        }
      

        public IEnumerable<Tuple<AutoSecurityTrade, Security>> GetSecurityTradeHistorySecurities(IEnumerable<AutoSecurityTrade> securityTradeHistory)
        {
            return securityTradeHistory.Join(_context.Securities, x => x.SecurityId, y => y.Id, (sth, s) => new { sth, s }).Select(x => new Tuple<AutoSecurityTrade, Security>(x.sth, x.s)).ToList();
        }

        public IEnumerable<AutoSecurityTrade> GetSecurityTradeHistory(AutoSecurityTradesResourceParameters securityTradeHistoryResourceParameters)
        {

            if (securityTradeHistoryResourceParameters.rangePurchaseDateEnd == DateTime.MinValue
             && securityTradeHistoryResourceParameters.rangePurchaseDateStart == DateTime.MinValue
             && securityTradeHistoryResourceParameters.rangeSellDateEnd == DateTime.MinValue
             && securityTradeHistoryResourceParameters.rangeSellDateStart == DateTime.MinValue
             && !securityTradeHistoryResourceParameters.positionSold.HasValue
             && securityTradeHistoryResourceParameters.securityId == 0

                 )
            {
                return _context.AutoSecurityTrades.ToList();
            }

            var collection = _context.AutoSecurityTrades as IQueryable<AutoSecurityTrade>;

            if (securityTradeHistoryResourceParameters.rangePurchaseDateEnd != DateTime.MinValue)
            {
                collection = collection.Where(a => a.PurchaseDate != null && a.PurchaseDate <= securityTradeHistoryResourceParameters.rangePurchaseDateEnd);
            }
            if (securityTradeHistoryResourceParameters.rangePurchaseDateStart != DateTime.MinValue)
            {
                collection = collection.Where(a => a.PurchaseDate != null && a.PurchaseDate >= securityTradeHistoryResourceParameters.rangePurchaseDateStart);
            }

            if (securityTradeHistoryResourceParameters.rangeSellDateEnd != DateTime.MinValue)
            {
                collection = collection.Where(a => a.SellDate != null && a.SellDate <= securityTradeHistoryResourceParameters.rangeSellDateEnd);
            }
            if (securityTradeHistoryResourceParameters.rangeSellDateStart != DateTime.MinValue)
            {
                collection = collection.Where(a => a.SellDate != null && a.SellDate >= securityTradeHistoryResourceParameters.rangeSellDateStart);
            }


            if (securityTradeHistoryResourceParameters.positionSold.HasValue)
            {
                if (securityTradeHistoryResourceParameters.positionSold.Value)
                {
                    collection = collection.Where(a => a.SellDate != null);
                }
                else
                {
                    collection = collection.Where(a => a.SellDate == null);
                }

            }

            if (securityTradeHistoryResourceParameters.securityId != 0)
            {

                collection = collection.Where(a => a.SecurityId == securityTradeHistoryResourceParameters.securityId);
            }

            return collection.ToList();
        }
        /*
        public IEnumerable<Earning> GetEarnings(int securityId)
        {
            if (securityId == 0)
            {
                throw new ArgumentNullException(nameof(securityId));
            }


            return _context.Earnings.Where(x => x.SecurityId == securityId).ToList();


        }
        */

        public SecurityTask GetTasks(string taskName)
        {
            return _context.SecurityTasks.FirstOrDefault(x => x.TaskName == taskName);
        }

        public void UpdateTasks(SecurityTask task)
        {
            _context.SecurityTasks.Update(task);
            Save();
        }



        /// <summary>
        /// Joins the security percent statistics from the database and the recent updates
        /// </summary>
        /// <param name="securityPercentageStatistic"></param>
        /// <param name="currentSecurityPercentageStatistics"></param>
        /// <returns></returns>
        private List<SecurityPercentageStatistic> GetSecurityPercentageStatisticList(List<SecurityPercentageStatisticDto> securityPercentageStatistic, List<SecurityPercentageStatistic> currentSecurityPercentageStatistics)
        {


            List<SecurityPercentageStatistic> securityPurchaseChanges = securityPercentageStatistic.Join(currentSecurityPercentageStatistics, x => x.SecurityId,
                y => y.SecurityId, (query1, query2) => new { query1, query2 }).Select(x => new SecurityPercentageStatistic
                {
                    SecurityId = x.query1.SecurityId,
                    Id = x.query2.Id,
                    
                    DateCreated = x.query2.DateCreated,
                    DateModified = x.query1.DateModified,
                    AverageDrop = x.query1.AverageDrop,
                    Percent5 = x.query1.Percent5,
                    Percent10 = x.query1.Percent10,
                    Percent15 = x.query1.Percent15,
                    totalPercentSum = x.query1.totalPercentSum,
                    highLowRangeAverage = x.query1.highLowRangeAverage,
                    belowAverageCount = x.query1.belowAverageCount,
                    AvgDropLowAvg = x.query1.AvgDropLowAvg,
                    AvgDropHighLowRangeAvg = x.query1.AvgDropHighLowRangeAvg
                }).ToList();


            return securityPurchaseChanges;

        }

        public List<Security> GetCurrentPeakRanges()
        {
            DateTime pastDate = DateTime.Now.AddDays(-2);

            return _context.Securities.Join(_context.CurrentPeakRanges, x => x.Id,
                y => y.SecurityId, (query1, query2) => new { query1, query2 }).Where(o => o.query2.DateModified > pastDate)
                .Select(x => x.query1).ToList();

        }

        public List<Security> GetCurrentSecurityPercentage()
        {
            DateTime pastDate = DateTime.Now.AddDays(-7);

            return _context.Securities.Join(_context.SecurityPercentageStatistics, x => x.Id,
                y => y.SecurityId, (query1, query2) => new { query1, query2 }).Where(o => o.query2.DateModified > pastDate)
                .Select(x => x.query1).ToList();

        }
        public void UpsertSecurityPercentageStatistics(List<SecurityPercentageStatisticDto> securityPercentageStatistics)
        {



            List<SecurityPercentageStatistic> currentSecuritPurchaseChecks = _context.SecurityPercentageStatistics.ToList();



            List<SecurityPercentageStatistic> securityPercentageStats = GetSecurityPercentageStatisticList(securityPercentageStatistics, currentSecuritPurchaseChecks);




            var updatedAlready = securityPercentageStats.Join(currentSecuritPurchaseChecks, x => x.SecurityId,
                y => y.SecurityId, (query1, query2) => new { query1, query2 }).Where(o => o.query1.AverageDrop == o.query2.AverageDrop
                && o.query1.Percent5 == o.query2.Percent5
                && o.query1.Percent10 == o.query2.Percent10
                && o.query1.Percent15 == o.query2.Percent15
                && o.query1.totalPercentSum == o.query2.totalPercentSum
                && o.query1.highLowRangeAverage == o.query2.highLowRangeAverage
                && o.query1.belowAverageCount == o.query2.belowAverageCount
                && o.query1.AvgDropLowAvg == o.query2.AvgDropLowAvg
                && o.query1.AvgDropHighLowRangeAvg == o.query2.AvgDropHighLowRangeAvg

                && o.query1.AverageDrop != 0
                ).Select(x => x.query1).ToList();

            securityPercentageStats = securityPercentageStats.Except(updatedAlready).ToList();


            _utility.UpdateRecords(securityPercentageStats,_context);


            var securityHistory = GetSecurityPercentageStatisticsHistory(securityPercentageStats);
            UpsertSecurityPercentageStatisticsHistory(securityHistory);



            /*Will find any records that are new and add them to the DB*/
            var newRecords = securityPercentageStatistics.Join(currentSecuritPurchaseChecks, x => x.SecurityId,
             y => y.SecurityId, (query1, query2) => new { query1, query2 }).Select(x => x.query1);


            var newSecurityPurchases = securityPercentageStatistics.Except(newRecords).ToList();
            var securityCheckAdd = _mapper.Map<List<SecurityPercentageStatistic>>(newSecurityPurchases).Cast<object>();

            _utility.AddRecords(securityCheckAdd,_context);




        }
        public  void UpsertSecurityPercentageStatisticsHistory(List<SecurityPercentageStatisticHistory> securityPercentageStatisticsHistory)
        {
            _utility.AddRecords(securityPercentageStatisticsHistory,_context);
        }

        public List<SecurityPercentageStatisticHistory> GetSecurityPercentageStatisticsHistory(List<SecurityPercentageStatistic> securityPercentageStatistics)
        {
            var info = _context.SecurityPercentageStatistics.ToList()
                .Join(securityPercentageStatistics, x => x.SecurityId, y => y.SecurityId, (secPercentageDb, secPercentage) => new { secPercentageDb, secPercentage })
                .Select(x => new SecurityPercentageStatisticHistory { 
                    SecurityId = x.secPercentageDb.SecurityId,
                    DateCalculated= x.secPercentageDb.DateModified,
                    AverageDrop = x.secPercentageDb.AverageDrop,
                    Percent5 = x.secPercentageDb.Percent5,
                    Percent10 = x.secPercentageDb.Percent10,
                    Percent15 = x.secPercentageDb.Percent15
                }).ToList();
            return info;
        }





        public SecurityPercentageStatistic PercentageChangeGetTasks(string taskName)
        {
            return _context.SecurityPercentageStatistics.FirstOrDefault();// x => x.TaskName == taskName);
        }
        public void PercentageChangeUpdateTasks(SecurityPercentageStatistic task)
        {
            _context.SecurityPercentageStatistics.Update(task);
            Save();
        }


        public List<AutoSecurityTrade> ProcessAutoSecurityTrades(List<AutoSecurityTrade> securityTrades)
        {
            for (int i = 0; i < securityTrades.Count; i++)
            {

                if (securityTrades[i].Id > 0)
                {
                    UpdateSecurityTradeHistory(securityTrades[i]);

                }
                else if (SecurityTradesExists(securityTrades[i]))
                {
                    securityTrades.RemoveAt(i);
                    i--;
                }
                else
                {
                    AddSecurityTradeHistory(securityTrades[i]);
                }

            }
            return securityTrades;
        }

        public List<AutoSecurityTrade> GetRecommendedSecurityTrades(string securityTradeType)
        {
            DateTime currentDay = DateTime.Now;
            DateTime priorDay = currentDay.AddDays(-1);
            decimal perLoss = (decimal).4;


            switch (securityTradeType)
            {

                case "averagedrop":
                default:
                    return _context.Securities
                .Join(_context.SecurityPercentageStatistics, x => x.Id, y => y.SecurityId, (security, secPercentStats) => new { security, secPercentStats })
                .Where(x => x.security.Volume > 100000 && x.security.LastModified > priorDay && x.security.preferred == true &&  x.secPercentStats.AverageDrop != 0 &&
                ((x.security.CurrentPrice - x.security.PriorDayOpen) / x.security.PriorDayOpen) * 100 < x.secPercentStats.AverageDrop * (decimal)1.5)
                .Select(x =>
                new AutoSecurityTrade
                {
                    SecurityId = x.security.Id,
                    PurchaseDate = currentDay,
                    PurchasePrice = x.security.CurrentPrice,
                    PercentageLevel = 1,
                    SharesBought = 1
                }).ToList();
                case "percent15":
                    perLoss = (decimal).2;
                    return _context.Securities
                .Join(_context.SecurityPercentageStatistics, x => x.Id, y => y.SecurityId, (security, secPercentStats) => new { security, secPercentStats })
                .Where(x => x.security.Volume > 100000 && x.security.LastModified > priorDay && x.security.preferred == true &&  x.secPercentStats.AverageDrop != 0 &&
                ((x.security.CurrentPrice - x.security.PriorDayOpen) / x.security.PriorDayOpen) * 100 < x.secPercentStats.Percent15)
                .Select(x =>
                new AutoSecurityTrade
                {
                    SecurityId = x.security.Id,
                    PurchaseDate = currentDay,
                    PurchasePrice = x.security.CurrentPrice,
                    PercentageLevel = 2,
                    SharesBought = 2
                }).ToList();
                case "percent10":
                    perLoss = (decimal).1;
                    return _context.Securities
                .Join(_context.SecurityPercentageStatistics, x => x.Id, y => y.SecurityId, (security, secPercentStats) => new { security, secPercentStats })
                .Where(x => x.security.Volume > 100000 && x.security.LastModified > priorDay && x.security.preferred == true && x.secPercentStats.AverageDrop != 0 &&
                ((x.security.CurrentPrice - x.security.PriorDayOpen) / x.security.PriorDayOpen) * 100 < x.secPercentStats.Percent10)
                .Select(x =>
                new AutoSecurityTrade
                {
                    SecurityId = x.security.Id,
                    PurchaseDate = currentDay,
                    PurchasePrice = x.security.CurrentPrice,
                    PercentageLevel = 3,
                    SharesBought = 3
                }).ToList();
                case "percent5":
                    perLoss = (decimal)0;
                    return _context.Securities
                .Join(_context.SecurityPercentageStatistics, x => x.Id, y => y.SecurityId, (security, secPercentStats) => new { security, secPercentStats })
                .Where(x => x.security.Volume > 100000 && x.security.LastModified > priorDay && x.security.preferred == true && x.secPercentStats.AverageDrop != 0 &&
                ((x.security.CurrentPrice - x.security.PriorDayOpen) / x.security.PriorDayOpen) * 100 < x.secPercentStats.Percent5)
                .Select(x =>
                new AutoSecurityTrade
                {
                    SecurityId = x.security.Id,
                    PurchaseDate = currentDay,
                    PurchasePrice = x.security.CurrentPrice,
                    PercentageLevel = 4,
                    SharesBought = 4
                }).ToList();
                case "checkSellPoint":
                    decimal percentRaise = (decimal)1.01;

                    return _context.Securities
                        .Join(_context.AutoSecurityTrades,
                        x => x.Id, y => y.SecurityId, (security, tradeHistory) => new { security, tradeHistory })
                        .Where(x => x.security.CurrentPrice > x.tradeHistory.PurchasePrice * percentRaise && x.tradeHistory.SellDate == null
                        && x.security.LastModified > priorDay)
                        .Select(x =>
                new AutoSecurityTrade
                {
                    Id = x.tradeHistory.Id,
                    SecurityId = x.security.Id,
                    PurchaseDate = x.tradeHistory.PurchaseDate,
                    PurchasePrice = x.tradeHistory.PurchasePrice,
                    PercentageLevel = x.tradeHistory.PercentageLevel,
                    SharesBought = x.tradeHistory.SharesBought,
                    SellDate = currentDay,
                    SellPrice = x.security.CurrentPrice
                }).ToList();


            }



        }

        public bool SecurityTradesExists(AutoSecurityTrade securityTradeHistory)
        {
            DateTime? purchaseDate = securityTradeHistory.PurchaseDate;
            if (purchaseDate.HasValue)
            {


                purchaseDate = new DateTime(purchaseDate.Value.Year, purchaseDate.Value.Month, purchaseDate.Value.Day, 0, 0, 0);

                var securityTrade = _context.AutoSecurityTrades.Where(x => x.SecurityId == securityTradeHistory.SecurityId
                 && x.PurchaseDate > purchaseDate && x.PercentageLevel == securityTradeHistory.PercentageLevel).FirstOrDefault();

                if (securityTrade == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }

            }
            else
            {
                return true;
            }

        }

        public void UpdateSecurityTradeHistory(AutoSecurityTrade securityTradeHistory)
        {

            var entry = _context.AutoSecurityTrades.First(e => e.Id == securityTradeHistory.Id);
            _context.Entry(entry).CurrentValues.SetValues(securityTradeHistory);
            Save();
        }


        public void AddSecurityAlert(SecurityAlert securityAlert)
        {

            _context.SecurityAlerts.Add(securityAlert);
            Save();
        }
        
        public void AddSecurityTradeHistory(AutoSecurityTrade securityTradeHistory)
        {

            _context.AutoSecurityTrades.Add(securityTradeHistory);
            Save();
        }


        public List<Security> SecurityAlertCheck(SecurityAlertType securityAlertType)
        {

            var collection = _context.Securities as IQueryable<Security>;

            DateTime dateRecorded = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 0, 0, 0, DateTimeKind.Utc);
            if (securityAlertType.PercentageCheck > 0)
            {
                collection = collection.Where(x => x.PercentageChange > securityAlertType.PercentageCheck  && x.LastModified > dateRecorded);
            }
            else
            {
                collection = collection.Where(x => x.PercentageChange < securityAlertType.PercentageCheck && x.LastModified > dateRecorded);
            }

            if (securityAlertType.preferred)
            {
                collection = collection.Where(x => x.preferred == true);
            }


            collection = collection.Where(x => x.Volume > 100000);


            return collection.ToList();


        }

        public SecurityAlertType GetSecurityAlertType(int id)
        {
            return _context.SecurityAlertTypes.Where(x => x.Id == id).First();
        }


        public string ConvertStringScreenerAlertTypeMessage(List<StockScreenerAlertsHistoryDto> stockScreenerAlertsHistoryRecords, StockScreenerAlertType screenAlertsType)
        {
            StringBuilder messageString = new StringBuilder();

            var securityList = stockScreenerAlertsHistoryRecords.Select(x => x.SecurityId).ToList();
            var securityRecs =   _context.Securities.Where(x => securityList.Contains(x.Id)).OrderBy(x=>x.Name).ToList();
            

            foreach (var securityRec in securityRecs)
            {

                switch (screenAlertsType.AlertType)
                {
                    case "EMAIL":
                        messageString.Append(Environment.NewLine +
                     (securityRec.Name)
                     + "(" + securityRec.Symbol + ") " + securityRec.CurrentPrice.ToString() + "(" + securityRec.PercentageChange.ToString() + "%) Percent from Year Low: "
                     + decimal.Round(((decimal)(((securityRec.YearLow - securityRec.CurrentPrice) / securityRec.YearLow) * 100)), 2).ToString() + "% Percent from Year High: " +
                    decimal.Round(((decimal)(((securityRec.CurrentPrice - securityRec.YearHigh) / securityRec.YearHigh) * 100)), 2).ToString()
                     );
                        break;
                    case "SMS":
                    default:
                        messageString.Append(Environment.NewLine +
                       (securityRec.Name.Length > 15 ? securityRec.Name.Substring(0, 15) + ".." : securityRec.Name)
                       + "(" + securityRec.Symbol + ") " + securityRec.CurrentPrice.ToString() + "(" + securityRec.PercentageChange.ToString() + "%)");
                        break;

                }
            }



            return messageString.ToString();
        }

        public string ConvertStringSecurityAlertCheck(List<Security> securities)
        {
            StringBuilder messageString = new StringBuilder();

            foreach(var security in securities)
            {
                messageString.Append(Environment.NewLine + security.Symbol + "(" + security.PercentageChange.ToString() + ") ");
            }
            


            return messageString.ToString();
        }



        public bool SecurityAlertTradesExists(SecurityAlert securityAlert)
        {


            DateTime dateRecorded = new DateTime(securityAlert.dateRecorded.Year, securityAlert.dateRecorded.Month, securityAlert.dateRecorded.Day, 0, 0, 0);
            var results = _context.SecurityAlerts.Where(x => x.SecurityId == securityAlert.SecurityId 
            && x.dateRecorded > dateRecorded 
            && x.alertType == securityAlert.alertType).ToList();


            return results.Count() > 0;

           
        }









        public List<Security> ProcessSecurityAlerts(List<Security> securities, SecurityAlertType securityAlertType)
        {
            for (int i = 0; i < securities.Count; i++)
            {

                DateTime dateRecorded = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, DateTime.UtcNow.Second, DateTimeKind.Utc);
                SecurityAlert securityAlert = new SecurityAlert();
                securityAlert.SecurityId = securities[i].Id;
                securityAlert.alertType = securityAlertType.Id;
                securityAlert.dateRecorded = dateRecorded;


                if (SecurityAlertTradesExists(securityAlert))
                {
                    securities.RemoveAt(i);
                    i--;
                }
                else
                {
                    AddSecurityAlert(securityAlert);
                }

            }
            return securities;
        }





       

        public void UpsertCurrentPeakRanges(List<CurrentPeakRangeDto> currentPeakRanges)
        {
            List<CurrentPeakRange> currentPeakRangesInDb = _context.CurrentPeakRanges.ToList();
            List<CurrentPeakRange> existingPeakRangeDetails = GetCurrentCurrentPeakRanges(currentPeakRangesInDb, currentPeakRanges);


            var updatedAlready = existingPeakRangeDetails.Join(currentPeakRangesInDb, x => x.SecurityId,
               y => y.SecurityId, (query1, query2) => new { query1, query2 }).Where(o => o.query1.RangeName == o.query2.RangeName
               && o.query1.RangeDateStart == o.query2.RangeDateStart
               && o.query1.RangeLength == o.query2.RangeLength
                && o.query1.PeakRangeCurrentPercentage == o.query2.PeakRangeCurrentPercentage
                && o.query1.LastOpenHigh == o.query2.LastOpenHigh

               ).Select(x => x.query1).ToList();


            existingPeakRangeDetails = existingPeakRangeDetails.Except(updatedAlready).ToList();


            _utility.UpdateRecords(existingPeakRangeDetails,_context);



            var newRecords = currentPeakRanges.Join(currentPeakRangesInDb, x => x.SecurityId,
           y => y.SecurityId, (query1, query2) => new { query1, query2 }).Select(x => x.query1);


            currentPeakRanges = currentPeakRanges.Except(newRecords).ToList();
            List<CurrentPeakRange> newcurrentPeakRange = _mapper.Map<List<CurrentPeakRange>>(currentPeakRanges).ToList();

            if (newcurrentPeakRange.Count > 0)
            {
                _utility.AddRecords(newcurrentPeakRange,_context);
            }

        }


        private List<CurrentPeakRange> GetCurrentCurrentPeakRanges(List<CurrentPeakRange> currentPeakRangesInDb, List<CurrentPeakRangeDto> currentPeakRanges)
        {

            List<CurrentPeakRange> existingPeakRangeDetails = currentPeakRanges.Join(currentPeakRangesInDb, x => x.SecurityId,
              y => y.SecurityId, (query1, query2) => new { query1, query2 }).Select(x => new CurrentPeakRange
              { 
                  SecurityId = x.query1.SecurityId,
                  Id = x.query2.Id,
                  DateCreated = x.query2.DateCreated,
                  DateModified = x.query1.DateModified,
                  RangeName = x.query1.RangeName,
                  RangeLength = x.query1.RangeLength,
                  RangeDateStart = x.query1.RangeDateStart,
                  PeakRangeCurrentPercentage = x.query1.PeakRangeCurrentPercentage,
                  LastOpenHigh = x.query1.LastOpenHigh


              }).ToList();


            return existingPeakRangeDetails;
        }

        public void UpsertSecurityProfile(List<SecurityForUpdateDto> securities)
        {
         
            List<Security> dbSecurities = GetProfileSecurities(securities);

            var securityTable = _context.Securities.Select(x=> new SecurityUpdateProfile { 
                Id= x.Id, 
                Description =x.Description, 
                IPODate = x.IPODate,
                Symbol = x.Symbol
            }).ToList();




            var updatedAlready = dbSecurities.Join(securityTable, x => x.Id,
              y => y.Id, (query1, query2) => new { query1, query2 }).Where(o =>
              o.query1.Description == o.query2.Description
              && o.query1.IPODate == o.query2.IPODate


              ).Select(x => x.query1).ToList();


            var updatedbSecurities = dbSecurities.Except(updatedAlready).ToList();



            List<Security> update = new List<Security>();
            foreach (Security security in updatedbSecurities)
            {
                if (securityTable.Where(x => x.Symbol == security.Symbol
              ).Count() > 0)
                {
                    update.Add(security);
                }
            }
            BulkSaveUpdate(update);



        }


        private List<Security> GetProfileSecurities(List<SecurityForUpdateDto> securities)
        {


            List<Security> security = _context.Securities.ToList();

            security = securities.Join(security, x => x.Symbol, y => y.Symbol, (query1, query2) => new { query1, query2 })
                .Where(x => x.query1.SecurityType == x.query2.SecurityType)
                .Select(x => new Security
                {
                    IPODate = x.query1.IPODate,
                    Description = x.query1.Description,
                    Id = x.query2.Id,
                    Symbol = x.query1.Symbol,
                    Name = x.query2.Name,
                    DayHigh = Decimal.Round(x.query2.DayHigh.Value, 2),
                    DayLow = Decimal.Round(x.query2.DayLow.Value, 2),
                    YearHigh = Decimal.Round(x.query2.YearHigh.Value, 2),
                    YearLow = Decimal.Round(x.query2.YearLow.Value, 2),
                    CurrentPrice = Decimal.Round(x.query2.CurrentPrice, 2),

                    EarningsDate = x.query2.EarningsDate,
                    Volume = x.query2.Volume,
                    PriorDayOpen = Decimal.Round(x.query2.PriorDayOpen.Value, 2),
                    LastModified = x.query1.LastModified,
                    SecurityType = x.query2.SecurityType,
                    Industry = x.query2.Industry,
                    Sector = x.query2.Sector,
                    PercentageChange = x.query2.PercentageChange,
                    preferred = x.query2.preferred,
                    excludeHistorical = x.query2.excludeHistorical,
                    IPOYear = x.query2.IPOYear,
                    Dividend = x.query2.Dividend,
                    DividendDate = x.query2.DividendDate


                }).ToList();
            return security;
        }



        private List<PeakRangeDetail> GetCurrentPeakRangeDetails(List<PeakRangeDetail> currentPeakRangeDetails,List<PeakRangeDetailDto> peakRangeDetails)
        {

            
            

            List<PeakRangeDetail> existingPeakRangeDetails = peakRangeDetails.Join(currentPeakRangeDetails, x => x.SecurityId,
              y => y.SecurityId, (query1, query2) => new { query1, query2 }).Where(o => o.query1.RangeName == o.query2.RangeName

              ).Select(x => new PeakRangeDetail
              {
                  SecurityId = x.query1.SecurityId,
                  Id = x.query2.Id,
                  DateCreated = x.query2.DateCreated,
                  DateModified = x.query1.DateModified,
                  RangeName = x.query1.RangeName,
                  RangeCount = x.query1.RangeCount,
                  RangeLength = x.query1.RangeLength,
                  MaxRangeLength = x.query1.MaxRangeLength,
                  MaxRangeDateStart = x.query1.MaxRangeDateStart,
                  MaxRangeDateEnd = x.query1.MaxRangeDateEnd
                  /*
  
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public int RangeCount { get; set; }
        public int RangeLength { get; set; }
        public int MaxRangeLength { get; set; }
        public DateTime MaxRangeDateStart { get; set; }
        public DateTime MaxRangeDateEnd { get; set; }
 }
                   */

              }).ToList();


            return existingPeakRangeDetails;
        }



        public void UpsertPeakRangeDetails(List<PeakRangeDetailDto> peakRangeDetails)
        {

            List<PeakRangeDetail> currentPeakRangeDetails = _context.PeakRangeDetails.ToList();
            List<PeakRangeDetail> existingPeakRangeDetails = GetCurrentPeakRangeDetails(currentPeakRangeDetails, peakRangeDetails);


            var updatedAlready = existingPeakRangeDetails.Join(currentPeakRangeDetails, x => x.SecurityId,
               y => y.SecurityId, (query1, query2) => new { query1, query2 }).Where(o => o.query1.RangeName == o.query2.RangeName
               && o.query1.MaxRangeDateEnd == o.query2.MaxRangeDateEnd
               && o.query1.MaxRangeDateStart == o.query2.MaxRangeDateStart
               && o.query1.MaxRangeLength == o.query2.MaxRangeLength
               && o.query1.RangeCount == o.query2.RangeCount
               && o.query1.RangeLength == o.query2.RangeLength
               ).Select(x => x.query1).ToList();


            existingPeakRangeDetails = existingPeakRangeDetails.Except(updatedAlready).ToList();


            _utility.UpdateRecords(existingPeakRangeDetails,_context);



            var newRecords = peakRangeDetails.Join(currentPeakRangeDetails, x => x.SecurityId,
           y => y.SecurityId, (query1, query2) => new { query1, query2 }).Where(o => o.query1.RangeName == o.query2.RangeName
           ).Select(x => x.query1);


            peakRangeDetails = peakRangeDetails.Except(newRecords).ToList();
            List<PeakRangeDetail> newpeakRangeDetails = _mapper.Map<List<PeakRangeDetail>>(peakRangeDetails).ToList();

            if (newpeakRangeDetails.Count > 0)
            {
                _utility.AddRecords(newpeakRangeDetails,_context);
            }







        }

        
    }
}

/*
 *  Security record = new Security();

            Type type = record.GetType();
            string purchtype = type.Name;
            //To restrict return properties. If all properties are required don't provide flag.
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            PropertyInfo[] properties = type.GetProperties(flags);

            foreach (PropertyInfo property in properties)
            {
                
               var types = property.GetCustomAttributes();
                foreach(var t in types)
                {
                    if (t.GetType().Name== "KeyAttribute"){
                        //This Property is the key
                    }
                    
                }
                Console.WriteLine("Name: " + property.Name + ", Value: " + property.GetValue(record, null));
                var detail = property.GetValue(record, null);
                var info = property.PropertyType.Name;
                
                    if(info == "Nullable`1")
                {
                    var genericTypes = property.PropertyType.GenericTypeArguments;
                    string nullableTypeName;
                    foreach(var genericType in genericTypes)
                    {
                        nullableTypeName = genericType.Name;
                    }
                }
                //Int32
                //DateTime
                //Decimal
                //String == null
                //Nullable`1
                //Boolean
            }

*/