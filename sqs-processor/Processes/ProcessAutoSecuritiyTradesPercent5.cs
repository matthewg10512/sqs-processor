using sqs_processor.Services.Network;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Text;
using sqs_processor.Services.Factories;

namespace sqs_processor.Processes
{
    class ProcessAutoSecuritiyTradesPercent5 : IProcess
    {
        private readonly ISecuritiesRepository _securityRepository;
         public ProcessAutoSecuritiyTradesPercent5(IServiceFactory serviceFactory)
        {
            _securityRepository = serviceFactory.GetSecuritiesRepository();
        }
        public void RunTask()
        {
            var securityTrades = _securityRepository.GetRecommendedSecurityTrades("percent5");
            Console.WriteLine("securityTrades Length" + securityTrades.Count);
            _securityRepository.ProcessAutoSecurityTrades(securityTrades);
        }
    }
}
