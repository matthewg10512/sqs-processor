using Microsoft.EntityFrameworkCore;
using sqs_processor.DbContexts;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Services.context
{
    public class ContextOptions : IContextOptions
    {
        private DbContextOptions<SecuritiesLibraryContext> _contextOptions;
        public ContextOptions(DbContextOptions<SecuritiesLibraryContext> contextOptions)
        {
            _contextOptions = contextOptions;
        }
        public DbContextOptions<SecuritiesLibraryContext> getContextOptions()
        {
            return _contextOptions;
        }
    }
}
