using AutoMapper;
using Microsoft.EntityFrameworkCore;
using sqs_processor.DbContexts;
using sqs_processor.Entities;
using sqs_processor.Models;
using sqs_processor.ResourceParameters;
using sqs_processor.Services.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sqs_processor.Services.repos
{
  public  class DividendsRepository : IDividendsRepository
    {
        private readonly SecuritiesLibraryContext _context;
        private readonly IMapper _mapper;
        private readonly IUtility _utility;

        public DividendsRepository(SecuritiesLibraryContext context,
            //IConfiguration config,
            IMapper mapper, IUtility utility)
        {

            _context = context ?? throw new ArgumentNullException(nameof(context));
            // _config = config ?? throw new ArgumentNullException(nameof(config));
            _mapper = mapper;
            _utility = utility;
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




        public IEnumerable<Dividend> GetDividends(int securityId)
        {
            return _context.Dividends.Where(x => x.SecurityId == securityId);
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


        public IEnumerable<Tuple<Dividend, Security>> GetSecuritiesDividends(IEnumerable<Dividend> dividends)
        {

            return dividends.Join(_context.Securities
                , x => x.SecurityId,
              y => y.Id, (query1, query2) => new { query1, query2 }).Select(x => new Tuple<Dividend, Security>(x.query1, x.query2)).ToList();

        }






        public void UpdateDividends(List<DividendDto> dividends)
        {
            if (dividends.Count == 0)
            {
                return;
            }
            DateTime searchDate = dividends[dividends.Count-1].PayableDate.AddDays(-60);
            var securityRecs = dividends.GroupBy(x => x.SecurityId).Select(g=>g.Key).ToList();

            List<Dividend> currentDividends = _context.Dividends.Where(x => securityRecs.Contains(x.SecurityId)).ToList();




            UpdateDividends(dividends, currentDividends);
        }


        /*
        public void UpdateDividends(List<DividendDto> dividends, Security security)
        {

            if (dividends.Count == 0)
            {
                return;
            }

            List<Dividend> currentDividends = _context.Dividends.Where(x => x.SecurityId == dividends[0].SecurityId).ToList();



            UpdateDividends(dividends, currentDividends);
        }
        */




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



        private void dividendsAdd(List<Dividend> dividends)
        {
            //  _context.BulkInsert(dividends);
            _utility.AddRecords(dividends,_context);
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





        public bool Save()
        {

            return (_context.SaveChanges() >= 0);
        }














    }
}
