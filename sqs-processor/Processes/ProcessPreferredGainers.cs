using sqs_processor.Services.amazon;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Text;

using sqs_processor.Services.Factories;


namespace sqs_processor.Processes
{
    class ProcessPreferredGainers : IProcess
    {
        private readonly ISecuritiesRepository _securityRepository;
        private readonly IAmazonUtility _amazonUtility;

        public ProcessPreferredGainers(IServiceFactory serviceFactory)
        {
            _securityRepository = serviceFactory.GetSecuritiesRepository();
            _amazonUtility = serviceFactory.GetAmazonUtility();
        }

        public void RunTask()
        {

            var securityAlertType = _securityRepository.GetSecurityAlertType(1);
            var records = _securityRepository.SecurityAlertCheck(securityAlertType);

            _securityRepository.ProcessSecurityAlerts(records, securityAlertType);

            Console.WriteLine("Records Length" + records.Count);
            string message = _securityRepository.ConvertStringSecurityAlertCheck(records);
            if (message != "")
            {
                _amazonUtility.SendSNSMessage(securityAlertType.awsSNSURL, message);
            }

        }
    }
}
