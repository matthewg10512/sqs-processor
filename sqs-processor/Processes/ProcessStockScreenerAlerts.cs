using sqs_processor.ResourceParameters;
using sqs_processor.Services.Factories;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Processes
{
   public class ProcessStockScreenerAlerts :IProcess
    {

        private IUnitOfWork _unitOfWork;
        private readonly IUnitofWorkFactory _unitOfWorkFactory;
        public ProcessStockScreenerAlerts(IServiceFactory serviceFactory)
        {
            // _securityRepository = serviceFactory.GetSecuritiesRepository();
            _unitOfWorkFactory = serviceFactory.GetUnitOfWorkFactoryService();
        }

        public void RunTask()
        {
            _unitOfWork = _unitOfWorkFactory.GetUnitOfWork();
             StockScreenerSearchResourceParameters stockScreenResourceParams  = _unitOfWork.securityRepository.GetStockScreenerSearchDetails(1);

           var results =  _unitOfWork.securityRepository.GetStockScreenerResults(stockScreenResourceParams);

            int i = 0;
            i += 1;

        }
    }
}
