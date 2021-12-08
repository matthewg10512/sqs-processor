using sqs_processor.Services.Network;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Text;
using sqs_processor.Services.Factories;


namespace sqs_processor.Processes
{
    class ProcessAutoSecuritiyTradesPercent15 : IProcess
    {
        //private readonly ISecuritiesRepository _securityRepository;
        private readonly IUnitOfWork _unitOfWork;
        public ProcessAutoSecuritiyTradesPercent15(IServiceFactory serviceFactory)
        {
            //_securityRepository = serviceFactory.GetSecuritiesRepository();
            _unitOfWork = serviceFactory.GetUnitOfWorkFactoryService().GetUnitOfWork();
        }
        public void RunTask()
        {
            var securityTrades = _unitOfWork.securityRepository.GetRecommendedSecurityTrades("percent15");
            Console.WriteLine("securityTrades Length" + securityTrades.Count);
            _unitOfWork.securityRepository.ProcessAutoSecurityTrades(securityTrades);
            _unitOfWork.Dispose();
        }
    }
}
