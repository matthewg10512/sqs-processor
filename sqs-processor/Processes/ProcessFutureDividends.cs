using sqs_processor.Models;
using sqs_processor.Services.Network.Dividends;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Text;
using sqs_processor.Services.Factories;

namespace sqs_processor.Processes
{
    class ProcessFutureDividends : IProcess
    {
        //private readonly ISecuritiesRepository _securityRepository;
        private readonly IGetDividendsServices _dividendService;
        private readonly IUnitOfWork _unitOfWork;
        public ProcessFutureDividends(IServiceFactory serviceFactory)
        {
            //_securityRepository = serviceFactory.GetSecuritiesRepository();
            _dividendService = serviceFactory.GetDividendsServices();
            _unitOfWork = serviceFactory.GetUnitOfWorkFactoryService().GetUnitOfWork();
        }

        public void RunTask()
        {
            // LambdaLogger.Log("jobName: " + job.jobName);
            var task = _unitOfWork.securityRepository.GetTasks("FutureDividends");

            if (task != null)
            {

                try
                {
                    _dividendService.SetFutureURL();
                    string html = _dividendService.GetStringHtml("", "");
                    List<DividendDto> dividends = _dividendService.TransformData(html, 0);
                    dividends = _unitOfWork.dividendRepository.GetDividends(dividends);
                    _unitOfWork.dividendRepository.UpdateDividends(dividends);
                }
                catch (Exception ex)
                {

                }



                task.LastTaskRun = DateTime.Now;
                _unitOfWork.securityRepository.UpdateTasks(task);
            }

            _unitOfWork.Dispose();
        }
    }
}
