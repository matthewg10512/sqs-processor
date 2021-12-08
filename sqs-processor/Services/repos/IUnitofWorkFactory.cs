using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Services.repos
{
   public interface IUnitofWorkFactory
    {

        public IUnitOfWork GetUnitOfWork();
        

        

    }
}
