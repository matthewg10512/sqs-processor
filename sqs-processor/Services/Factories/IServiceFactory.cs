﻿using AutoMapper;
using sqs_processor.Services.amazon;
using sqs_processor.Services.context;
using sqs_processor.Services.Network;
using sqs_processor.Services.Network.Dividends;
using sqs_processor.Services.Network.Earnings;
using sqs_processor.Services.Network.HistoricalPrices;
using sqs_processor.Services.Network.Profile;
using sqs_processor.Services.Network.StockSplits;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Services.Factories
{
   public interface IServiceFactory
    {


        public ISecuritiesRepository GetSecuritiesRepository();

        public IGetSecurityService GetGetSecurityService();

        public IGetEarningsService GetEarningsService();
        public IGetDividendsServices GetDividendsServices();

        public IAmazonUtility GetAmazonUtility();
        public IGetHistoricalPricesService GetHistoricalPricesService();
        public IMapper GetMapperService();
        public IContextOptions GetContextOptionsService();
        public IUnitofWorkFactory GetUnitOfWorkFactoryService();
        public IGetSecurityProfile GetSecurityProfileService();
        public IGetStockSplitHistory GetStockSplitHistoryService();
    }
}
