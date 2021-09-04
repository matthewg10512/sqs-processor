using sqs_processor.Models;
using sqs_processor.Services.Network;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Processes
{
    class ProcessUpdateSecurity : IProcess
    {
        private readonly ISecuritiesRepository _securityRepository;
        private readonly IGetSecurityService _securityService;
        public ProcessUpdateSecurity(ISecuritiesRepository securityRepository, IGetSecurityService securityService)
        {
            _securityRepository = securityRepository;
            _securityService = securityService;
    }

        public void RunTask()
        {
           // LambdaLogger.Log("jobName: " + job.jobName);
            var task = _securityRepository.GetTasks("SecurityUpdate");

            if (task != null)
            {
                // value += task.TaskName;
                //LambdaLogger.Log("Running Task: " + task.TaskName + "url - " + task.TaskUrl);
                //RunAsyncJob(task.TaskUrl);
                //RunAsyncJob("http://kwik-kards.com/FinancialServices/api/UpdateAllSecurities");

                //nasdaq
                string html = _securityService.GetStringHtml("nasdaq");
                List<SecurityForUpdateDto> securityDict = _securityService.TransformData(html, "");
                _securityRepository.UpdateSecurities(securityDict);

                //nyse
                html = _securityService.GetStringHtml("nyse");
                securityDict = _securityService.TransformData(html, "");
                _securityRepository.UpdateSecurities(securityDict);




                task.LastTaskRun = DateTime.Now;
                _securityRepository.UpdateTasks(task);
            }

            
        }
    }
}
