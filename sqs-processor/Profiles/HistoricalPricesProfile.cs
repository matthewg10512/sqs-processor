using AutoMapper;
using sqs_processor.Entities;
using sqs_processor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Profiles
{
    public class HistoricalPricesProfile : Profile
    {
        public HistoricalPricesProfile()
        {
            CreateMap<HistoricalPrice, HistoricalPriceDto>();
            CreateMap<HistoricalPriceforUpdateDto, HistoricalPrice>()
                 .ForMember(c => c.Id, option => option.Ignore());
        }
    }
}
