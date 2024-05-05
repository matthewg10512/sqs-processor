using sqs_processor.Models;
using sqs_processor.Services.Network;
using sqs_processor.Services.Network.Earnings;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Text;
using sqs_processor.Services.Factories;
using System.Threading.Tasks;
using sqs_processor.Entities;
using System.Threading;
using System.Linq;

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
                    var securities = _unitOfWork.securityRepository.GetSecuritiesSymbolSecurityId(new ResourceParameters.SecuritiesResourceParameters()).OrderBy(x => x.Id).ToList(); ;
                    

                    foreach (SecurityIdSymbolDto security in securities)
                    {
                        ProcessEarning(security);
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
        private void ProcessEarning(SecurityIdSymbolDto security)
        {
            IUnitOfWork unitOfWork = _unitOfWorkFactory.GetUnitOfWork();
            List<EarningDto> earnings = new List<EarningDto>();
            if (security.Id < 17345)
            {
               // return;
            }
            Thread.Sleep(200);
            _earningService.SetURL("");
            string html = _earningService.GetStringHtml(security.Symbol, "");
            earnings.AddRange(_earningService.TransformData(html, security.Id));

            earnings = unitOfWork.securityRepository.GetEarnings(earnings);
            unitOfWork.securityRepository.UpdateEarnings(earnings);
            unitOfWork.Dispose();


        }
    }
}
