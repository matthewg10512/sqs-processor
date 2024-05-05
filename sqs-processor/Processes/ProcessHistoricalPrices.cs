using sqs_processor.Models;
using sqs_processor.ResourceParameters;
using sqs_processor.Services.Factories;
using sqs_processor.Services.Network.HistoricalPrices;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

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
            var securities = _unitOfWork.securityRepository.GetSecuritiesSymbolSecurityId(sr).OrderBy(x=> x.Id);
            int totalHistorical = 0;
            int iSecurityCount = 0;

            List<HistoricalPriceforUpdateDto> historicalPrices = new List<HistoricalPriceforUpdateDto>();

            Parallel.ForEach(
                        securities,
    new ParallelOptions { MaxDegreeOfParallelism = 3 },
             security => { ProcessHistoricalPrice(security); }
             
             );

            /*
            foreach (var security in securities)
            {
                try
                {
                    if (security.Id  < 21380)
                    {
                       // continue;
                    }
                    //var historicalPrice = _securityRepository.GetHistoricalPricesRange(security.Id);
                    var historicalPrice = _unitOfWork.securityRepository.GetLastHistoricalPrice(security.Id);
                    //there aren't any values for this
                    var stockSplitHistories = _unitOfWork.securityRepository.GetStockSplitHistories(security.Id,  new StockSplitHistoriesResourceParameters()
                        { HistoricDateHigh = DateTime.UtcNow ,HistoricDateLow = DateTime.UtcNow.AddDays(-60)}
                    );
                    

                    if (historicalPrice == null)
                    {
                        _historicalPriceService.startRange = DateTime.Now.AddDays(-5000);
                    }
                    else if (stockSplitHistories.Count() > 0)
                    {
                        var earliestHistoricPrice = _unitOfWork.securityRepository.GetFirstHistoricalPrice(security.Id);
                        _historicalPriceService.startRange = earliestHistoricPrice.HistoricDate.AddDays(-1);
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
                    if (totalHistorical > 600000)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Messages error " + ex.Message);
                }

            }

        



            if (historicalPrices.Count > 0)
            {
                //_securityRepository.UpsertHistoricalPrices(historicalPrices);
                _unitOfWork.securityRepository.UpsertHistoricalPrices(historicalPrices);


            }
            foreach (var security in securities)
            {
                try
                {
                    _unitOfWork.securityRepository.UpdatePercentageChangeHistoricPrice(security.Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Messages error " + ex.Message);
                }
            }
                

            
            */
            _unitOfWork.Dispose();
        }

        private void ProcessHistoricalPrice(SecurityIdSymbolDto security)
        {
            IUnitOfWork unitOfWork = _unitOfWorkFactory.GetUnitOfWork();
            int totalHistorical = 0;
            int iSecurityCount = 0;

            List<HistoricalPriceforUpdateDto> historicalPrices = new List<HistoricalPriceforUpdateDto>();

                try
                {
              
                
                    if (security.Id  < 8400)
                    {
                        //return;
                    }
                Thread.Sleep(500);
                //var historicalPrice = _securityRepository.GetHistoricalPricesRange(security.Id);
                var historicalPrice = unitOfWork.securityRepository.GetLastHistoricalPrice(security.Id);
                    //there aren't any values for this
                    var stockSplitHistories = unitOfWork.securityRepository.GetStockSplitHistories(security.Id,  new StockSplitHistoriesResourceParameters()
                        { HistoricDateHigh = DateTime.UtcNow ,HistoricDateLow = DateTime.UtcNow.AddDays(-15)}
                    );
                    

                    if (historicalPrice == null)
                    {
                        _historicalPriceService.startRange = DateTime.Now.AddDays(-5000);
                    }
                    else if (stockSplitHistories.Count() > 0)
                    {
                        var earliestHistoricPrice = unitOfWork.securityRepository.GetFirstHistoricalPrice(security.Id);
                        _historicalPriceService.startRange = earliestHistoricPrice.HistoricDate.AddDays(-1);
                    }
                    else
                    {

                        _historicalPriceService.startRange = historicalPrice.HistoricDate.AddDays(-1);



                    }

                    _historicalPriceService.endRange = DateTime.Now;

                    string html = _historicalPriceService.GetStringHtml(security);
                    historicalPrices.AddRange(_historicalPriceService.TransformData(html, security.Id));
                    int historicalCount = historicalPrices.Count;
                    if (historicalCount > 0)
                    {
                        totalHistorical += historicalCount;
                    //_securityRepository.UpsertHistoricalPrices(historicalPrices);
                    unitOfWork.securityRepository.UpsertHistoricalPrices(historicalPrices);
                        historicalPrices = new List<HistoricalPriceforUpdateDto>();
                        //_unitOfWork.Dispose();
                        //_unitOfWork = _unitOfWorkFactory.GetUnitOfWork();

                    }
                    iSecurityCount += 1;
                 
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Messages error " + ex.Message);
                }

            

        



            try
                {
                unitOfWork.securityRepository.UpdatePercentageChangeHistoricPrice(security.Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Messages error " + ex.Message);
                }



            unitOfWork.Dispose();
        }
    }
}
