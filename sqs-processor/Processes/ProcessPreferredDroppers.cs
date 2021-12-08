using sqs_processor.Services.amazon;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Text;

using sqs_processor.Services.Factories;


namespace sqs_processor.Processes
{
    class ProcessPreferredDroppers : IProcess
    {
        //private readonly ISecuritiesRepository _securityRepository;
        private readonly IAmazonUtility _amazonUtility;
        private readonly IUnitOfWork _unitOfWork;
        public ProcessPreferredDroppers(IServiceFactory serviceFactory)
        {
          //  _securityRepository = serviceFactory.GetSecuritiesRepository();
            _amazonUtility = serviceFactory.GetAmazonUtility();
            _unitOfWork = serviceFactory.GetUnitOfWorkFactoryService().GetUnitOfWork();
        }

        public void RunTask()
        {

            var securityAlertType = _unitOfWork.securityRepository.GetSecurityAlertType(4);
            var records = _unitOfWork.securityRepository.SecurityAlertCheck(securityAlertType);

            _unitOfWork.securityRepository.ProcessSecurityAlerts(records, securityAlertType);

            Console.WriteLine("Records Length" + records.Count);
            string message = _unitOfWork.securityRepository.ConvertStringSecurityAlertCheck(records);
            if (message != "")
            {
                _amazonUtility.SendSNSMessage(securityAlertType.awsSNSURL, message);
            }
            _unitOfWork.Dispose();

        }
    }
}
