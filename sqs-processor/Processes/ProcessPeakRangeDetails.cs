using sqs_processor.Models;
using sqs_processor.Services.Factories;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sqs_processor.Processes
{
   public class ProcessPeakRangeDetails: IProcess
    {
        private readonly ISecuritiesRepository _securityRepository;
        public ProcessPeakRangeDetails(IServiceFactory serviceFactory)
        {
            _securityRepository = serviceFactory.GetSecuritiesRepository();
        }
        public void RunTask()
        {

            var securities = _securityRepository.GetSecurities(new ResourceParameters.SecuritiesResourceParameters());
            List<PeakRangeDetailDto> peakRangeDetails = new List<PeakRangeDetailDto>();
            List<CurrentPeakRangeDto> currentPeakRanges = new List<CurrentPeakRangeDto>();
            Dictionary<string, PeakRangeDetailDto> localPeakRanges = new Dictionary<string, PeakRangeDetailDto>();// new PeakRangeDetailDto[35];
            foreach (var security in securities)
            {
            
                try
                {
                    localPeakRanges = new Dictionary<string, PeakRangeDetailDto>();// new PeakRangeDetailDto[25];
                    var historicalPrices = _securityRepository.GetHistoricalPrices(security.Id, new ResourceParameters.HistoricalPricesResourceParameters());
                    historicalPrices = historicalPrices.OrderBy(x => x.HistoricDate).ToList();
                    DateTime currentDateLow = new DateTime();
                    DateTime newRange;
                    int daysRange;
                    DateTime rangeStart = new DateTime();
                    decimal? highRange = 0;
                    decimal? lowRange = 0;
                    if(historicalPrices.Count < 1)
                    {
                        continue;
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
                                            peakrangedetails.MaxRangeDateStart = currentDateLow;
                                            peakrangedetails.MaxRangeDateEnd = currentDateLow.AddDays(daysRange);
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
                                        peakrangedetails.MaxRangeDateStart = currentDateLow;
                                        peakrangedetails.MaxRangeDateEnd = currentDateLow.AddDays(daysRange);
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
                    currentPeakRange.RangeDateStart = currentDateLow;
                    currentPeakRange.PeakRangeCurrentPercentage = (decimal)((highRange - security.CurrentPrice) / highRange) * 100;
                    currentPeakRange.DateCreated = DateTime.Now;
                    currentPeakRange.DateModified = DateTime.Now;

                    currentPeakRanges.Add(currentPeakRange);
                    foreach( var localPeakRange in localPeakRanges)
                    {
                       peakRangeDetails.Add(localPeakRange.Value);
                    }

                    if (peakRangeDetails.Count > 500)
                    {
                        _securityRepository.UpsertPeakRangeDetails(peakRangeDetails);
                        _securityRepository.UpsertCurrentPeakRanges(currentPeakRanges);

                        currentPeakRanges = new List<CurrentPeakRangeDto>();
                        peakRangeDetails = new List<PeakRangeDetailDto>();
                    }
                }
                catch(Exception ex)
                {

                }
           

            }
            if (peakRangeDetails.Count > 0)
            {
                _securityRepository.UpsertPeakRangeDetails(peakRangeDetails);
                _securityRepository.UpsertCurrentPeakRanges(currentPeakRanges);

                //currentPeakRanges = new List<CurrentPeakRangeDto>();
                //peakRangeDetails = new List<PeakRangeDetailDto>();
            }
        }


    }
}
