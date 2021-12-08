using Microsoft.EntityFrameworkCore;
using sqs_processor.DbContexts;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Services.context
{
   public interface IContextOptions
    {
        public DbContextOptions<SecuritiesLibraryContext> getContextOptions();
    }
}
