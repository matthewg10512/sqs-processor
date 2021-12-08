using AutoMapper;
using sqs_processor.DbContexts;
using sqs_processor.Services.context;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Services.repos
{
   public class UnitofWorkFactory : IUnitofWorkFactory
    {
        IContextOptions _contextOptions;
        IMapper _mapper;
        public UnitofWorkFactory(IMapper mapper, IContextOptions contextOptions)
        {
            _mapper = mapper;
            _contextOptions = contextOptions;
        }

        public IUnitOfWork GetUnitOfWork()
        {

            SecuritiesLibraryContext context = new SecuritiesLibraryContext(_contextOptions.getContextOptions());
            IUnitOfWork unitOfWork = new UnitOfWork(_mapper, context);
            return unitOfWork;
        }
    }
}
