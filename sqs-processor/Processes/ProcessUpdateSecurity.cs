using sqs_processor.Models;
using sqs_processor.Services.Network;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Text;
using sqs_processor.Services.Factories;


namespace sqs_processor.Processes
{
    class ProcessUpdateSecurity : IProcess
    {
        //private readonly ISecuritiesRepository _securityRepository;
        private readonly IGetSecurityService _securityService;
        private readonly IUnitOfWork _unitOfWork;
        public ProcessUpdateSecurity(IServiceFactory serviceFactory)
        {
            //_securityRepository = serviceFactory.GetSecuritiesRepository();
            _securityService = serviceFactory.GetGetSecurityService();
            _unitOfWork = serviceFactory.GetUnitOfWorkFactoryService().GetUnitOfWork();
    }

        public void RunTask()
        {
           // LambdaLogger.Log("jobName: " + job.jobName);
            var task = _unitOfWork.securityRepository.GetTasks("SecurityUpdate");

            if (task != null)
            {
                // value += task.TaskName;
                //LambdaLogger.Log("Running Task: " + task.TaskName + "url - " + task.TaskUrl);
                //RunAsyncJob(task.TaskUrl);
                //RunAsyncJob("http://kwik-kards.com/FinancialServices/api/UpdateAllSecurities");

                //nasdaq
                string html = _securityService.GetStringHtml("nasdaq");
                List<SecurityForUpdateDto> securityDict = _securityService.TransformData(html, "");
                _unitOfWork.securityRepository.UpdateSecurities(securityDict);

                //nyse
                html = _securityService.GetStringHtml("nyse");
                securityDict = _securityService.TransformData(html, "");
                _unitOfWork.securityRepository.UpdateSecurities(securityDict);




                task.LastTaskRun = DateTime.Now;
                _unitOfWork.securityRepository.UpdateTasks(task);
            }
            _unitOfWork.Dispose();



        }
    }
}
