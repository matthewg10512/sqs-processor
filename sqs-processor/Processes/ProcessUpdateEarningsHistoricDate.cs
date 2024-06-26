﻿using sqs_processor.Models;
using sqs_processor.ResourceParameters;
using sqs_processor.Services.Factories;
using sqs_processor.Services.Network.HistoricalPrices;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace sqs_processor.Processes
{
    class ProcessUpdateEarningsHistoricDate : IProcess
    {
       // private readonly ISecuritiesRepository _securityRepository;
        IUnitOfWork _unitOfWork;
        private readonly IUnitofWorkFactory _unitOfWorkFactory;
        public ProcessUpdateEarningsHistoricDate(IServiceFactory serviceFactory)
        {
           // _securityRepository = serviceFactory.GetSecuritiesRepository();
           
            _unitOfWorkFactory = serviceFactory.GetUnitOfWorkFactoryService();
        }

      
        public void RunTask()
        {
            SecuritiesResourceParameters sr = new SecuritiesResourceParameters();
            //var securities = _securityRepository.GetSecurities(sr);
            _unitOfWork = _unitOfWorkFactory.GetUnitOfWork();
            var securities = _unitOfWork.securityRepository.GetSecuritiesSecurityId(sr).OrderBy(x=> x.Id);


            Parallel.ForEach(
                        securities,
    new ParallelOptions { MaxDegreeOfParallelism = 6 },
             security => { ProcessSecurityYearOverYearComparison(security); }
             
             );

          
            _unitOfWork.Dispose();
        }

        private void ProcessSecurityYearOverYearComparison(SecurityIdDto security)
        {
            IUnitOfWork unitOfWork = _unitOfWorkFactory.GetUnitOfWork();
            if (security.Id < 24000)
            {
                return;
            }
            try
                {
                unitOfWork.securityRepository.UpdateEarningsHistoricDate(security.Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Messages error " + ex.Message);
                }



            unitOfWork.Dispose();
        }
    }
}
