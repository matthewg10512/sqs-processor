using AutoMapper;
using PuppeteerSharp;
using sqs_processor.Models;
using sqs_processor.ResourceParameters;
using sqs_processor.Services.Factories;
using sqs_processor.Services.Network.StockSplits;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace sqs_processor.Processes
{


    public class ProcessStockSplitHistory : IProcess
    {
        private static bool processingRecords;
        
        private static IMapper _mapper;
        IUnitOfWork _unitOfWork;
        private readonly IUnitofWorkFactory _unitOfWorkFactory;
        private readonly IGetStockSplitHistory _stockSplitHistoryService;
        public ProcessStockSplitHistory(IServiceFactory serviceFactory)
        {
            _unitOfWork = serviceFactory.GetUnitOfWorkFactoryService().GetUnitOfWork();

            _mapper = serviceFactory.GetMapperService();

            _stockSplitHistoryService = serviceFactory.GetStockSplitHistoryService();
            _unitOfWorkFactory = serviceFactory.GetUnitOfWorkFactoryService();
        }

        public void RunTask()
        {


            List<StockSplitHistoryDto> stockSplits = new List<StockSplitHistoryDto>();

            SecuritiesResourceParameters sr = new SecuritiesResourceParameters();
            //var securities = _securityRepository.GetSecurities(sr);
            _unitOfWork = _unitOfWorkFactory.GetUnitOfWork();
            var securities = _unitOfWork.securityRepository.GetSecurities(sr);
            int totalStockSplit = 0;

           // List<HistoricalPriceforUpdateDto> historicalPrices = new List<HistoricalPriceforUpdateDto>();

            foreach (var security in securities)
            {
                try
                {
                    if (security.Id != 251)
                    {
                            //   continue;
                    }

                    string html = _stockSplitHistoryService.GetStringHtml(security);
                    stockSplits.AddRange(_stockSplitHistoryService.TransformData(html, security));
                    int stockSplitCount = stockSplits.Count;
                    if (stockSplitCount > 500)
                    {
                        totalStockSplit += stockSplitCount;
                        //_securityRepository.UpsertHistoricalPrices(historicalPrices);
                        _unitOfWork.securityRepository.UpsertStockSplitHistory(stockSplits);
                        stockSplits = new List<StockSplitHistoryDto>();
                        _unitOfWork.Dispose();
                        _unitOfWork = _unitOfWorkFactory.GetUnitOfWork();

                    }

                    
                    if (totalStockSplit > 300000)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Messages error " + ex.Message);
                }

            }

            try
            {
                if (stockSplits.Count > 0)
                {
                    _unitOfWork.securityRepository.UpsertStockSplitHistory(stockSplits);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Messages error " + ex.Message);
            }


            _unitOfWork.Dispose();





        }


        }
}
