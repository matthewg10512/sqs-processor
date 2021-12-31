using sqs_processor.Models;
using sqs_processor.Services.Network.Dividends;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Text;
using sqs_processor.Services.Factories;

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
                    List<DividendDto> dividends= new List<DividendDto>();
                    foreach (var security in securities)
                    {
                        if (security.Id != 251)
                        {
                         //   continue;
                        }
                        string html = _dividendService.GetStringHtml(security.Symbol, "");
                         
                        dividends.AddRange(_dividendService.TransformData(html, 0));
                        if (dividends.Count > 1000)
                        {
                            dividends = _unitOfWork.dividendRepository.GetDividends(dividends);
                            _unitOfWork.dividendRepository.UpdateDividends(dividends);
                            dividends = new List<DividendDto>();
                            _unitOfWork.Dispose();
                            _unitOfWork = _unitOfWorkFactory.GetUnitOfWork();
                        }
                    }

                    if (dividends.Count > 0)
                    {
                        dividends = _unitOfWork.dividendRepository.GetDividends(dividends);
                        _unitOfWork.dividendRepository.UpdateDividends(dividends);
                    }
                    
                    
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
