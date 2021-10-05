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
   public interface IServiceFactory
    {


        public ISecuritiesRepository GetSecuritiesRepository();

        public IGetSecurityService GetGetSecurityService();

        public IGetEarningsService GetEarningsService();
        public IGetDividendsServices GetDividendsServices();

        public IAmazonUtility GetAmazonUtility();

    }
}
