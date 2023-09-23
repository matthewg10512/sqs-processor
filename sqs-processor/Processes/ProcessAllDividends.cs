using sqs_processor.Models;
using sqs_processor.Services.Network.Dividends;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Text;
using sqs_processor.Services.Factories;
using System.Threading.Tasks;
using sqs_processor.Entities;

namespace sqs_processor.Processes
{
    class ProcessAllDividends : IProcess
    {
        //private readonly ISecuritiesRepository _securityRepository;
        private readonly IGetDividendsServices _dividendService;
        private IUnitOfWork _unitOfWork;
        private IUnitofWorkFactory _unitOfWorkFactory;
        public ProcessAllDividends(IServiceFactory serviceFactory)
        {
            //_securityRepository = serviceFactory.GetSecuritiesRepository();
            _dividendService = serviceFactory.GetDividendsServices();
            _unitOfWorkFactory = serviceFactory.GetUnitOfWorkFactoryService();
        }

        public void RunTask()
        {
            _unitOfWork = _unitOfWorkFactory.GetUnitOfWork();
            // LambdaLogger.Log("jobName: " + job.jobName);
            var task = _unitOfWork.securityRepository.GetTasks("ProcessAllDividends");

            if (task != null)
            {

                try
                {

                    var securities = _unitOfWork.securityRepository.GetSecurities(new ResourceParameters.SecuritiesResourceParameters());

                    Parallel.ForEach(
securities,
new ParallelOptions { MaxDegreeOfParallelism = 3 },
security => { ProcessDividend(security); }

);




                }
                catch (Exception ex)
                {

                }



                task.LastTaskRun = DateTime.Now;
                _unitOfWork.securityRepository.UpdateTasks(task);
            }

            _unitOfWork.Dispose();
        }
        private void ProcessDividend(Security security)
        {
            List<DividendDto> dividends = new List<DividendDto>();
            string html = _dividendService.GetStringHtml(security.Symbol, "");
            IUnitOfWork unitOfWork = _unitOfWorkFactory.GetUnitOfWork();
            dividends.AddRange(_dividendService.TransformData(html, 0));

            dividends = unitOfWork.dividendRepository.GetDividends(dividends);
            unitOfWork.dividendRepository.UpdateDividends(dividends);
        }

    }
}
