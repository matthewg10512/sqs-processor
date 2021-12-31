using AutoMapper;
using sqs_processor.DbContexts;
using sqs_processor.Services.context;
using sqs_processor.Services.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Services.repos
{
   public class UnitofWorkFactory : IUnitofWorkFactory
    {
        IContextOptions _contextOptions;
        IMapper _mapper;
        IUtility _utility;
        public UnitofWorkFactory(IMapper mapper, IContextOptions contextOptions, IUtility utility)
        {
            _mapper = mapper;
            _contextOptions = contextOptions;
            _utility = utility;
        }

        public IUnitOfWork GetUnitOfWork()
        {

            SecuritiesLibraryContext context = new SecuritiesLibraryContext(_contextOptions.getContextOptions());
            IUnitOfWork unitOfWork = new UnitOfWork(_mapper, context, _utility);
            return unitOfWork;
        }
    }
}
