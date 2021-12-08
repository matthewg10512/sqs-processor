using AutoMapper;
using sqs_processor.Services.amazon;
using sqs_processor.Services.context;
using sqs_processor.Services.Network;
using sqs_processor.Services.Network.Dividends;
using sqs_processor.Services.Network.Earnings;
using sqs_processor.Services.Network.HistoricalPrices;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Services.Factories
{
    public class ServiceFactory : IServiceFactory
    {

        private readonly ISecuritiesRepository _securityRepository;
        private readonly IGetSecurityService _securityService;
        private readonly IGetEarningsService _earningService;
        private readonly IGetDividendsServices _dividendsService;
        private readonly IAmazonUtility _amazonUtility;
        private readonly IGetHistoricalPricesService _historicalPriceService;
        private readonly IMapper _mapper;
        private readonly IContextOptions _contextOptions;
        private readonly IUnitofWorkFactory _unitOfWorkFactoryService;
        

        public ServiceFactory(ISecuritiesRepository securityRepository,IGetSecurityService securityService,
        IGetEarningsService earningService,
        IGetDividendsServices dividendsService,
        IAmazonUtility amazonUtility,
        IGetHistoricalPricesService historicalPriceService,
            IMapper mapper,
           IContextOptions contextOptions, IUnitofWorkFactory
            unitOfWorkFactoryService)
        {
            _securityRepository = securityRepository;
            _securityService =securityService;
            _earningService = earningService;
            _dividendsService = dividendsService;
            _amazonUtility = amazonUtility;
            _historicalPriceService = historicalPriceService;
            _mapper = mapper;
            _contextOptions = contextOptions;
            _unitOfWorkFactoryService = unitOfWorkFactoryService;

        }

        public ISecuritiesRepository GetSecuritiesRepository()
        {
            return _securityRepository;
        }

        public IGetSecurityService GetGetSecurityService()
        {
            return _securityService;
        }

        public IGetEarningsService GetEarningsService()
        {
            return _earningService;
        }

        public IGetDividendsServices GetDividendsServices()
        {
            return _dividendsService;
        }

        public IAmazonUtility GetAmazonUtility()
        {
            return _amazonUtility;
        }

        public IGetHistoricalPricesService GetHistoricalPricesService()
        {
            return _historicalPriceService;
        }

        public IMapper GetMapperService()
        {
            return _mapper;
        }

        public IContextOptions GetContextOptionsService()
        {
            return _contextOptions;
        }

        public IUnitofWorkFactory GetUnitOfWorkFactoryService()
        {
            return _unitOfWorkFactoryService;
        }
        

    }
}
