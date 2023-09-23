using sqs_processor.Entities;
using sqs_processor.Models;
using sqs_processor.ResourceParameters;
using sqs_processor.Services.Factories;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sqs_processor.Processes
{
	public class SecurityPercentageDetail
    {
		public decimal LowPercent { get; set; }
		public int Count { get; set; }
		public int PreviousRunningTotal { get; set; }

	}

	public class SecurityPercentages
	{
		public decimal LowPercent { get; set; }
		public int Percent5 { get; set; }
		public int Percent10 { get; set; }
		public int Percent15 { get; set; }

		/*
				lowPercent,
		CASE WHEN  PreviousRunningTotal < dropCount * .05 THEN 1000 ELSE PreviousRunningTotal END,
		CASE WHEN  PreviousRunningTotal < dropCount * .10 THEN 1000 ELSE PreviousRunningTotal END, 
		CASE WHEN  PreviousRunningTotal < dropCount * .15 THEN 1000 ELSE PreviousRunningTotal END
				*/

	}
	public class ProcessSecurityPercentageStatistics : IProcess
    {

        // private readonly ISecuritiesRepository _securityRepository;
        private IUnitOfWork _unitOfWork;
        private readonly IUnitofWorkFactory _unitOfWorkFactory;
        public ProcessSecurityPercentageStatistics(IServiceFactory serviceFactory)
        {
            // _securityRepository = serviceFactory.GetSecuritiesRepository();
            _unitOfWorkFactory = serviceFactory.GetUnitOfWorkFactoryService();
        }
		

		

	
		public void RunTask()
        {
			_unitOfWork = _unitOfWorkFactory.GetUnitOfWork();
			var secResourceParam = new SecuritiesResourceParameters();
			secResourceParam.lastModifiedRangeStart = DateTime.Now.AddDays(-2);

			//var info = _unitOfWork.securityRepository.GetPotentialBuys();


			var securities = _unitOfWork.securityRepository.GetSecurities(secResourceParam);
			var currentSecurityPercentage = _unitOfWork.securityRepository.GetCurrentSecurityPercentage();
			securities = securities.Except(currentSecurityPercentage).ToList();




			Parallel.ForEach(
					  securities,
  new ParallelOptions { MaxDegreeOfParallelism = 8 },
		   security => { ProcessSecurityPercentageStatistic(security); }

		   );



				_unitOfWork.Dispose();
		}

		private void ProcessSecurityPercentageStatistic(Entities.Security security)
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
					var historicalPrices = unitOfWork.securityRepository.GetHistoricalPrices(security.Id, hisParams);
					if (historicalPrices.Count == 0)
					{
						securityPercentageStatistics.Add(AddEmptyHistoricalPrice(security.Id));
						
					}
					historicalPrices = historicalPrices.OrderBy(x => x.HistoricDate).ToList();
					decimal averagePercentDrop = GetAverageDrop(historicalPrices);
					if (averagePercentDrop == 0 && securityPercentageStatistics.Count == 0 )
					{
						securityPercentageStatistics.Add(AddEmptyHistoricalPrice(security.Id));
					
					}
					int dropCount = GetDropCount(historicalPrices, averagePercentDrop);
					if (dropCount == 0 && securityPercentageStatistics.Count == 0)
					{
						securityPercentageStatistics.Add(AddEmptyHistoricalPrice(security.Id));
					
					}
					//decimal averageDrop = GetAverageCount(historicalPrices, averagePercentDrop);


					decimal totalPercentSum = GetTotalPercentSum(historicalPrices);

					decimal highLowRangeAverage = GetLowHighRangeAverage(historicalPrices);





					List<SecurityPercentages> securityPercentrages = GetPercentages(historicalPrices, averagePercentDrop, dropCount);


					if (securityPercentrages.Count == 0 && securityPercentageStatistics.Count == 0)
					{
						securityPercentageStatistics.Add(AddEmptyHistoricalPrice(security.Id));
					
					}

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


				if (securityPercentageStatistics.Count == 0)
				{
					securityPercentageStatistics.Add(securityPercentageStatistic);
				}
						unitOfWork.securityRepository.UpsertSecurityPercentageStatistics(securityPercentageStatistics);

						unitOfWork.Dispose();

				}
				catch (Exception ex)
				{

				}
			

		}
			private decimal GetAverageDropHighLowRangePercentageAverage(List<HistoricalPrice> historicalPrices, decimal averagePercentDrop)
		{
			int securityId = historicalPrices[0].SecurityId;
			var historicalPriceList = historicalPrices.Where(x => x.Close != 0 && x.Open != 0 && x.Low != 0 && x.High !=0).ToList();
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
			private decimal GetLowPercentageAverage(List<HistoricalPrice> historicalPrices, decimal averagePercentDrop)
		{
			int securityId = historicalPrices[0].SecurityId;
			var historicalPriceList = historicalPrices.Where(x => x.Close != 0 && x.Open != 0 && x.Low != 0 && x.High !=0).ToList();
			var info = historicalPriceList
				//.Where(x => x.PercentChange < averagePercentDrop * (decimal)1.5)
				.Where(x => ((x.Low - x.Open) / x.Open) * 100 < averagePercentDrop * (decimal)1.5)
				.GroupBy(x => new { ID = decimal.Round((decimal)(((x.Low - x.Close) / x.Close) * 100), 1) })
				.Select(g => new { Count = g.Count(), Value = g.Key, Id = securityId }).ToList().OrderBy(x=>x.Value.ID).ToList();

			var detail = info.Join(historicalPriceList.Where(x => x.PercentChange < averagePercentDrop * (decimal)1.5), x => x.Id, y => y.SecurityId, (invProj, invProjSec) => new { invProj, invProjSec })
				.Where(x => x.invProj.Value.ID == decimal.Round((decimal)(((x.invProjSec.Low - x.invProjSec.Close) / x.invProjSec.Close) * 100), 1)).Select
				(x=> new {
					SecurityId = x.invProj.Id,
					LowPercentage = decimal.Round((decimal)((x.invProjSec.Low - x.invProjSec.Open) / x.invProjSec.Open) * 100,1),
					CountDetails = x.invProj.Count
				}).ToList();

			var lowAveragePercentDrop = detail.GroupBy(x => x.SecurityId).Select(x => x.Sum(g => g.LowPercentage * g.CountDetails) / x.Sum(g=>g.CountDetails)).FirstOrDefault();
			return lowAveragePercentDrop;
			

		}


		private List<SecurityPercentages> GetPercentages(List<HistoricalPrice> historicalPrices, decimal averagePercentDrop, int dropCount)
        {
			var historicalPriceList = historicalPrices.Where(x => x.Close != 0 && x.Open != 0 && x.Low != 0 && x.High !=0).ToList();
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
		private int GetDropCount(List<HistoricalPrice> historicalPrices, decimal averagePercentDrop)
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

		private decimal GetTotalPercentSum(List<HistoricalPrice> historicalPrices)
		{
			return historicalPrices
				.GroupBy(x => new { ID = x.SecurityId })
				.Select(g => new { Sum = g.Sum(p => p.PercentChange) })
				.Select(x => x).FirstOrDefault().Sum.Value;
		}
		private decimal GetAverageDrop(List<HistoricalPrice> historicalPrices)
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

		
		private decimal GetLowHighRangeAverage(List<HistoricalPrice> historicalPrices)
		{
			var historicalPriceList = historicalPrices.Where(x => x.Close != 0 && x.Open != 0 && x.Low != 0 && x.High !=0).ToList();
			return historicalPriceList
				.Where(x => x.Low > 0)
				.GroupBy(x => new { ID = x.SecurityId })
				.Select(g => new { Average = g.Average(p => ((p.High - p.Low)/p.Low)*100) })
				.Select(x => x).FirstOrDefault().Average.Value;
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



		private decimal GetAverageCount(List<HistoricalPrice> historicalPrices, decimal averagePercentDrop)
		{
			//ABS(percentChange - Round((Low - Open) / open * 100, 1)) < .5;

			/*
			 myTable.GroupBy(t => new  {ID = t.ID})
   .Select (g => new {
            Average = g.Average (p => p.Score), 
            ID = g.Key.ID 
         })
			 */

			//	    AVG((Close - Open)/open * 100) INTO paramaverageDrop
			//			ABS(percentChange - Round((Low - Open) / open * 100, 1)) < .5



			var historicalPriceList = historicalPrices.Where(x => x.Close != 0 && x.Open != 0 && x.Low != 0 && x.High != 0).ToList();
			return historicalPriceList
				.Where(x =>
				//Math.Abs(x.PercentChange.Value - Math.Round(((x.Low.Value - x.Open.Value) / x.Open.Value) * 100, 1)) < (decimal?).5
				x.PercentChange < averagePercentDrop && x.Low.Value < x.Close * (decimal).996
				)
				.GroupBy(x => new { ID = x.SecurityId })
				.Select(g => new { Average = g.Average(p => (p.Close - p.Open) / p.Open * 100) })
				.Select(x => x).FirstOrDefault().Average.Value;

		}

	}
}



/*
				lowPercent,
		CASE WHEN  PreviousRunningTotal < dropCount * .05 THEN 1000 ELSE PreviousRunningTotal END,
		CASE WHEN  PreviousRunningTotal < dropCount * .10 THEN 1000 ELSE PreviousRunningTotal END, 
		CASE WHEN  PreviousRunningTotal < dropCount * .15 THEN 1000 ELSE PreviousRunningTotal END
				*/


				/*
                 SELECT 

                INSERT INTO
	 templowPercentTable
		 (lowPercent, 
		  percent5, 
		  percent10, 
		  percent15
		  ) 
 WITH
		cte 
  AS (SELECT 
			SecurityId
			,Round((Low - Open)/open * 100,1) AS lowPercent
			, COUNT(*) AS instanceCount
	  FROM 
			securities.HistoricalPrices
	 WHERE 
			securityid = locSecurityId
	 AND
			HistoricDate > curdate() -730
	 AND 
			ABS(percentChange  - Round((Low - Open)/open * 100,1)) < 1
	 GROUP BY
			SecurityId,Round((Low - Open)/open * 100,1))
	,
		cte3 
	AS
		(SELECT
				lowPercent, 
				instanceCount,
				SUM(instanceCount) OVER (PARTITION BY
												SecurityId 
										ORDER BY 
												lowPercent
										ROWS BETWEEN UNBOUNDED PRECEDING 
											 AND 1 PRECEDING
										) 
				AS  PreviousRunningTotal
		   FROM
				cte
  )
 
	SELECT
		lowPercent,
		CASE WHEN  PreviousRunningTotal < dropCount *.05 THEN 1000 ELSE PreviousRunningTotal END,
		CASE WHEN  PreviousRunningTotal < dropCount *.10 THEN 1000 ELSE PreviousRunningTotal END, 
		CASE WHEN  PreviousRunningTotal < dropCount *.15 THEN 1000 ELSE PreviousRunningTotal END 
	FROM
		cte3 a
	WHERE
		a.PreviousRunningTotal IS NOT NULL;


				SELECT 
			lowpercent  INTO parampercent5
	FROM 
		templowPercentTable
	WHERE
		percent5 = ( SELECT
						MIN(percent5) 
					 FROM
						templowPercentTable
					)
                    LIMIT 1;


	SELECT
		 lowpercent  INTO parampercent10 
	FROM
		templowPercentTable
     WHERE
		percent10 = ( SELECT 
						MIN(percent10) 
					  FROM 
						templowPercentTable
					)
                    LIMIT 1;




                    
	SELECT
		 lowpercent  INTO parampercent15
	FROM
		templowPercentTable
     WHERE
		percent15 = ( SELECT 
						MIN(percent15) 
					  FROM 
						templowPercentTable
					)
                    LIMIT 1;





                */