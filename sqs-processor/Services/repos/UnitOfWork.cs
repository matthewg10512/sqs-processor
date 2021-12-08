using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using sqs_processor.DbContexts;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Services.repos
{
    class UnitOfWork : IUnitOfWork 
    {
        private ISecuritiesRepository _securityRepository;

        private SecuritiesLibraryContext _context;

        IMapper _mapper;
        public UnitOfWork(
            //SecuritiesLibraryContext dbContext, 
            //IConfiguration config,
            IMapper mapper, SecuritiesLibraryContext context)
        {

            _context = context;// new SecuritiesLibraryContext(contextOptions);
            // _dbContext = dbContext;
            //_config = config;
            _mapper = mapper;
        }


        public ISecuritiesRepository securityRepository
        {
            get
            {
                return _securityRepository ??
                    (_securityRepository = new SecuritiesRepository(_context, 
                     _mapper));
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
