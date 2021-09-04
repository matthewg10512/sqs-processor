using sqs_processor.Services.amazon;
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
        private readonly string snsTopicArn = "arn:aws:sns:us-east-2:930271955226:Droppers";

        public ProcessDroppers(ISecuritiesRepository securityRepository, IAmazonUtility amazonUtility)
        {
            _securityRepository = securityRepository;
            _amazonUtility = amazonUtility;
        }

        public void RunTask()
        {
            var records = _securityRepository.SecurityAlertCheck(2);
            string message = _securityRepository.ConvertStringSecurityAlertCheck(records);
            if (message != "")
            {
                _amazonUtility.SendSNSMessage(snsTopicArn, message);
            }
        }
    }
}
