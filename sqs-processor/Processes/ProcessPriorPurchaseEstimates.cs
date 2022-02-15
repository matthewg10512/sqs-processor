using sqs_processor.Models;
using sqs_processor.Services.Factories;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sqs_processor.Processes
{
    public class ProcessPriorPurchaseEstimates : IProcess
    {
       // private readonly ISecuritiesRepository _securityRepository;
        private IUnitOfWork _unitOfWork;
        private readonly IUnitofWorkFactory _unitOfWorkFactory;
        public ProcessPriorPurchaseEstimates(IServiceFactory serviceFactory)
        {
            // _securityRepository = serviceFactory.GetSecuritiesRepository();
            _unitOfWorkFactory = serviceFactory.GetUnitOfWorkFactoryService();
        }
        public void RunTask()
        {
            _unitOfWork = _unitOfWorkFactory.GetUnitOfWork();
            var securities = _unitOfWork.securityRepository.GetSecurities(new ResourceParameters.SecuritiesResourceParameters());
            List<PriorPurchaseEstimateDto> priorPurchaseEstimates = new List<PriorPurchaseEstimateDto>();
            DateTime firstPurchaseDate = DateTime.Now;
            bool firstPurchase = false;
            foreach (var security in securities)
            {
                firstPurchaseDate = DateTime.Now;
                firstPurchase = false;
                if (security.CurrentPrice < 5)
                {
                    continue;
                }
                if(security.Id != 251)
                {
                    //continue;
                }
                var historicalPrices = _unitOfWork.securityRepository.GetHistoricalPrices(security.Id, new ResourceParameters.HistoricalPricesResourceParameters());
                historicalPrices = historicalPrices.OrderBy(x => x.HistoricDate).ToList();
                decimal totalPrice = 0;
                decimal totalShares = 0;
                int monthSet = 0;
                int purchaseFrequency = 0;//monthly
                foreach (var historicalPrice in historicalPrices)
                {
                    if (historicalPrice.HistoricDate.Month != monthSet)
                    {
                        monthSet = historicalPrice.HistoricDate.Month;
                        if (!firstPurchase){
                            firstPurchase = true;
                            firstPurchaseDate = historicalPrice.HistoricDate;
                        }

                        totalPrice += historicalPrice.Open.Value;
                        totalShares += 1;
                    }

                }
                PriorPurchaseEstimateDto securityPurchase = new PriorPurchaseEstimateDto();
                securityPurchase.SecurityId = security.Id;
                securityPurchase.DateCreated = DateTime.Now;
                securityPurchase.DateModified = DateTime.Now;
                securityPurchase.PurchasePrice = totalPrice;
                securityPurchase.Shares = totalShares;
                securityPurchase.FirstPurchaseDate = firstPurchaseDate;
                securityPurchase.PurchaseFrequency = purchaseFrequency;

                priorPurchaseEstimates.Add(securityPurchase);

                if (priorPurchaseEstimates.Count > 500)
                {
                    _unitOfWork.securityRepository.UpsertPriorPurchaseEstimates(priorPurchaseEstimates);
                    priorPurchaseEstimates = new List<PriorPurchaseEstimateDto>();
                    _unitOfWork.Dispose();
                    _unitOfWork = _unitOfWorkFactory.GetUnitOfWork();
                }

            }

            if (priorPurchaseEstimates.Count > 0)
            {
                _unitOfWork.securityRepository.UpsertPriorPurchaseEstimates(priorPurchaseEstimates);

            }
            _unitOfWork.Dispose();
        }

        
    }
}
