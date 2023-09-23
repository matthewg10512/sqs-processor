using sqs_processor.Models;
using sqs_processor.Services.Factories;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sqs_processor.Processes
{
    public class GroupStats
    {
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Average { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Max { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Min { get; set; }
    }
   public class ProcessSecurityAnalytics : IProcess
    {
        //private readonly ISecuritiesRepository _securityRepository;
        private IUnitOfWork _unitOfWork;
        private readonly IUnitofWorkFactory _unitOfWorkFactory;
        public ProcessSecurityAnalytics(IServiceFactory serviceFactory)
        {
            //_securityRepository = serviceFactory.GetSecuritiesRepository();
            _unitOfWorkFactory = serviceFactory.GetUnitOfWorkFactoryService();
        }
        public void RunTask()
        {
            _unitOfWork = _unitOfWorkFactory.GetUnitOfWork();
            var securities = _unitOfWork.securityRepository.GetSecurities(new ResourceParameters.SecuritiesResourceParameters());
            
            
            int securityCount = 0;

            // var currentSecurityCurrentPeakRanges = _unitOfWork.securityRepository.GetCurrentPeakRanges();

            //securities = securities.Except(currentSecurityCurrentPeakRanges).ToList();

            Parallel.ForEach(
                      securities,
  new ParallelOptions { MaxDegreeOfParallelism = 8 },
           security => { ProcessSecurityAnalytic(security); }

           );

          
           

            
           
            _unitOfWork.Dispose();
        }
        private void ProcessSecurityAnalytic(Entities.Security security)
        {

            List<SecurityAnalyticDto> securityAnalytics = new List<SecurityAnalyticDto>();

            IUnitOfWork unitOfWork = _unitOfWorkFactory.GetUnitOfWork();

            try
            {

                if (security.Id != 251)
                {
                    //  continue;
                }

                var historicalPrices = unitOfWork.securityRepository.GetHistoricalPricesCloseHistoricDate(security.Id, new ResourceParameters.HistoricalPricesResourceParameters());
                historicalPrices = historicalPrices.OrderBy(x => x.HistoricDate).ToList();

                if (historicalPrices.Count < 1)
                {
                    return ;
                }

                var latestHistoricDate = historicalPrices[historicalPrices.Count - 1].HistoricDate;//get the last historical price
                var historicGroupStats10day = GetMovingAveragePrices(historicalPrices, latestHistoricDate, -10);
                var historicGroupStats20day = GetMovingAveragePrices(historicalPrices, latestHistoricDate, -20);
                var historicGroupStats30day = GetMovingAveragePrices(historicalPrices, latestHistoricDate, -30);
                var historicGroupStats50day = GetMovingAveragePrices(historicalPrices, latestHistoricDate, -50);
                var historicGroupStats100day = GetMovingAveragePrices(historicalPrices, latestHistoricDate, -100);

                var historicGroupStats200day = GetMovingAveragePrices(historicalPrices, latestHistoricDate, -200);

                var historicGroupStats1year = GetMovingAveragePrices(historicalPrices, latestHistoricDate, -365);
                var historicGroupStats2year = GetMovingAveragePrices(historicalPrices, latestHistoricDate, -730);

                SecurityAnalyticDto securityAnalytic = new SecurityAnalyticDto()
                {
                    SecurityId = security.Id,
                    MovingAverageDay10 = Decimal.Round((historicGroupStats10day.Average.HasValue ? historicGroupStats10day.Average.Value : 0), 2),
                    MovingAverageDay20 = Decimal.Round((historicGroupStats20day.Average.HasValue ? historicGroupStats20day.Average.Value : 0), 2),
                    MovingAverageDay30 = Decimal.Round((historicGroupStats30day.Average.HasValue ? historicGroupStats30day.Average.Value : 0), 2),
                    MovingAverageDay50 = Decimal.Round((historicGroupStats50day.Average.HasValue ? historicGroupStats50day.Average.Value : 0), 2),
                    MovingAverageDay100 = Decimal.Round((historicGroupStats100day.Average.HasValue ? historicGroupStats100day.Average.Value : 0), 2),
                    MovingAverageDay200 = Decimal.Round((historicGroupStats200day.Average.HasValue ? historicGroupStats200day.Average.Value : 0), 2),
                    MovingAverageYear1 = Decimal.Round((historicGroupStats1year.Average.HasValue ? historicGroupStats1year.Average.Value : 0), 2),
                    MovingAverageYear2 = Decimal.Round((historicGroupStats2year.Average.HasValue ? historicGroupStats2year.Average.Value : 0), 2),

                    MaxPriceDay10 = Decimal.Round((historicGroupStats10day.Max.HasValue ? historicGroupStats10day.Max.Value : 0), 2),
                    MaxPriceDay20 = Decimal.Round((historicGroupStats20day.Max.HasValue ? historicGroupStats20day.Max.Value : 0), 2),
                    MaxPriceDay30 = Decimal.Round((historicGroupStats30day.Max.HasValue ? historicGroupStats30day.Max.Value : 0), 2),
                    MaxPriceDay50 = Decimal.Round((historicGroupStats50day.Max.HasValue ? historicGroupStats50day.Max.Value : 0), 2),
                    MaxPriceDay100 = Decimal.Round((historicGroupStats100day.Max.HasValue ? historicGroupStats100day.Max.Value : 0), 2),
                    MaxPriceDay200 = Decimal.Round((historicGroupStats200day.Max.HasValue ? historicGroupStats200day.Max.Value : 0), 2),
                    MaxPriceYear1 = Decimal.Round((historicGroupStats1year.Max.HasValue ? historicGroupStats1year.Max.Value : 0), 2),
                    MaxPriceYear2 = Decimal.Round((historicGroupStats2year.Max.HasValue ? historicGroupStats2year.Max.Value : 0), 2),

                    MinPriceDay10 = Decimal.Round((historicGroupStats10day.Min.HasValue ? historicGroupStats10day.Min.Value : 0), 2),
                    MinPriceDay20 = Decimal.Round((historicGroupStats20day.Min.HasValue ? historicGroupStats20day.Min.Value : 0), 2),
                    MinPriceDay30 = Decimal.Round((historicGroupStats30day.Min.HasValue ? historicGroupStats30day.Min.Value : 0), 2),
                    MinPriceDay50 = Decimal.Round((historicGroupStats50day.Min.HasValue ? historicGroupStats50day.Min.Value : 0), 2),
                    MinPriceDay100 = Decimal.Round((historicGroupStats100day.Min.HasValue ? historicGroupStats100day.Min.Value : 0), 2),
                    MinPriceDay200 = Decimal.Round((historicGroupStats200day.Min.HasValue ? historicGroupStats200day.Min.Value : 0), 2),
                    MinPriceYear1 = Decimal.Round((historicGroupStats1year.Min.HasValue ? historicGroupStats1year.Min.Value : 0), 2),
                    MinPriceYear2 = Decimal.Round((historicGroupStats2year.Min.HasValue ? historicGroupStats2year.Min.Value : 0), 2),
                    LastModified = DateTime.UtcNow,
                    LatestDateChecked = latestHistoricDate


                };
                securityAnalytics.Add(securityAnalytic);

                unitOfWork.securityRepository.UpsertSecurityAnalytics(securityAnalytics);
                
                    
                    securityAnalytics = new List<SecurityAnalyticDto>();
            }
            catch (Exception ex)
            {

            }
            unitOfWork.Dispose();

        }

        public GroupStats GetMovingAveragePrices(List<HistoricalPriceCloseHistoricDateDto> historicalPrices, DateTime latestHistoricDate, int daysDeducted)
        {
            return historicalPrices.Where(x => x.HistoricDate > latestHistoricDate.AddDays(daysDeducted))
                        .GroupBy(x => x.SecurityId)
                        .Select(x => new GroupStats
                        {
                            Average= x.Average(p => p.Close),
                            Max = x.Max(p => p.Close),
                            Min = x.Min(p => p.Close)
                        }
                    ).FirstOrDefault();

        }


    }
}
