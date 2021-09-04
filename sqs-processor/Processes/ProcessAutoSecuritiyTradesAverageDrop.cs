using sqs_processor.Services.Network;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Processes
{
    class ProcessAutoSecuritiyTradesAverageDrop : IProcess
    {
        private readonly ISecuritiesRepository _securityRepository;
         public ProcessAutoSecuritiyTradesAverageDrop(ISecuritiesRepository securityRepository)
        {
            _securityRepository = securityRepository;
        }
        public void RunTask()
        {
            var securityTrades = _securityRepository.GetRecommendedSecurityTrades("averagedrop");
            _securityRepository.ProcessAutoSecurityTrades(securityTrades);
        }
    }
}
