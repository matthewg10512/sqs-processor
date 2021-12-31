using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Services.repos
{
    public interface IUnitOfWork
    {
        ISecuritiesRepository securityRepository { get; }
        IDividendsRepository dividendRepository { get; }
        public void Dispose();
    }
}
