using sqs_processor.Entities;
using sqs_processor.Models;
using sqs_processor.ResourceParameters;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Services.repos
{
    public interface IDividendsRepository
    {
        public IEnumerable<Dividend> GetDividends(DividendsResourceParameters dividendsResourceParameters);

        IEnumerable<Dividend> GetDividends(int securityId);

        Dividend GetDividend(int securityId, int dividendId);
        public List<DividendDto> GetDividends(List<DividendDto> dividends);

        public IEnumerable<Tuple<Dividend, Security>> GetSecuritiesDividends(IEnumerable<Dividend> dividends);

        public void UpdateDividends(List<DividendDto> dividends);
       // public void UpdateDividends(List<DividendDto> dividends, Security security);
        bool Save();
    }
}
