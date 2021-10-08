using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using sqs_processor.DbContexts;
using sqs_processor.Entities;
using sqs_processor.Models;
using sqs_processor.ResourceParameters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace sqs_processor.Services.repos
{
    public class SecuritiesRepository : ISecuritiesRepository, IDisposable
    {
        private readonly IConfiguration _config;

        private readonly SecuritiesLibraryContext _context;
        private readonly IMapper _mapper;

        public SecuritiesRepository(SecuritiesLibraryContext context, IConfiguration config, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mapper = mapper;
        }

        public IEnumerable<Dividend> GetDividends(int securityId)
        {
            return _context.Dividends.Where(x => x.SecurityId == securityId);
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

        public Dividend GetDividend(int securityId, DateTime exDividendDate)
        {

            if (securityId == 0)
            {
                throw new ArgumentNullException(nameof(securityId));
            }


            return _context.Dividends
              .Where(c => c.SecurityId == securityId && c.ExDividendDate == exDividendDate).FirstOrDefault();
        }




        public Dividend GetDividend(int securityId, int dividendId)
        {

            if (securityId == 0)
            {
                throw new ArgumentNullException(nameof(securityId));
            }

            if (dividendId == 0)
            {
                throw new ArgumentNullException(nameof(dividendId));
            }

            return _context.Dividends
              .Where(c => c.SecurityId == securityId && c.Id == dividendId).FirstOrDefault();
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


            //.Join(_context.Securities, x => x.x.StockId, security => security.Id, (x, security) => new { x.x, security }).Select(x => new Tuple<Earning, Security>(x.x, x.security)).ToList();

        }

        public IEnumerable<Earning> GetEarnings(EarningsResourceParameters earningsResourceParameters)
        {
            /*
                   public int stockId { get; set; }
        public DateTime actualEarningsDate { get; set; }
        public DateTime rangeStartEarningsDate { get; set; }
        public DateTime rangeEndEarningsDate { get; set; }
        public string searchQuery { get; set; }
            */


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



        public List<DividendDto> GetDividends(List<DividendDto> dividends)
        {


            var security = _context.Securities.Select(x => new Security { Symbol = x.Symbol, Id = x.Id });

            dividends = dividends.Join(security, x => x.symbol, y => y.Symbol, (query1, query2) => new { query1, query2 }).Select(x => new DividendDto
            {

                SecurityId = x.query2.Id,
                symbol = x.query1.symbol,
                AnnouncementDate = x.query1.AnnouncementDate,
                Frequency = x.query1.Frequency,
                Amount = x.query1.Amount,
                Yield = x.query1.Yield,
                ExDividendDate = x.query1.ExDividendDate,
                RecordDate = x.query1.RecordDate,
                PayableDate = x.query1.PayableDate,




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
            return dividends;
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
                && !securitiesResourceParameters.minVolume.HasValue);
        }


        public IEnumerable<Security> GetSecurities(SecuritiesResourceParameters securitiesResourceParameters)
        {
            if (SearchAllSecurities(securitiesResourceParameters)
                    )
            {
                return _context.Securities.ToList<Security>();
            }
            var collection = _context.Securities as IQueryable<Security>;



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



        public IEnumerable<HistoricalPrice> GetHistoricalPrices(int securityId, HistoricalPricesResourceParameters historicalPriceResourceParameters)
        {
            var collection = _context.HistoricalPrices as IQueryable<HistoricalPrice>;


            collection = collection.Where(x => x.SecurityId == securityId);

            collection = collection.Where(x => x.HistoricDate >= historicalPriceResourceParameters.HistoricDateLow && x.HistoricDate <= historicalPriceResourceParameters.HistoricDateHigh);

            return collection.ToList();
        }




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
            List<Earning> currentHistoricalEarnings = _context.Earnings.ToList();
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
          y => y.SecurityId, (query1, query2) => new { query1, query2 }).Where(o => o.query1.ActualEarningsDate == o.query2.ActualEarningsDate
          ).Select(x => x.query1);


            earnings = earnings.Except(newRecords).ToList();
            List<Earning> newEarnings = _mapper.Map<List<Earning>>(earnings).ToList();

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


        public void UpdateDividends(List<DividendDto> dividends)
        {
            if (dividends.Count == 0)
            {
                return;
            }
            DateTime searchDate = dividends[0].PayableDate.AddDays(-60);
            List<Dividend> currentDividends = _context.Dividends.Where(x => x.PayableDate > searchDate).ToList();
            UpdateDividends(dividends, currentDividends);
        }



        /// <summary>
        /// Gets the dividends from the database compared to what dividends are in there
        /// </summary>
        /// <param name="dividends"></param>
        /// <param name="currentDividends"></param>
        /// <returns></returns>
        private List<Dividend> GetDividends(List<DividendDto> dividends, List<Dividend> currentDividends)
        {

            List<Dividend> dividendChanges = dividends.Join(currentDividends, x => x.SecurityId,
                y => y.SecurityId, (query1, query2) => new { query1, query2 }).Where(o => o.query1.ExDividendDate == o.query2.ExDividendDate

                ).Select(x => new Dividend
                {
                    SecurityId = x.query1.SecurityId,
                    Id = x.query2.Id,
                    AnnouncementDate = x.query1.AnnouncementDate,
                    Frequency = x.query1.Frequency,
                    Amount = x.query1.Amount,
                    Yield = x.query1.Yield,
                    RecordDate = x.query1.RecordDate,
                    PayableDate = x.query1.PayableDate,
                    ExDividendDate = x.query1.ExDividendDate

                }).ToList();


            return dividendChanges;

        }

        private void UpdateDividends(List<DividendDto> dividends, List<Dividend> currentDividends)
        {




            List<Dividend> existingDividends = GetDividends(dividends, currentDividends);


            var updatedAlready = existingDividends.Join(currentDividends, x => x.SecurityId,
               y => y.SecurityId, (query1, query2) => new { query1, query2 }).Where(o => o.query1.ExDividendDate == o.query2.ExDividendDate
               && o.query1.Amount == o.query2.Amount
               && o.query1.AnnouncementDate == o.query2.AnnouncementDate
               && o.query1.Yield == o.query2.Yield
               && o.query1.RecordDate == o.query2.RecordDate
               && o.query1.PayableDate == o.query2.PayableDate
               ).Select(x => x.query1).ToList();


            existingDividends = existingDividends.Except(updatedAlready).ToList();


            dividendsUpdate(existingDividends);



            var newRecords = dividends.Join(currentDividends, x => x.SecurityId,
           y => y.SecurityId, (query1, query2) => new { query1, query2 }).Where(o => o.query1.ExDividendDate == o.query2.ExDividendDate
           ).Select(x => x.query1);


            dividends = dividends.Except(newRecords).ToList();
            List<Dividend> newDividends = _mapper.Map<List<Dividend>>(dividends).ToList();

            if (newDividends.Count > 0)
            {
                dividendsAdd(newDividends);
            }


        }







        public void UpdateDividends(List<DividendDto> dividends, Security security)
        {

            if (dividends.Count == 0)
            {
                return;
            }

            List<Dividend> currentDividends = _context.Dividends.Where(x => x.SecurityId == dividends[0].SecurityId).ToList();



            UpdateDividends(dividends, currentDividends);
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


        public void UpsertHistoricalPrices(List<HistoricalPriceforUpdateDto> historicalPrices, Security security)
        {

            if (historicalPrices.Count == 0)
            {
                return;
            }

            List<HistoricalPrice> currentHistoricalPrices = _context.HistoricalPrices.Where(x => x.SecurityId == historicalPrices[0].SecurityId).ToList();


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


        }

        private void dividendsUpdate(List<Dividend> dividends)
        {

            //  _context.BulkUpdate(dividends);

            int sercurityLoop = 0;
            string sqlCall = "";
            foreach (Dividend dividend in dividends)
            {

                //Id, SecurityId, AnnouncementDate, Frequency, Amount, Yield, ExDividendDate, RecordDate, PayableDate
                sqlCall += "UPDATE Dividends SET Frequency = '" + dividend.Frequency + "', Amount= " + dividend.Amount + " , " +
                "Yield= " + dividend.Yield + ", ExDividendDate= '" +
                dividend.ExDividendDate.ToString("yyyy-MM-dd") + "'" + ", RecordDate = '" + dividend.RecordDate.ToString("yyyy-MM-dd") + "'" +
                ", PayableDate= '" + dividend.PayableDate.ToString("yyyy-MM-dd") + "'" +
                " WHERE " +
                " SecurityId = " + dividend.SecurityId +
                " AND AnnouncementDate = '" + dividend.AnnouncementDate.ToString("yyyy-MM-dd") + "'; ";

                sercurityLoop += 1;
                if (sercurityLoop == 500)
                {
                    _context.Database.ExecuteSqlRaw(sqlCall);
                    sercurityLoop = 0;
                    sqlCall = "";
                }
            }

            if (sercurityLoop > 0)
            {
                _context.Database.ExecuteSqlRaw(sqlCall);
            }
        }

        private void historicPriceUpdate(List<HistoricalPrice> historicalPrices)
        {
            //   _context.BulkUpdate(historicalPrices);


            int sercurityLoop = 0;
            string sqlCall = "";
            foreach (HistoricalPrice historicalPrice in historicalPrices)
            {

                // Id, SecurityId, Open, Close, High, Low, Volume, HistoricDate, PercentChange

                sqlCall += "UPDATE HistoricalPrices SET Open = " + historicalPrice.Open + ", Close= " + historicalPrice.Close + " , " +
                "High= " + historicalPrice.High + ", Low= " +
                historicalPrice.Low + ", Volume= " + historicalPrice.Volume +
                ", PercentChange= " + historicalPrice.PercentChange +
                " WHERE " +
                " SecurityId = " + historicalPrice.SecurityId +
                " AND HistoricDate = '" + historicalPrice.HistoricDate.ToString("yyyy-MM-dd") + "'; ";

                sercurityLoop += 1;
                if (sercurityLoop == 500)
                {
                    _context.Database.ExecuteSqlRaw(sqlCall);
                    sercurityLoop = 0;
                    sqlCall = "";
                }
            }

            if (sercurityLoop > 0)
            {
                _context.Database.ExecuteSqlRaw(sqlCall);
            }

        }

        private void historicPriceAdd(List<HistoricalPrice> historicalPrices)
        {

            //  _context.BulkInsert(historicalPrices);

            int sercurityLoop = 0;
            string sqlCall = "";
            foreach (HistoricalPrice historicalPrice in historicalPrices)
            {

                // Id, SecurityId, Open, Close, High, Low, Volume, HistoricDate, PercentChange
                if (sqlCall == "")
                {
                    sqlCall += "INSERT INTO HistoricalPrices (SecurityId, Open, Close, High, Low, Volume, HistoricDate, PercentChange) VALUES";
                }
                else
                {
                    sqlCall += ",";
                }

                sqlCall += "(" + historicalPrice.SecurityId + ", " +
                    historicalPrice.Open + ", " +
                    historicalPrice.Close + ", " +
                    historicalPrice.High + ", " +
                    historicalPrice.Low + ", " +
                    historicalPrice.Volume + ", " +
                    "'" + historicalPrice.HistoricDate.ToString("yyyy-MM-dd") + "', " +
                    historicalPrice.PercentChange +


                    ")";
                //" AND HistoricDate = '" + historicalPrice.HistoricDate.ToString("yyyy-MM-dd") + "'; ";

                sercurityLoop += 1;
                if (sercurityLoop == 500)
                {

                    _context.Database.ExecuteSqlRaw(sqlCall);
                    sqlCall = "";
                    sercurityLoop = 0;
                }
            }

            if (sercurityLoop > 0)
            {
                _context.Database.ExecuteSqlRaw(sqlCall);
            }


        }

        private void dividendsAdd(List<Dividend> dividends)
        {
            //  _context.BulkInsert(dividends);

            int sercurityLoop = 0;
            string sqlCall = "";
            foreach (Dividend dividend in dividends)
            {

                // Id, SecurityId, Open, Close, High, Low, Volume, HistoricDate, PercentChange
                if (sqlCall == "")
                {
                    sqlCall += "INSERT INTO Dividends (SecurityId, AnnouncementDate, Amount, Yield, ExDividendDate, RecordDate, PayableDate) VALUES";
                }
                else
                {
                    sqlCall += ",";
                }

                sqlCall += "(" + dividend.SecurityId + ", " +
                    "'" + dividend.AnnouncementDate.ToString("yyyy-MM-dd") + "', " +
                    dividend.Amount + ", " +
                    dividend.Yield + ", " +
                    "'" + dividend.ExDividendDate.ToString("yyyy-MM-dd") + "', " +
                    "'" + dividend.RecordDate.ToString("yyyy-MM-dd") + "', " +
                    "'" + dividend.PayableDate.ToString("yyyy-MM-dd") + "' " +



                    ")";
                //" AND HistoricDate = '" + historicalPrice.HistoricDate.ToString("yyyy-MM-dd") + "'; ";

                sercurityLoop += 1;
                if (sercurityLoop == 500)
                {

                    _context.Database.ExecuteSqlRaw(sqlCall);
                    sqlCall = "";
                    sercurityLoop = 0;
                }
            }

            if (sercurityLoop > 0)
            {
                _context.Database.ExecuteSqlRaw(sqlCall);
            }


        }


        private void earningsUpdate(List<Earning> earnings)
        {
            //  _context.BulkUpdate(earnings);


            int sercurityLoop = 0;
            string sqlCall = "";
            foreach (Earning earning in earnings)
            {

                // Id, SecurityId, ActualEarningsDate, EPSEstimate, ReportedEPS, GAAPEPS, RevenueEstimate, ActualRevenue, ReportTime
                sqlCall += "UPDATE Dividends SET EPSEstimate = " + earning.EPSEstimate + ", ReportedEPS= " + earning.ReportedEPS + " , " +
                "GAAPEPS= " + earning.GAAPEPS + ", RevenueEstimate= " +
                earning.RevenueEstimate + ", ActualRevenue = " + earning.ActualRevenue + "" +
                ", ReportTime= '" + earning.ReportTime + "'" +
                " WHERE " +
                " SecurityId = " + earning.SecurityId +
                " AND ActualEarningsDate = '" + earning.ActualEarningsDate.ToString("yyyy-MM-dd") + "'; ";

                sercurityLoop += 1;
                if (sercurityLoop == 500)
                {
                    _context.Database.ExecuteSqlRaw(sqlCall);
                    sercurityLoop = 0;
                    sqlCall = "";
                }
            }

            if (sercurityLoop > 0)
            {
                _context.Database.ExecuteSqlRaw(sqlCall);
            }


        }


        private void earningsAdd(List<Earning> earnings)
        {
            //   _context.BulkInsert(earnings);

            int sercurityLoop = 0;
            string sqlCall = "";
            foreach (Earning earning in earnings)
            {

                // Id, SecurityId, ActualEarningsDate, EPSEstimate, ReportedEPS, GAAPEPS, RevenueEstimate, ActualRevenue, ReportTime
                if (sqlCall == "")
                {
                    sqlCall += "INSERT INTO Earnings (SecurityId, ActualEarningsDate, EPSEstimate, ReportedEPS, GAAPEPS, RevenueEstimate, ActualRevenue, ReportTime) VALUES";
                }
                else
                {
                    sqlCall += ",";
                }

                sqlCall += "(" + earning.SecurityId + ", " +
                    "'" + earning.ActualEarningsDate.ToString("yyyy-MM-dd") + "', " +
                    earning.EPSEstimate + ", " +
                    earning.ReportedEPS + ", " +
                    earning.GAAPEPS + ", " +
                    earning.RevenueEstimate + ", " +
                    earning.ActualRevenue + ", " +
                    "'" + earning.ReportTime + "' " +



                    ")";
                //" AND HistoricDate = '" + historicalPrice.HistoricDate.ToString("yyyy-MM-dd") + "'; ";

                sercurityLoop += 1;
                if (sercurityLoop == 500)
                {

                    _context.Database.ExecuteSqlRaw(sqlCall);
                    sqlCall = "";
                    sercurityLoop = 0;
                }
            }

            if (sercurityLoop > 0)
            {
                _context.Database.ExecuteSqlRaw(sqlCall);
            }


        }




        /*
        public void DynamicQuery(string sql)
        {
            string connectionString = _config.GetConnectionString("FinancialServices");
            //  using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["FinancialServices"].ConnectionString))
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                db.Execute(sql);
            }
        }
        */


        public HistoricalPrice GetHistoricPrice(int securityId, DateTime historicDate)
        {
            if (securityId == 0)
            {
                throw new ArgumentNullException(nameof(securityId));
            }

            // return _context.Authors.FirstOrDefault(a => a.Id == authorId);

            return _context.HistoricalPrices.FirstOrDefault(x => x.SecurityId == securityId && x.HistoricDate == historicDate);
        }






        private List<Security> GetSecurities(List<SecurityForUpdateDto> earnings)
        {


            List<Security> security = _context.Securities.ToList();

            security = earnings.Join(security, x => x.Symbol, y => y.Symbol, (query1, query2) => new { query1, query2 }).Select(x => new Security
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
                preferred = x.query2.preferred





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

              ).Select(x => x.query1);


            dbSecurities = dbSecurities.Except(updatedAlready).ToList();



            List<Security> update = new List<Security>();
            foreach (Security security in dbSecurities)
            {
                if (securityTable.Where(x => x.Symbol == security.Symbol
              ).Count() > 0)
                {
                    update.Add(security);
                }
            }
            BulkSaveUpdate(update);



        }


        public void BulkSaveUpdate(List<Security> securities)
        {
            /*
            _context.ChangeTracker.AutoDetectChangesEnabled = false;
            int info = 1;
            foreach (Security security in securities)
            {
                
                var entry = _context.Securities.First(e => e.Id == security.Id);
                _context.Entry(entry).CurrentValues.SetValues(security);
                Save();
                info += 1;
                if (info > 400)
                {
                    break;
                }
                //var securityRec = _context.Securities.Where()
                //_context.Update(security);
            }
            _context.ChangeTracker.AutoDetectChangesEnabled = true;
            */

            int sercurityLoop = 0;
            string sqlCall = "";
            foreach (Security security in securities)
            {

                sqlCall += "UPDATE Securities SET CurrentPrice = " + security.CurrentPrice + ", YearLow= " + security.YearLow + " , " +
                "YearHigh= " + security.YearHigh + ", Volume= " +
                security.Volume + ", DayLow= " + security.DayLow +
                ", DayHigh= " + security.DayHigh +
                ", LastModified ='" + security.LastModified.Value.ToString("yyyy-MM-dd HH:mm:ss") + "'" +
                ", PriorDayOpen=" + security.PriorDayOpen +
                ", PercentageChange=" + security.PercentageChange +
                " WHERE " +
                " Id = " + security.Id + "; ";

                sercurityLoop += 1;
                if (sercurityLoop == 500)
                {

                    _context.Database.ExecuteSqlRaw(sqlCall);
                    sercurityLoop = 0;
                    sqlCall = "";
                }
            }

            if (sercurityLoop > 0)
            {
                _context.Database.ExecuteSqlRaw(sqlCall);
            }




            //  _context.BulkUpdate(securities);



        }

        public IEnumerable<Tuple<Earning, Security>> GetSecuritiesEarnings(IEnumerable<Earning> earnings)
        {
            return earnings.Join(_context.Securities
                , x => x.SecurityId,
              y => y.Id, (query1, query2) => new { query1, query2 }).Select(x => new Tuple<Earning, Security>(x.query1, x.query2)).ToList();
        }
        public IEnumerable<Tuple<Dividend, Security>> GetSecuritiesDividends(IEnumerable<Dividend> dividends)
        {

            return dividends.Join(_context.Securities
                , x => x.SecurityId,
              y => y.Id, (query1, query2) => new { query1, query2 }).Select(x => new Tuple<Dividend, Security>(x.query1, x.query2)).ToList();

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

        public IEnumerable<Dividend> GetDividends(DividendsResourceParameters dividendsResourceParameters)
        {

            if (dividendsResourceParameters.exDividendDate == DateTime.MinValue
               && dividendsResourceParameters.rangeExDividendDateEnd == DateTime.MinValue
               && dividendsResourceParameters.rangeExDividendDateStart == DateTime.MinValue
               && string.IsNullOrWhiteSpace(dividendsResourceParameters.searchQuery)
               && dividendsResourceParameters.securityId == 0

                   )
            {
                return _context.Dividends.ToList<Dividend>();
            }


            var collection = _context.Dividends as IQueryable<Dividend>;



            if (dividendsResourceParameters.exDividendDate != DateTime.MinValue)
            {

                collection = collection.Where(a => a.ExDividendDate == dividendsResourceParameters.exDividendDate);
            }

            if (dividendsResourceParameters.rangeExDividendDateStart != DateTime.MinValue)
            {

                collection = collection.Where(a => a.ExDividendDate != null && a.ExDividendDate >= dividendsResourceParameters.rangeExDividendDateStart);
            }

            if (dividendsResourceParameters.rangeExDividendDateEnd != DateTime.MinValue)
            {

                collection = collection.Where(a => a.ExDividendDate != null && a.ExDividendDate <= dividendsResourceParameters.rangeExDividendDateEnd);
            }


            if (dividendsResourceParameters.securityId != 0)
            {

                collection = collection.Where(a => a.SecurityId == dividendsResourceParameters.securityId);
            }


            return collection.ToList();


            /*
            //return _context.Earnings.Where(x => x.ActualEarningsDate > earningsResourceParameters.actualEarningsDate).Join(_context.PreferredSecurities, x => x.StockId, security => security.Id, (x, security) => new { x, security }).Select(x =>  x.x }).ToList();
            return _context.Earnings.Where(x => x.ActualEarningsDate > earningsResourceParameters.actualEarningsDate)
               // .Join(_context.PreferredSecurities, x => x.StockId, security => security.StockId, (x, security) => new { x, security })
                .Join(_context.Securities.Where(x=>x.preferred==true), x => x.SecurityId, security => security.Id , (x,security) => new { x,security}).Select(x =>  new Tuple<Earning,Security>(x.x , x.security ) ).ToList();
            */
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

        public SecurityTasks GetTasks(string taskName)
        {
            return _context.SecurityTasks.FirstOrDefault(x => x.TaskName == taskName);
        }

        public void UpdateTasks(SecurityTasks task)
        {
            _context.SecurityTasks.Update(task);
            Save();
        }












        public SecurityPercentageStatistics PercentageChangeGetTasks(string taskName)
        {
            return _context.SecurityPercentageStatistics.FirstOrDefault();// x => x.TaskName == taskName);
        }
        public void PercentageChangeUpdateTasks(SecurityPercentageStatistics task)
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
                .Join(_context.SecurityPercentageStatistics, x => x.Id, y => y.SecurityId, (security, tradeHistory) => new { security, tradeHistory })
                .Where(x => x.security.Volume > 100000 && x.security.LastModified > priorDay && x.security.preferred == true &&
                ((x.security.CurrentPrice - x.security.PriorDayOpen) / x.security.PriorDayOpen) * 100 < x.tradeHistory.AverageDrop - perLoss)
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
                .Join(_context.SecurityPercentageStatistics, x => x.Id, y => y.SecurityId, (security, tradeHistory) => new { security, tradeHistory })
                .Where(x => x.security.Volume > 100000 && x.security.LastModified > priorDay && x.security.preferred == true &&
                ((x.security.CurrentPrice - x.security.PriorDayOpen) / x.security.PriorDayOpen) * 100 < x.tradeHistory.Percent15 - perLoss)
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
                .Join(_context.SecurityPercentageStatistics, x => x.Id, y => y.SecurityId, (security, tradeHistory) => new { security, tradeHistory })
                .Where(x => x.security.Volume > 100000 && x.security.LastModified > priorDay && x.security.preferred == true &&
                ((x.security.CurrentPrice - x.security.PriorDayOpen) / x.security.PriorDayOpen) * 100 < x.tradeHistory.Percent10 - perLoss)
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
                .Join(_context.SecurityPercentageStatistics, x => x.Id, y => y.SecurityId, (security, tradeHistory) => new { security, tradeHistory })
                .Where(x => x.security.Volume > 100000 && x.security.LastModified > priorDay && x.security.preferred == true &&
                ((x.security.CurrentPrice - x.security.PriorDayOpen) / x.security.PriorDayOpen) * 100 < x.tradeHistory.Percent5 - perLoss)
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
                        .Where(x => x.security.CurrentPrice > x.tradeHistory.PurchasePrice * percentRaise && x.tradeHistory.SellDate == null)
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
           // _context.Entry(_context.Securities).Reload();
            DateTime dateRecorded = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 0, 0, 0, DateTimeKind.Utc);
            List<Security> securities = new List<Security>();
            if (securityAlertType.PercentageCheck > 0)
            {
                securities = _context.Securities.Where(x => x.PercentageChange > securityAlertType.PercentageCheck  && x.LastModified > dateRecorded).ToList();
            }
            else
            {
                securities = _context.Securities.Where(x => x.PercentageChange < securityAlertType.PercentageCheck && x.LastModified > dateRecorded).ToList();
            }

            if (securityAlertType.preferred)
            {
                securities = securities.Where(x => x.preferred == true).ToList();
            }



            return securities;


        }

        public SecurityAlertType GetSecurityAlertType(int id)
        {
            return _context.SecurityAlertTypes.Where(x => x.Id == id).First();
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










    }
}
