using sqs_processor.Models;
using sqs_processor.Services.Factories;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sqs_processor.Processes
{
    public class ProcessSecurityPurchaseChecks : IProcess
    {
       // private readonly ISecuritiesRepository _securityRepository;
        private IUnitOfWork _unitOfWork;
        private readonly IUnitofWorkFactory _unitOfWorkFactory;
        public ProcessSecurityPurchaseChecks(IServiceFactory serviceFactory)
        {
            // _securityRepository = serviceFactory.GetSecuritiesRepository();
            _unitOfWorkFactory = serviceFactory.GetUnitOfWorkFactoryService();
        }
        public void RunTask()
        {
            _unitOfWork = _unitOfWorkFactory.GetUnitOfWork();
            var securities = _unitOfWork.securityRepository.GetSecurities(new ResourceParameters.SecuritiesResourceParameters());
            List<SecurityPurchaseCheckDto> securityPurchases = new List<SecurityPurchaseCheckDto>();
            foreach (var security in securities)
            {
                if (security.CurrentPrice < 5)
                {
                    continue;
                }
                var historicalPrices = _unitOfWork.securityRepository.GetHistoricalPrices(security.Id, new ResourceParameters.HistoricalPricesResourceParameters());
                historicalPrices = historicalPrices.OrderBy(x => x.HistoricDate).ToList();
                decimal totalPrice = 0;
                decimal totalShares = 0;
                int loopPurchase = 0;
                foreach (var historicalPrice in historicalPrices)
                {
                    if (loopPurchase > 20)
                    {
                        totalPrice += historicalPrice.Open.Value;
                        totalShares += 1;
                        loopPurchase = 1;
                    }
                    loopPurchase += 1;

                }
                SecurityPurchaseCheckDto securityPurchase = new SecurityPurchaseCheckDto();
                securityPurchase.SecurityId = security.Id;
                securityPurchase.DateCreated = DateTime.Now;
                securityPurchase.DateModified = DateTime.Now;
                securityPurchase.PurchasePrice = totalPrice;
                securityPurchase.Shares = totalShares;
                securityPurchases.Add(securityPurchase);

                if (securityPurchases.Count > 500)
                {
                    _unitOfWork.securityRepository.UpsertSecurityPurchaseChecks(securityPurchases);
                    securityPurchases = new List<SecurityPurchaseCheckDto>();
                    _unitOfWork.Dispose();
                    _unitOfWork = _unitOfWorkFactory.GetUnitOfWork();
                }

            }

            if (securityPurchases.Count > 0)
            {
                _unitOfWork.securityRepository.UpsertSecurityPurchaseChecks(securityPurchases);

            }
            _unitOfWork.Dispose();
        }

        
    }
}
