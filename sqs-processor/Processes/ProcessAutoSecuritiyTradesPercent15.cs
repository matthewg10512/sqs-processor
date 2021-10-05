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
        private readonly ISecuritiesRepository _securityRepository;
         public ProcessAutoSecuritiyTradesPercent15(IServiceFactory serviceFactory)
        {
            _securityRepository = serviceFactory.GetSecuritiesRepository();
        }
        public void RunTask()
        {
            var securityTrades = _securityRepository.GetRecommendedSecurityTrades("percent15");
            Console.WriteLine("securityTrades Length" + securityTrades.Count);
            _securityRepository.ProcessAutoSecurityTrades(securityTrades);
        }
    }
}
