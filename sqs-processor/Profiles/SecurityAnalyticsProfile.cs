using AutoMapper;
using sqs_processor.Entities;
using sqs_processor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Profiles
{
   public  class SecurityAnalyticsProfile : Profile
    {
        public SecurityAnalyticsProfile()
        {
            CreateMap<SecurityAnalytic, SecurityAnalyticDto>()
                     .ForMember(c => c.Id, option => option.Ignore());
            CreateMap<SecurityAnalyticDto, SecurityAnalytic>()
                     .ForMember(c => c.Id, option => option.Ignore());
        }
    }
}
