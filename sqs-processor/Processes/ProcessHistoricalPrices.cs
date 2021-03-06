using sqs_processor.Models;
using sqs_processor.ResourceParameters;
using sqs_processor.Services.Factories;
using sqs_processor.Services.Network.HistoricalPrices;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace sqs_processor.Processes
{
    class ProcessHistoricalPrices : IProcess
    {
       // private readonly ISecuritiesRepository _securityRepository;
        private readonly IGetHistoricalPricesService _historicalPriceService;
        IUnitOfWork _unitOfWork;
        private readonly IUnitofWorkFactory _unitOfWorkFactory;
        public ProcessHistoricalPrices(IServiceFactory serviceFactory)
        {
           // _securityRepository = serviceFactory.GetSecuritiesRepository();
            _historicalPriceService = serviceFactory.GetHistoricalPricesService();
            _unitOfWorkFactory = serviceFactory.GetUnitOfWorkFactoryService();
        }
        public void RunTask()
        {
            SecuritiesResourceParameters sr = new SecuritiesResourceParameters();
            //var securities = _securityRepository.GetSecurities(sr);
            _unitOfWork = _unitOfWorkFactory.GetUnitOfWork();
            var securities = _unitOfWork.securityRepository.GetSecurities(sr);
            int totalHistorical = 0;
            int iSecurityCount = 0;

            List<HistoricalPriceforUpdateDto> historicalPrices = new List<HistoricalPriceforUpdateDto>();

            foreach (var security in securities)
            {
                if(security.Id < 18983)
                {
                 // continue;
                }
                //var historicalPrice = _securityRepository.GetHistoricalPricesRange(security.Id);
                var historicalPrice = _unitOfWork.securityRepository.GetHistoricalPricesRange(security.Id);
                //there aren't any values for this
                if (historicalPrice == null)
                {
                    _historicalPriceService.startRange = DateTime.Now.AddDays(-4000);
                }
                else
                {

                    _historicalPriceService.startRange = historicalPrice.HistoricDate.AddDays(-1);



                }

                _historicalPriceService.endRange = DateTime.Now;

                string html = _historicalPriceService.GetStringHtml(security);
                historicalPrices.AddRange(_historicalPriceService.TransformData(html, security.Id));
                int historicalCount = historicalPrices.Count;
                if (historicalCount > 500)
                {
                    totalHistorical += historicalCount;
                    //_securityRepository.UpsertHistoricalPrices(historicalPrices);
                    _unitOfWork.securityRepository.UpsertHistoricalPrices(historicalPrices);
                    historicalPrices = new List<HistoricalPriceforUpdateDto>();
                    _unitOfWork.Dispose();
                    _unitOfWork = _unitOfWorkFactory.GetUnitOfWork();

                }
                iSecurityCount += 1;
                if(totalHistorical > 300000)
                {
                    break;
                }


            }

            if (historicalPrices.Count > 0)
            {
                //_securityRepository.UpsertHistoricalPrices(historicalPrices);
                _unitOfWork.securityRepository.UpsertHistoricalPrices(historicalPrices);


            }
            _unitOfWork.Dispose();
        }
    }
}
