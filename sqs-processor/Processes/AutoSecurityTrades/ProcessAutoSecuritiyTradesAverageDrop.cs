using sqs_processor.Services.Factories;
using sqs_processor.Services.Network;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Processes
{
    class ProcessAutoSecuritiyTradesAverageDrop : IProcess
    {
        //private readonly ISecuritiesRepository _securityRepository;
        private readonly IUnitOfWork _unitOfWork;
        public ProcessAutoSecuritiyTradesAverageDrop(IServiceFactory serviceFactory)
        {
            //_securityRepository = serviceFactory.GetSecuritiesRepository();
        }
        public void RunTask()
        {
            var securityTrades = _unitOfWork.securityRepository.GetRecommendedSecurityTrades("averagedrop");
            Console.WriteLine("securityTrades Length" + securityTrades.Count);
            _unitOfWork.securityRepository.ProcessAutoSecurityTrades(securityTrades);
            _unitOfWork.Dispose();
        }
    }
}
