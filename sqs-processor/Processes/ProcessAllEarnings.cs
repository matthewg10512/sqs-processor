using sqs_processor.Models;
using sqs_processor.Services.Network;
using sqs_processor.Services.Network.Earnings;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Text;
using sqs_processor.Services.Factories;

namespace sqs_processor.Processes
{
    class ProcessAllEarnings : IProcess
    {
        //private readonly ISecuritiesRepository _securityRepository;
        private readonly IGetEarningsService _earningService;
        private IUnitOfWork _unitOfWork;
        private IUnitofWorkFactory _unitOfWorkFactory;
        public ProcessAllEarnings(IServiceFactory serviceFactory)
        {
            //_securityRepository = serviceFactory.GetSecuritiesRepository();
            _earningService = serviceFactory.GetEarningsService();
            _unitOfWorkFactory = serviceFactory.GetUnitOfWorkFactoryService();
    }

        public void RunTask()
        {
            _unitOfWork = _unitOfWorkFactory.GetUnitOfWork();
            // LambdaLogger.Log("jobName: " + job.jobName);
            var task = _unitOfWork.securityRepository.GetTasks("ProcessAllEarnings");

            if (task != null)
            {

                try
                {
                    var securities = _unitOfWork.securityRepository.GetSecurities(new ResourceParameters.SecuritiesResourceParameters());
                    List<EarningDto> earnings = new List<EarningDto>();
                    foreach (var security in securities)
                    {
                        _earningService.SetURL("");
                        string html = _earningService.GetStringHtml(security.Symbol, "");
                        earnings.AddRange(_earningService.TransformData(html, security.Id));
                        if (earnings.Count > 1000)
                        {
                            earnings = _unitOfWork.securityRepository.GetEarnings(earnings);
                            _unitOfWork.securityRepository.UpdateEarnings(earnings);
                            _unitOfWork.Dispose();
                            _unitOfWork = _unitOfWorkFactory.GetUnitOfWork();
                            earnings = new List<EarningDto>();
                        }
                    }
                    if (earnings.Count > 0)
                    {
                        earnings = _unitOfWork.securityRepository.GetEarnings(earnings);
                        _unitOfWork.securityRepository.UpdateEarnings(earnings);

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
