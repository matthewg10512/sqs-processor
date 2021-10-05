using sqs_processor.Services.amazon;
using sqs_processor.Services.Factories;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Processes
{
    class ProcessDroppers : IProcess
    {
        private readonly ISecuritiesRepository _securityRepository;
        private readonly IAmazonUtility _amazonUtility;
        
        public ProcessDroppers(IServiceFactory serviceFactory)
        {
            _securityRepository = serviceFactory.GetSecuritiesRepository();
            _amazonUtility = serviceFactory.GetAmazonUtility();
        }

        public void RunTask()
        {
            var securityAlertType = _securityRepository.GetSecurityAlertType(2);
            var records = _securityRepository.SecurityAlertCheck(securityAlertType);


            _securityRepository.ProcessSecurityAlerts(records, securityAlertType);

            Console.WriteLine("Records Length" + records.Count);
            string message = _securityRepository.ConvertStringSecurityAlertCheck(records);
            Console.WriteLine("message" + message);
            if (message != "")
            {
                
                _amazonUtility.SendSNSMessage(securityAlertType.awsSNSURL, message);
            }
        }
    }
}
