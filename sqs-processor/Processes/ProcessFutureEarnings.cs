﻿using sqs_processor.Models;
using sqs_processor.Services.Network;
using sqs_processor.Services.Network.Earnings;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Processes
{
    class ProcessFutureEarnings : IProcess
    {
        private readonly ISecuritiesRepository _securityRepository;
        private readonly IGetEarningsService _earningService;
        public ProcessFutureEarnings(ISecuritiesRepository securityRepository, IGetEarningsService earningService)
        {
            _securityRepository = securityRepository;
            _earningService = earningService;
    }

        public void RunTask()
        {
           // LambdaLogger.Log("jobName: " + job.jobName);
            var task = _securityRepository.GetTasks("FutureEarnings");

            if (task != null)
            {

                try
                {
                    _earningService.SetURL("");
                    string html = _earningService.GetStringHtml("", "");
                    List<EarningDto> earnings = _earningService.TransformData(html, 0);
                    earnings = _securityRepository.GetEarnings(earnings);
                    _securityRepository.UpdateEarnings(earnings);
                }
                catch(Exception ex)
                {

                }



                task.LastTaskRun = DateTime.Now;
                _securityRepository.UpdateTasks(task);
            }

            
        }
    }
}
