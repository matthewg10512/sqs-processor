using AutoMapper;
using sqs_processor.Entities;
using sqs_processor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Profiles
{
   public  class StockSplitHistoriesProfile : Profile
    {
        public StockSplitHistoriesProfile()
        {
            CreateMap<StockSplitHistoryDto, StockSplitHistory>()
                     .ForMember(c => c.Id, option => option.Ignore());
        }
    }
}
