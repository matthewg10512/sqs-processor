using sqs_processor.Entities;
using sqs_processor.ResourceParameters;
using sqs_processor.Services.amazon;
using sqs_processor.Services.Factories;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sqs_processor.Processes
{
   public class ProcessStockScreenerAlerts :IProcess
    {
        private readonly IAmazonUtility _amazonUtility;
        private IUnitOfWork _unitOfWork;
        private readonly IUnitofWorkFactory _unitOfWorkFactory;
        public ProcessStockScreenerAlerts(IServiceFactory serviceFactory)
        {
            // _securityRepository = serviceFactory.GetSecuritiesRepository();
            _unitOfWorkFactory = serviceFactory.GetUnitOfWorkFactoryService();
            _amazonUtility = serviceFactory.GetAmazonUtility();
        }

        public void RunTask()
        {
            _unitOfWork = _unitOfWorkFactory.GetUnitOfWork();

            var marketClosed = _unitOfWork.securityRepository.IsMarketClosed(DateTime.UtcNow);

            var stockScreeners = _unitOfWork.securityRepository.GetStockScreeners();

            foreach (var stockScreener in stockScreeners)
            {
                if(stockScreener.AlertType != 1)
                {
                    continue;
                }

                var screenAlertsType = _unitOfWork.securityRepository.GetStockScreenerAlertType(stockScreener.AlertType);

                StockScreenerSearchResourceParameters stockScreenResourceParams = _unitOfWork.securityRepository.GetStockScreenerSearchDetails(stockScreener.id);

                var screenerResults = _unitOfWork.securityRepository.GetStockScreenerResults(stockScreenResourceParams);

                if (stockScreener.OnlyPreferred)
                {
                    screenerResults = screenerResults.Where(x => x.Security.preferred == true).ToList();
                }
                var recordstoAdd = _unitOfWork.securityRepository.GetNewStockScreenerAlertsHistory(screenerResults.Select(x => x.Security).ToList(), stockScreener.id);

                _unitOfWork.securityRepository.AddStockScreenerAlertsHistoryRecords(recordstoAdd);


                if (stockScreener.AutoTrade)
                {

                    var securityTrades = screenerResults.Join(recordstoAdd, x => x.Security.Id, y => y.SecurityId, (screenerResult, recordstoAdd) => new { screenerResult, recordstoAdd })
                        .Select(x => new AutoSecurityTrade {
                            PercentageLevel = 1,
                            PurchaseDate = DateTime.Now, 
                            PurchasePrice = x.screenerResult.Security.CurrentPrice, 
                            SecurityId = x.screenerResult.Security.Id, 
                            SharesBought= 1 }).ToList();
                    
                    //Console.WriteLine("securityTrades Length" + securityTrades.Count);
                    _unitOfWork.securityRepository.ProcessAutoSecurityTrades(securityTrades);


                }
                
                string message = _unitOfWork.securityRepository.ConvertStringScreenerAlertTypeMessage(recordstoAdd);
                Console.WriteLine("message" + message);
                if (message != "")
                {
                    message += Environment.NewLine + stockScreener.Name +message;

                    _amazonUtility.SendSNSMessage(screenAlertsType.awsSNSURL, message);
                }

            }



            

            int i = 0;
            i += 1;

        }
    }
}
