using AutoMapper;
using sqs_processor.Entities;
using sqs_processor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Profiles
{
   public  class HistoricPerformancesProfile : Profile
    {
        public HistoricPerformancesProfile()
        {
            CreateMap<HistoricPerformanceDto, HistoricPerformance>()
                     .ForMember(c => c.Id, option => option.Ignore());
        }
    }
}
