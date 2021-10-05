using sqs_processor.Services.amazon;
using sqs_processor.Services.Network;
using sqs_processor.Services.Network.Dividends;
using sqs_processor.Services.Network.Earnings;
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

        public ServiceFactory(ISecuritiesRepository securityRepository,IGetSecurityService securityService,
        IGetEarningsService earningService,
        IGetDividendsServices dividendsService,
        IAmazonUtility amazonUtility)
        {
            _securityRepository = securityRepository;
            _securityService =securityService;
            _earningService = earningService;
            _dividendsService = dividendsService;
            _amazonUtility = amazonUtility;
    
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

    }
}
