using sqs_processor.Models;
using sqs_processor.Services.Factories;
using sqs_processor.Services.Network.Profile;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Processes
{
    class ProcessUpdateSecurityProfiles : IProcess
    {

        private readonly IGetSecurityProfile _securityProfileService;
        private readonly IUnitOfWork _unitOfWork;
        public ProcessUpdateSecurityProfiles(IServiceFactory serviceFactory)
        {
            _securityProfileService = serviceFactory.GetSecurityProfileService();
            _unitOfWork = serviceFactory.GetUnitOfWorkFactoryService().GetUnitOfWork();
        }
        public void RunTask()
        {


            var securities = _unitOfWork.securityRepository.GetSecurities(new ResourceParameters.SecuritiesResourceParameters());
            List<SecurityForUpdateDto> securityProfile = new List<SecurityForUpdateDto>();
            foreach (var security in securities)
            {
                if(security.Description != "")
                {
                    continue;
                }
               string results =  _securityProfileService.GetStringHtml(security.Symbol);
                securityProfile.AddRange(_securityProfileService.TransformData(results, security.Symbol, security.SecurityType));
                if(securityProfile.Count > 1000)
                {
                    _unitOfWork.securityRepository.UpsertSecurityProfile(securityProfile);
                    securityProfile = new List<SecurityForUpdateDto>();
                }
            }
            if (securityProfile.Count > 0)
            {
                _unitOfWork.securityRepository.UpsertSecurityProfile(securityProfile);
            }
            _unitOfWork.Dispose();
        }
    }
}
