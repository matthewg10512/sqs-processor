using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using sqs_processor.DbContexts;
using sqs_processor.Services.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Services.repos
{
    class UnitOfWork : IUnitOfWork 
    {
        private ISecuritiesRepository _securityRepository;
        private IDividendsRepository _dividendRepository;

        private SecuritiesLibraryContext _context;

        IMapper _mapper;

        IUtility _utility;

        public UnitOfWork(
            //SecuritiesLibraryContext dbContext, 
            //IConfiguration config,
            IMapper mapper, SecuritiesLibraryContext context, IUtility utility)
        {

            _context = context;// new SecuritiesLibraryContext(contextOptions);
            // _dbContext = dbContext;
            //_config = config;
            _mapper = mapper;
            _utility = utility;
        }
        public IDividendsRepository dividendRepository
        {

            get
            {
                return _dividendRepository ??
                    (_dividendRepository = new DividendsRepository(_context,
                     _mapper, _utility));
            }
        }

        public ISecuritiesRepository securityRepository
        {
            get
            {
                return _securityRepository ??
                    (_securityRepository = new SecuritiesRepository(_context, 
                     _mapper, _utility));
            }
        }

        public void ClearContext(){
            _securityRepository = null;
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
