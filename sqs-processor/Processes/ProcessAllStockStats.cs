using AutoMapper;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using sqs_processor.Entities;
using sqs_processor.Models;
using sqs_processor.ResourceParameters;
using sqs_processor.Services.Factories;
using sqs_processor.Services.Network.HistoricalPrices;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sqs_processor.Processes
{
    public class ProcessAllStockStats : IProcess
    {
        private readonly IMapper _mapper;
        private readonly IGetHistoricalPricesService _historicalPriceService;
        IUnitOfWork _unitOfWork;
        private readonly IUnitofWorkFactory _unitOfWorkFactory;
        public ProcessAllStockStats(IServiceFactory serviceFactory)
        {
            // _securityRepository = serviceFactory.GetSecuritiesRepository();
            _historicalPriceService = serviceFactory.GetHistoricalPricesService();
            _unitOfWorkFactory = serviceFactory.GetUnitOfWorkFactoryService();
            _mapper = serviceFactory.GetMapperService();
        }


        public void RunTask()
        {
            SecuritiesResourceParameters sr = new SecuritiesResourceParameters();
            //sr.symbol = "AAPL";
            _unitOfWork = _unitOfWorkFactory.GetUnitOfWork();
            var securities = _unitOfWork.securityRepository.GetSecurities(sr).OrderBy(x=>x.Id);


            Parallel.ForEach(
                        securities,
    new ParallelOptions { MaxDegreeOfParallelism = 3 },
             security => { ProcessAllStockStat(security); }

             );



          

        


                

            
           
        }



        private void ProcessAllStockStat(Security security)
        {
           
                try
                {
                    if (security.Id < 14007)
                    {
                       // return;
                    }

                    _historicalPriceService.startRange = DateTime.Now.AddDays(-8000);

                    _historicalPriceService.endRange = DateTime.Now;
                    SecurityIdSymbolDto securityRec = new SecurityIdSymbolDto();
                    securityRec.Id = security.Id;
                    securityRec.Symbol = security.Symbol;
                    string html = _historicalPriceService.GetStringHtml(securityRec);
                    List<HistoricalPriceforUpdateDto> historicalPrices = _historicalPriceService.TransformData(html, security.Id).OrderBy(x => x.HistoricDate).ToList();
                    if (historicalPrices.Count() > 0)
                    {
                        var currentClose = historicalPrices[0].Close;
                        for (var i = 1; i < historicalPrices.Count(); i++)
                        {
                            var close = historicalPrices[i].Close;

                            var percentage = ((close - currentClose) / currentClose) * 100;
                            historicalPrices[i].PercentChange = percentage;
                            currentClose = historicalPrices[i].Close;
                        }



                        try
                        {
                  
                            UpdateSecurityPercentageStatistics(security, historicalPrices);
                            UpdateSecurityAnalytic(security, historicalPrices);
                            UpdateProcessPeakRangeDetail(security, historicalPrices);
                            UpdateProcessPriorPurchaseEstimate(security, historicalPrices);
                            UpdateHistoricPerformance(security, historicalPrices);
                       
                            UpdateProcessBullRun(security, historicalPrices, (decimal).8);
                            UpdateProcessBullRun(security, historicalPrices, (decimal).9);



                    }
                        catch (Exception ex)
                        {

                        }





                    }



                }
                catch (Exception ex)
                {
                    Console.WriteLine("Messages error " + ex.Message);
                }

            
        }
        private BullBearRunDto NewBullRunRecord(int securityid, decimal bullEndedPercent)
        {
            return new BullBearRunDto()
                    {
                        DateCreated = DateTime.Now,
                        DateModified = DateTime.Now,
                        SecurityId = securityid,
                        PercentRangeCheck = 1 - bullEndedPercent,
                        HighPrice = 0,
                        LowPrice = 0,
                    };
        }


         private void UpdateProcessBullRun(Security security, List<HistoricalPriceforUpdateDto> fullhistoricalPrices, decimal bullEndedPercent)
        {
            DateTime sixYearsAgo = DateTime.Now;
            sixYearsAgo = sixYearsAgo.AddYears(-6);
            List<HistoricalPriceforUpdateDto> historicalPrices = fullhistoricalPrices.Where(x => x.HistoricDate > sixYearsAgo).ToList();
            List<BullBearRunDto> bullRunDetails = new List<BullBearRunDto>();
            List<CurrentBullBearRunDto> currentBullRunDetails = new List<CurrentBullBearRunDto>();
            try
            {

                bool firstRecordRead = false;

                if (historicalPrices.Count < 1)
                {
                    return;
                }
                bool bullRunHasEnded = false;

                bool bullRunFromLowHasEnded = false;

                BullBearRunDto bullRunDetail = NewBullRunRecord(security.Id, bullEndedPercent);

                BullBearRunDto bullRunFromLowDetail = NewBullRunRecord(security.Id, bullEndedPercent);

                foreach (var historicalPrice in historicalPrices)
                {
                    if (firstRecordRead)
                    {
                        
                            if (historicalPrice.Open < bullRunDetail.HighPrice * bullEndedPercent)
                            {

                            if (!bullRunHasEnded)
                            {
                                bullRunHasEnded = true;
                                bullRunDetail.EndRunPrice = historicalPrice.Open;
                                bullRunDetail.RunEndDate = historicalPrice.HistoricDate;
                                bullRunDetail.RunType = 1;
                                decimal? currentHigh = bullRunDetail.HighPrice;
                                DateTime currentHighDate = bullRunDetail.HighDate;
                                if (bullRunDetail.StartRunPrice.HasValue)
                                {
                                    bullRunDetails.Add(bullRunDetail);

                                }

                                bullRunDetail = NewBullRunRecord(security.Id, bullEndedPercent);

                                bullRunDetail.StartRunPrice = historicalPrice.Open;
                                bullRunDetail.RunStartDate = historicalPrice.HistoricDate;
                                bullRunDetail.LowPrice = historicalPrice.Open;
                                bullRunDetail.LowDate = historicalPrice.HistoricDate;
                                bullRunDetail.HighPrice = currentHigh;
                                bullRunDetail.HighDate = currentHighDate;
                            }
                                
                                
                            }


                        if (bullRunFromLowDetail.HighPrice.Value == (decimal?)198.02)
                        {
                            if(historicalPrice.Open < 158)
                            {
                                var highPriceCheck = bullRunFromLowDetail.HighPrice * bullEndedPercent;
                            }

                        }

                        if (historicalPrice.Open < bullRunFromLowDetail.HighPrice * bullEndedPercent)
                        {
                            if (!bullRunFromLowHasEnded)
                            {
                                bullRunFromLowHasEnded = true;
                                bullRunFromLowDetail.EndRunPrice = historicalPrice.Open;
                                bullRunFromLowDetail.RunEndDate = historicalPrice.HistoricDate;
                                bullRunFromLowDetail.RunType = 3;
                                decimal? currentHigh = bullRunDetail.HighPrice;
                                DateTime currentHighDate = bullRunDetail.HighDate;
                                if (bullRunFromLowDetail.StartRunPrice.HasValue) { 
                                bullRunDetails.Add(bullRunFromLowDetail);
                                }

                                bullRunFromLowDetail = NewBullRunRecord(security.Id, bullEndedPercent);

                                bullRunFromLowDetail.StartRunPrice = historicalPrice.Open;
                                bullRunFromLowDetail.RunStartDate = historicalPrice.HistoricDate;
                                bullRunFromLowDetail.LowPrice = historicalPrice.Open;
                                bullRunFromLowDetail.LowDate = historicalPrice.HistoricDate;
                                bullRunFromLowDetail.HighPrice = currentHigh;
                                bullRunFromLowDetail.HighDate = currentHighDate;

                            }
                        }




                            if (bullRunDetail.LowPrice > historicalPrice.Open)
                        {
                            bullRunDetail.LowPrice = historicalPrice.Open;
                            bullRunDetail.LowDate = historicalPrice.HistoricDate;
                        }

                        if (bullRunFromLowDetail.LowPrice > historicalPrice.Open)
                        {
                            bullRunFromLowDetail.LowPrice = historicalPrice.Open;
                            bullRunFromLowDetail.LowDate = historicalPrice.HistoricDate;
                        }

                        
                        if (historicalPrice.Open  > bullRunFromLowDetail.LowPrice  * (1 + ((1 - bullEndedPercent) * (decimal)1.5))) {
                            if (bullRunFromLowHasEnded)
                            {
                                bullRunFromLowHasEnded = false;

                                bullRunFromLowDetail.EndRunPrice = historicalPrice.Open;
                                bullRunFromLowDetail.RunEndDate = historicalPrice.HistoricDate;
                                bullRunFromLowDetail.RunType = 2;

                                decimal? currentLow = bullRunFromLowDetail.LowPrice;
                                DateTime currentLowDate = bullRunFromLowDetail.LowDate;

                                if (bullRunFromLowDetail.StartRunPrice.HasValue)
                                {
                                    bullRunDetails.Add(bullRunFromLowDetail);
                                }
                                

                                bullRunFromLowDetail = NewBullRunRecord(security.Id, bullEndedPercent);

                                bullRunFromLowDetail.StartRunPrice = historicalPrice.Open;
                                bullRunFromLowDetail.RunStartDate = historicalPrice.HistoricDate;
                                bullRunFromLowDetail.LowPrice = currentLow;
                                bullRunFromLowDetail.LowDate = currentLowDate;
                                bullRunFromLowDetail.HighPrice = historicalPrice.Open;
                                bullRunFromLowDetail.HighDate = historicalPrice.HistoricDate;
                            }



                        }
                        if (bullRunFromLowDetail.HighPrice < historicalPrice.Open)
                        {
                            bullRunFromLowDetail.HighPrice = historicalPrice.Open;
                            bullRunFromLowDetail.HighDate = historicalPrice.HistoricDate;
                        }
                        if (bullRunDetail.HighPrice < historicalPrice.Open)
                        {

                            if (bullRunHasEnded)
                            {
                                bullRunHasEnded = false;
                                bullRunDetail.EndRunPrice = historicalPrice.Open;
                                bullRunDetail.RunEndDate = historicalPrice.HistoricDate;
                                bullRunDetail.RunType = 0;

                                decimal? currentLow = bullRunDetail.LowPrice;
                                DateTime currentLowDate = bullRunDetail.LowDate;

                                if (bullRunDetail.StartRunPrice.HasValue)
                                {
                                    bullRunDetails.Add(bullRunDetail);

                                }
                                

                                bullRunDetail = NewBullRunRecord(security.Id, bullEndedPercent);

                                bullRunDetail.StartRunPrice = historicalPrice.Open;
                                bullRunDetail.RunStartDate = historicalPrice.HistoricDate;
                                bullRunDetail.LowPrice = currentLow;
                                bullRunDetail.LowDate = currentLowDate;

                             

                            }
                            bullRunDetail.HighPrice = historicalPrice.Open;
                            bullRunDetail.HighDate = historicalPrice.HistoricDate;
                        }



                    }
                    else
                    {

                        bullRunFromLowDetail.LowPrice = historicalPrice.Open;
                        bullRunFromLowDetail.LowDate = historicalPrice.HistoricDate;
                        bullRunFromLowDetail.HighDate = historicalPrice.HistoricDate;
                        bullRunFromLowDetail.RunStartDate = historicalPrice.HistoricDate;
                        bullRunFromLowDetail.RunEndDate = historicalPrice.HistoricDate;

                        bullRunFromLowDetail.LowPrice = historicalPrice.Open;
                        bullRunFromLowDetail.HighPrice = historicalPrice.Open;
                        
                        bullRunDetail.RunStartDate = historicalPrice.HistoricDate;
                        bullRunDetail.RunEndDate = historicalPrice.HistoricDate;
                        
                        bullRunDetail.LowPrice = historicalPrice.Open;
                        bullRunDetail.HighPrice = historicalPrice.Open;
                        bullRunDetail.HighDate = historicalPrice.HistoricDate;


                        firstRecordRead = true;

                    }
                }// end for for historical prices



                bullRunDetail.EndRunPrice = historicalPrices[historicalPrices.Count-1].Open;
                bullRunDetail.RunEndDate = historicalPrices[historicalPrices.Count - 1].HistoricDate;
                bullRunDetail.RunType = bullRunHasEnded ? 0 : 1;


                CurrentBullBearRunDto currentBullBearRun = _mapper.Map<CurrentBullBearRunDto>(bullRunDetail);
                currentBullRunDetails.Add(currentBullBearRun);

                bullRunFromLowDetail.EndRunPrice = historicalPrices[historicalPrices.Count - 1].Open;
                bullRunFromLowDetail.RunEndDate = historicalPrices[historicalPrices.Count - 1].HistoricDate;
                bullRunFromLowDetail.RunType = bullRunFromLowHasEnded ? 2 : 3;

                 currentBullBearRun = _mapper.Map<CurrentBullBearRunDto>(bullRunFromLowDetail);

                currentBullRunDetails.Add(currentBullBearRun);


                IUnitOfWork unitOfWork = _unitOfWorkFactory.GetUnitOfWork(); ;
                unitOfWork.securityRepository.UpsertBullRuns(bullRunDetails);
                unitOfWork.securityRepository.UpsertCurrentBullRuns(currentBullRunDetails);
                unitOfWork.Dispose();

            }
            catch (Exception ex)
            {

            }
        }
        //{"Unknown column 'b.StartRunPrice' in 'field list'"}


        private void UpdateHistoricPerformance(Security security, List<HistoricalPriceforUpdateDto> historicalPrices)
        {
            IUnitOfWork unitOfWork = _unitOfWorkFactory.GetUnitOfWork(); ;
            List<HistoricPerformanceDto> historicPerformances = new List<HistoricPerformanceDto>(); ;


            HistoricPerformanceDto historicPerformance = new HistoricPerformanceDto();
            DateTime dateTime = DateTime.Now;
            
           
            var weekAgoPrice = DateTime.Parse(dateTime.AddDays(-7).ToShortDateString());
            var monthAgoPrice = DateTime.Parse(dateTime.AddMonths(-1).ToShortDateString());
            var quaterAgoPrice = DateTime.Parse(dateTime.AddMonths(-3).ToShortDateString());
            var yearAgoPrice = DateTime.Parse(dateTime.AddYears(-1).ToShortDateString());

            var weekAgoPriceRec = historicalPrices.Where(x=> x.HistoricDate < weekAgoPrice).LastOrDefault();
            var monthAgoPriceRec = historicalPrices.Where(x => x.HistoricDate < monthAgoPrice).LastOrDefault();
            var quaterAgoPriceRec = historicalPrices.Where(x => x.HistoricDate < quaterAgoPrice).LastOrDefault();
            var yearAgoPriceRec = historicalPrices.Where(x => x.HistoricDate < yearAgoPrice).LastOrDefault();


            if(weekAgoPriceRec  != null && monthAgoPriceRec != null && quaterAgoPriceRec != null && yearAgoPriceRec != null)
            {
                if(weekAgoPriceRec.HistoricDate == monthAgoPriceRec.HistoricDate 
                    || monthAgoPriceRec.HistoricDate == quaterAgoPriceRec.HistoricDate
                    || quaterAgoPriceRec.HistoricDate == yearAgoPriceRec.HistoricDate
                    )
                {
                    return;
                }
            }else { return; }

            if (weekAgoPriceRec != null)
            {
                historicPerformance.WeekAgoPrice = weekAgoPriceRec.Close;
            }

            if (monthAgoPriceRec != null)
            {
                historicPerformance.MonthAgoPrice = monthAgoPriceRec.Close;
            }

            if (quaterAgoPriceRec != null)
            {
                historicPerformance.QuaterAgoPrice = quaterAgoPriceRec.Close;
            }

            if (yearAgoPriceRec != null)
            {
                historicPerformance.YearAgoPrice = yearAgoPriceRec.Close;
            }
            historicPerformance.SecurityId = security.Id;
            historicPerformance.DateCalculated = DateTime.Now;
            historicPerformances.Add(historicPerformance);
            unitOfWork.securityRepository.UpsertHistoricPerformances(historicPerformances);
            historicPerformances = new List<HistoricPerformanceDto>();
            unitOfWork.Dispose();
        }
        
        private void UpdateProcessPriorPurchaseEstimate(Security security, List<HistoricalPriceforUpdateDto> historicalPrices)
        {
            IUnitOfWork unitOfWork = _unitOfWorkFactory.GetUnitOfWork(); ;



            List<PriorPurchaseEstimateDto> priorPurchaseEstimates = new List<PriorPurchaseEstimateDto>();
            DateTime firstPurchaseDate = DateTime.Now;
            bool firstPurchase = false;
            firstPurchaseDate = DateTime.Now;
            firstPurchase = false;
            if (security.CurrentPrice < 5)
            {
                return;
            }
            if (security.Id != 251)
            {
                //continue;
            }
            historicalPrices = historicalPrices.OrderBy(x => x.HistoricDate).ToList();
            decimal totalPrice = 0;
            decimal totalShares = 0;
            int monthSet = 0;
            int purchaseFrequency = 0;//monthly
            foreach (var historicalPrice in historicalPrices)
            {
                if (historicalPrice.HistoricDate.Month != monthSet)
                {
                    monthSet = historicalPrice.HistoricDate.Month;
                    if (!firstPurchase)
                    {
                        firstPurchase = true;
                        firstPurchaseDate = historicalPrice.HistoricDate;
                    }

                    totalPrice += historicalPrice.Open.Value;
                    totalShares += 1;
                }

            }
            PriorPurchaseEstimateDto securityPurchase = new PriorPurchaseEstimateDto();
            securityPurchase.SecurityId = security.Id;
            securityPurchase.DateCreated = DateTime.Now;
            securityPurchase.DateModified = DateTime.Now;
            securityPurchase.PurchasePrice = totalPrice;
            securityPurchase.Shares = totalShares;
            securityPurchase.FirstPurchaseDate = firstPurchaseDate;
            securityPurchase.PurchaseFrequency = purchaseFrequency;

            priorPurchaseEstimates.Add(securityPurchase);

            unitOfWork.securityRepository.UpsertPriorPurchaseEstimates(priorPurchaseEstimates);
            priorPurchaseEstimates = new List<PriorPurchaseEstimateDto>();
            unitOfWork.Dispose();


        }




        private void UpdateProcessPeakRangeDetail(Security security, List<HistoricalPriceforUpdateDto> historicalPrices)
        {

            IUnitOfWork unitOfWork = _unitOfWorkFactory.GetUnitOfWork(); ;
            List<PeakRangeDetailDto> peakRangeDetails = new List<PeakRangeDetailDto>();
            List<CurrentPeakRangeDto> currentPeakRanges = new List<CurrentPeakRangeDto>();
            Dictionary<string, PeakRangeDetailDto> localPeakRanges = new Dictionary<string, PeakRangeDetailDto>();// new PeakRangeDetailDto[35];
            try
            {
                localPeakRanges = new Dictionary<string, PeakRangeDetailDto>();// new PeakRangeDetailDto[25];
                
                DateTime currentDateLow = new DateTime();
                DateTime newRange;
                int daysRange;
                DateTime rangeStart = new DateTime();
                decimal? highRange = 0;
                decimal? lowRange = 0;
                if (historicalPrices.Count < 1)
                {
                    return;
                }
                foreach (var historicalPrice in historicalPrices)
                {

                    if (historicalPrice.Open > highRange)
                    {

                        if (highRange != 0 && highRange != lowRange)
                        {
                            decimal? percentLevel = (highRange - lowRange) / highRange;
                            if (percentLevel > (decimal).009)
                            {

                                newRange = historicalPrice.HistoricDate;
                                daysRange = (int)(newRange - rangeStart).TotalDays;
                                decimal? divide5 = 5;
                                int percentRanking = (int)Math.Floor((decimal)(percentLevel * 100) / 5);
                                string rangeName = (percentRanking * 5).ToString() + "% - " + (Math.Round((percentRanking * 5) + 4.99, 2)).ToString() + "%";
                                if (localPeakRanges.ContainsKey(rangeName))
                                {
                                    PeakRangeDetailDto peakrangedetails = localPeakRanges[rangeName];
                                    peakrangedetails.RangeLength += daysRange;
                                    if (peakrangedetails.MaxRangeLength < daysRange)
                                    {
                                        peakrangedetails.MaxRangeLength = daysRange;
                                        peakrangedetails.MaxRangeDateStart = rangeStart;
                                        peakrangedetails.MaxRangeDateEnd = rangeStart.AddDays(daysRange);
                                    }
                                    peakrangedetails.RangeCount += 1;
                                    localPeakRanges[rangeName] = peakrangedetails;
                                }
                                else
                                {
                                    PeakRangeDetailDto peakrangedetails = new PeakRangeDetailDto();
                                    peakrangedetails.RangeName = rangeName;
                                    peakrangedetails.RangeLength = daysRange;
                                    peakrangedetails.MaxRangeLength = daysRange;
                                    peakrangedetails.RangeCount = 1;
                                    peakrangedetails.SecurityId = security.Id;
                                    peakrangedetails.MaxRangeDateStart = rangeStart;
                                    peakrangedetails.MaxRangeDateEnd = rangeStart.AddDays(daysRange);
                                    peakrangedetails.DateCreated = DateTime.Now;
                                    peakrangedetails.DateModified = DateTime.Now;
                                    localPeakRanges[rangeName] = peakrangedetails;
                                }

                                // this.peakRanges.push(percentLevel);
                            }

                        }
                        rangeStart = historicalPrice.HistoricDate;
                        highRange = historicalPrice.Open;
                        lowRange = historicalPrice.Open;
                    }


                    if (historicalPrice.Open < lowRange)
                    {
                        lowRange = historicalPrice.Open;
                        currentDateLow = historicalPrice.HistoricDate;
                    }

                }// end for for historical prices


                CurrentPeakRangeDto currentPeakRange = new CurrentPeakRangeDto();
                newRange = historicalPrices[historicalPrices.Count - 1].HistoricDate;
                // 1410
                daysRange = (int)(newRange - rangeStart).TotalDays;
                decimal percentLevelSet = (decimal)((highRange - lowRange) / highRange);
                int percentRankingSet = (int)Math.Floor((percentLevelSet * 100) / 5);
                currentPeakRange.SecurityId = security.Id;
                currentPeakRange.RangeName = (percentRankingSet * 5).ToString() + "% - " + (Math.Round((percentRankingSet * 5) + 4.99, 2)).ToString() + "%";
                currentPeakRange.RangeLength = daysRange;
                currentPeakRange.RangeDateStart = rangeStart;
                currentPeakRange.PeakRangeCurrentPercentage = (decimal)((highRange - security.CurrentPrice) / highRange) * 100;
                currentPeakRange.DateCreated = DateTime.Now;
                currentPeakRange.DateModified = DateTime.Now;
                currentPeakRange.LastOpenHigh = highRange;
                currentPeakRanges.Add(currentPeakRange);
                foreach (var localPeakRange in localPeakRanges)
                {
                    peakRangeDetails.Add(localPeakRange.Value);
                }

                unitOfWork.securityRepository.UpsertPeakRangeDetails(peakRangeDetails);
                unitOfWork.securityRepository.UpsertCurrentPeakRanges(currentPeakRanges);
                unitOfWork.Dispose();

                currentPeakRanges = new List<CurrentPeakRangeDto>();
                peakRangeDetails = new List<PeakRangeDetailDto>();

            }
            catch (Exception ex)
            {

            }
        }



        public void UpdateSecurityAnalytic(Security security, List<HistoricalPriceforUpdateDto> fullHistoricalPrices)
        {
            List<SecurityAnalyticDto> securityAnalytics = new List<SecurityAnalyticDto>();

            IUnitOfWork unitOfWork = _unitOfWorkFactory.GetUnitOfWork();

            try
            {

                if (security.Id != 251)
                {
                    //  continue;
                }

                var historicParams = new ResourceParameters.HistoricalPricesResourceParameters();
                historicParams.HistoricDateHigh = DateTime.Now.AddDays(2);
                historicParams.HistoricDateLow = DateTime.Now.AddDays(-830);

                var historicalPrices = fullHistoricalPrices.Where(x => x.HistoricDate > historicParams.HistoricDateLow).ToList();


                if (historicalPrices.Count < 1)
                {
                    return;
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











        public GroupStats GetMovingAveragePrices(List<HistoricalPriceforUpdateDto> historicalPrices, DateTime latestHistoricDate, int daysDeducted)
        {
            return historicalPrices.Where(x => x.HistoricDate > latestHistoricDate.AddDays(daysDeducted))
                        .GroupBy(x => x.SecurityId)
                        .Select(x => new GroupStats
                        {
                            Average = x.Average(p => p.Close),
                            Max = x.Max(p => p.Close),
                            Min = x.Min(p => p.Close)
                        }
                    ).FirstOrDefault();

        }










































        private void UpdateSecurityPercentageStatistics(Security security,List<HistoricalPriceforUpdateDto> fullHistoricalPrices)
        {
            IUnitOfWork unitOfWork = _unitOfWorkFactory.GetUnitOfWork(); ;
            List<SecurityPercentageStatisticDto> securityPercentageStatistics = new List<SecurityPercentageStatisticDto>();

            if (security.Id != 828)
            {
                //683,2565,3162, 14974
                //continue;
            }
            try
            {
                HistoricalPricesResourceParameters hisParams = new HistoricalPricesResourceParameters();
                hisParams.HistoricDateHigh = DateTime.Now.AddDays(2);
                hisParams.HistoricDateLow = DateTime.Now.AddDays(-730);
                hisParams.openLow = 0;

                var historicalPrices = fullHistoricalPrices.Where(x => x.HistoricDate > hisParams.HistoricDateLow).ToList();



                if (historicalPrices.Count == 0)
                {
                    securityPercentageStatistics.Add(AddEmptyHistoricalPrice(security.Id));

                }
                historicalPrices = historicalPrices.OrderBy(x => x.HistoricDate).ToList();
                decimal averagePercentDrop = GetAverageDrop(historicalPrices);
                if (averagePercentDrop == 0 && securityPercentageStatistics.Count == 0)
                {
                    securityPercentageStatistics.Add(AddEmptyHistoricalPrice(security.Id));

                }
                int dropCount = GetDropCount(historicalPrices, averagePercentDrop);
                if (dropCount == 0 && securityPercentageStatistics.Count == 0)
                {
                    securityPercentageStatistics.Add(AddEmptyHistoricalPrice(security.Id));

                }
                //decimal averageDrop = GetAverageCount(historicalPrices, averagePercentDrop);





                if (securityPercentageStatistics.Count == 0)
                {
                    decimal totalPercentSum = GetTotalPercentSum(historicalPrices);
                    decimal highLowRangeAverage = GetLowHighRangeAverage(historicalPrices);
                    List<SecurityPercentages> securityPercentrages = GetPercentages(historicalPrices, averagePercentDrop, dropCount);
                    if (securityPercentrages.Count == 0 && securityPercentageStatistics.Count == 0)
                    {
                        securityPercentageStatistics.Add(AddEmptyHistoricalPrice(security.Id));
                    }




                    if (securityPercentageStatistics.Count == 0)
                    {
                        decimal lowPercentageAverage = GetLowPercentageAverage(historicalPrices, averagePercentDrop);
                        decimal averageDrophighLowRangePercentageAverage = GetAverageDropHighLowRangePercentageAverage(historicalPrices, averagePercentDrop);

                        int percent5 = securityPercentrages.Min(x => x.Percent5);
                        int percent10 = securityPercentrages.Min(x => x.Percent10);
                        int percent15 = securityPercentrages.Min(x => x.Percent15);

                        decimal percentDetails5 = securityPercentrages.Where(x => x.Percent5 == percent5).Select(x => x.LowPercent).FirstOrDefault();
                        decimal percentDetails10 = securityPercentrages.Where(x => x.Percent10 == percent10).Select(x => x.LowPercent).FirstOrDefault();
                        decimal percentDetails15 = securityPercentrages.Where(x => x.Percent15 == percent15).Select(x => x.LowPercent).FirstOrDefault();
                        SecurityPercentageStatisticDto securityPercentageStatistic = new SecurityPercentageStatisticDto();

                        securityPercentageStatistic.SecurityId = security.Id;
                        securityPercentageStatistic.AverageDrop = Math.Round(averagePercentDrop, 2);
                        securityPercentageStatistic.Percent5 = Math.Round(percentDetails5, 2);
                        securityPercentageStatistic.Percent10 = Math.Round(percentDetails10, 2);
                        securityPercentageStatistic.Percent15 = Math.Round(percentDetails15, 2);
                        securityPercentageStatistic.DateCreated = DateTime.Now;
                        securityPercentageStatistic.DateModified = DateTime.Now;
                        securityPercentageStatistic.totalPercentSum = Math.Round(totalPercentSum, 2);
                        securityPercentageStatistic.highLowRangeAverage = Math.Round(highLowRangeAverage, 2);
                        securityPercentageStatistic.belowAverageCount = dropCount;
                        securityPercentageStatistic.AvgDropLowAvg = lowPercentageAverage;
                        securityPercentageStatistic.AvgDropHighLowRangeAvg = averageDrophighLowRangePercentageAverage;
                        securityPercentageStatistics.Add(securityPercentageStatistic);
                    }
                }
                unitOfWork.securityRepository.UpsertSecurityPercentageStatistics(securityPercentageStatistics);

                unitOfWork.Dispose();

            }
            catch (Exception ex)
            {

            }

        }


        private SecurityPercentageStatisticDto AddEmptyHistoricalPrice(int securityId)
        {
            SecurityPercentageStatisticDto securityPercentageStatisticNoPrices = new SecurityPercentageStatisticDto();

            securityPercentageStatisticNoPrices.SecurityId = securityId;
            securityPercentageStatisticNoPrices.AverageDrop = 0;
            securityPercentageStatisticNoPrices.Percent5 = 0;
            securityPercentageStatisticNoPrices.Percent10 = 0;
            securityPercentageStatisticNoPrices.Percent15 = 0;
            securityPercentageStatisticNoPrices.totalPercentSum = 0;
            securityPercentageStatisticNoPrices.highLowRangeAverage = 0;
            securityPercentageStatisticNoPrices.belowAverageCount = 0;
            securityPercentageStatisticNoPrices.DateCreated = DateTime.Now;
            securityPercentageStatisticNoPrices.DateModified = DateTime.Now;
            return securityPercentageStatisticNoPrices;

        }
        private decimal GetAverageDrop(List<HistoricalPriceforUpdateDto> historicalPrices)
        {
            var localHistoricalPrices = historicalPrices
                .Where(x => x.PercentChange.Value < 0);

            if (localHistoricalPrices.Count() == 0)
            {
                return 0;
            }

            return localHistoricalPrices.GroupBy(x => new { ID = x.SecurityId })
                .Select(g => new { Average = g.Average(p => p.PercentChange) })
                .Select(x => x).FirstOrDefault().Average.Value;



        }
        private decimal GetLowHighRangeAverage(List<HistoricalPriceforUpdateDto> historicalPrices)
		{
			var historicalPriceList = historicalPrices.Where(x => x.Close != 0 && x.Open != 0 && x.Low != 0 && x.High !=0).ToList();
			return historicalPriceList
				.Where(x => x.Low > 0)
				.GroupBy(x => new { ID = x.SecurityId })
				.Select(g => new { Average = g.Average(p => ((p.High - p.Low)/p.Low)*100) })
				.Select(x => x).FirstOrDefault().Average.Value;
		}
        private List<SecurityPercentages> GetPercentages(List<HistoricalPriceforUpdateDto> historicalPrices, decimal averagePercentDrop, int dropCount)
        {
            var historicalPriceList = historicalPrices.Where(x => x.Close != 0 && x.Open != 0 && x.Low != 0 && x.High != 0).ToList();
            var securityPerDetails = historicalPriceList
                            .Where(x =>
                            //Math.Abs(x.PercentChange.Value - Math.Round(((x.Low.Value - x.Open.Value) / x.Open.Value) * 100, 1)) < (decimal?).5
                            x.PercentChange < averagePercentDrop && x.Low.Value < x.Close * (decimal).996
                            )
                            .GroupBy(x => new {
                                ID =
                            //Math.Round(((x.Low.Value - x.Open.Value) / x.Open.Value) * 100, 1)
                            x.PercentChange.Value
                            })
                            .Select(g => new { LowPercent = g.Key, Count = g.Count() }).ToList();
            ;
            securityPerDetails = securityPerDetails.OrderBy(x => x.LowPercent.ID).ToList();

            List<SecurityPercentageDetail> securityPercentageDetails = new List<SecurityPercentageDetail>();
            int totalCount = 0;
            foreach (var securityPerDetail in securityPerDetails)
            {


                SecurityPercentageDetail securityPercentageDetail = new SecurityPercentageDetail();
                securityPercentageDetail.LowPercent = securityPerDetail.LowPercent.ID;
                securityPercentageDetail.Count = securityPerDetail.Count;
                totalCount += securityPerDetail.Count;
                securityPercentageDetail.PreviousRunningTotal = totalCount;

                securityPercentageDetails.Add(securityPercentageDetail);



            }

            List<SecurityPercentages> securityPercentrages = securityPercentageDetails.Select(x => new SecurityPercentages
            {
                LowPercent = x.LowPercent,
                Percent5 = x.PreviousRunningTotal < dropCount * .05 ? 2000 : x.PreviousRunningTotal,
                Percent10 = x.PreviousRunningTotal < dropCount * .10 ? 2000 : x.PreviousRunningTotal,

                Percent15 = x.PreviousRunningTotal < dropCount * .15 ? 2000 : x.PreviousRunningTotal
            }).ToList();

            return securityPercentrages;
        }
        private int GetDropCount(List<HistoricalPriceforUpdateDto> historicalPrices, decimal averagePercentDrop)
        {
            //ABS(percentChange - Round((Low - Open) / open * 100, 1)) < .5;
            return historicalPrices
                .Where(x =>
                //Math.Abs(x.PercentChange.Value - 
                //Math.Round(((x.Low.Value - x.Open.Value) / x.Open.Value) * 100, 1)) < (decimal?).5
                x.PercentChange < averagePercentDrop
                //&& x.Low.Value < x.Close * (decimal).996
                )
                .Count();
        }

        private decimal GetLowPercentageAverage(List<HistoricalPriceforUpdateDto> historicalPrices, decimal averagePercentDrop)
        {
            int securityId = historicalPrices[0].SecurityId;
            var historicalPriceList = historicalPrices.Where(x => x.Close != 0 && x.Open != 0 && x.Low != 0 && x.High != 0).ToList();
            var info = historicalPriceList
                //.Where(x => x.PercentChange < averagePercentDrop * (decimal)1.5)
                .Where(x => ((x.Low - x.Open) / x.Open) * 100 < averagePercentDrop * (decimal)1.5)
                .GroupBy(x => new { ID = decimal.Round((decimal)(((x.Low - x.Close) / x.Close) * 100), 1) })
                .Select(g => new { Count = g.Count(), Value = g.Key, Id = securityId }).ToList().OrderBy(x => x.Value.ID).ToList();

            var detail = info.Join(historicalPriceList.Where(x => x.PercentChange < averagePercentDrop * (decimal)1.5), x => x.Id, y => y.SecurityId, (invProj, invProjSec) => new { invProj, invProjSec })
                .Where(x => x.invProj.Value.ID == decimal.Round((decimal)(((x.invProjSec.Low - x.invProjSec.Close) / x.invProjSec.Close) * 100), 1)).Select
                (x => new {
                    SecurityId = x.invProj.Id,
                    LowPercentage = decimal.Round((decimal)((x.invProjSec.Low - x.invProjSec.Open) / x.invProjSec.Open) * 100, 1),
                    CountDetails = x.invProj.Count
                }).ToList();

            var lowAveragePercentDrop = detail.GroupBy(x => x.SecurityId).Select(x => x.Sum(g => g.LowPercentage * g.CountDetails) / x.Sum(g => g.CountDetails)).FirstOrDefault();
            return lowAveragePercentDrop;


        }

        private decimal GetTotalPercentSum(List<HistoricalPriceforUpdateDto> historicalPrices)
        {
            return historicalPrices
                .GroupBy(x => new { ID = x.SecurityId })
                .Select(g => new { Sum = g.Sum(p => p.PercentChange) })
                .Select(x => x).FirstOrDefault().Sum.Value;
        }
        private decimal GetAverageDropHighLowRangePercentageAverage(List<HistoricalPriceforUpdateDto> historicalPrices, decimal averagePercentDrop)
        {
            int securityId = historicalPrices[0].SecurityId;
            var historicalPriceList = historicalPrices.Where(x => x.Close != 0 && x.Open != 0 && x.Low != 0 && x.High != 0).ToList();
            var info = historicalPriceList
                //((x.Low - x.Close) / x.Close) * 100
                //.Where(x => x.PercentChange < averagePercentDrop * (decimal)1.5)
                .Where(x => ((x.Low - x.Open) / x.Open) * 100 < averagePercentDrop * (decimal)1.5)
                .GroupBy(x => new { ID = decimal.Round((decimal)(((x.Low - x.Close) / x.Close) * 100), 1) })
                .Select(g => new { Count = g.Count(), Value = g.Key, Id = securityId }).ToList().OrderBy(x => x.Value.ID).ToList();


            var highLowRange = info.Join(historicalPriceList.Where(x => x.PercentChange < averagePercentDrop * (decimal)1.5), x => x.Id, y => y.SecurityId, (invProj, invProjSec) => new { invProj, invProjSec })
            .Where(x => x.invProj.Value.ID == decimal.Round((decimal)(((x.invProjSec.Low - x.invProjSec.Close) / x.invProjSec.Close) * 100), 1)).Select
            (x => new {
                SecurityId = x.invProj.Id,
                HighLowRangePercentage = decimal.Round((decimal)((x.invProjSec.High - x.invProjSec.Low) / x.invProjSec.Low) * 100, 1),
                CountDetails = x.invProj.Count
            }).ToList();

            var highLowRangePercentage = highLowRange.GroupBy(x => x.SecurityId).Select(x => x.Sum(g => g.HighLowRangePercentage * g.CountDetails) / x.Sum(g => g.CountDetails)
            //x.Average(g => g.HighLowRangePercentage)

            )
                .FirstOrDefault();
            return highLowRangePercentage;
        }


























    }
}
