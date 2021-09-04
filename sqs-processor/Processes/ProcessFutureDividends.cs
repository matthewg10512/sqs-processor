using sqs_processor.Models;
using sqs_processor.Services.Network.Dividends;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Processes
{
    class ProcessFutureDividends : IProcess
    {
        private readonly ISecuritiesRepository _securityRepository;
        private readonly IGetDividendsServices _dividendService;
        public ProcessFutureDividends(ISecuritiesRepository securityRepository, IGetDividendsServices dividendService)
        {
            _securityRepository = securityRepository;
            _dividendService = dividendService;
        }

        public void RunTask()
        {
            // LambdaLogger.Log("jobName: " + job.jobName);
            var task = _securityRepository.GetTasks("FutureDividends");

            if (task != null)
            {

                try
                {
                    _dividendService.SetFutureURL();
                    string html = _dividendService.GetStringHtml("", "");
                    List<DividendDto> dividends = _dividendService.TransformData(html, 0);
                    dividends = _securityRepository.GetDividends(dividends);
                    _securityRepository.UpdateDividends(dividends);
                }
                catch (Exception ex)
                {

                }



                task.LastTaskRun = DateTime.Now;
                _securityRepository.UpdateTasks(task);
            }


        }
    }
}
