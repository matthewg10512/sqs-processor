using sqs_processor.DbContexts;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Services.Utility
{
   public interface IUtility
    {
         void AddRecords(IEnumerable<object> records, SecuritiesLibraryContext _context);
         void UpdateRecords(IEnumerable<object> records, SecuritiesLibraryContext _context);

    }
}
