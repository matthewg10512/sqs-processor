using sqs_processor.Services.Network;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Processes
{
    class ProcessAutoSecuritiyTradesPercent5 : IProcess
    {
        private readonly ISecuritiesRepository _securityRepository;
         public ProcessAutoSecuritiyTradesPercent5(ISecuritiesRepository securityRepository)
        {
            _securityRepository = securityRepository;
        }
        public void RunTask()
        {
            var securityTrades = _securityRepository.GetRecommendedSecurityTrades("percent5");
            _securityRepository.ProcessAutoSecurityTrades(securityTrades);
        }
    }
}
