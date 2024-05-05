using AutoMapper;
using sqs_processor.Entities;
using sqs_processor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Profiles
{
   public  class CuurentBullRunsProfile : Profile
    {
        public CuurentBullRunsProfile()
        {
            CreateMap<CurrentBullBearRunDto, BullBearRunDto>()
                .ForMember(c => c.Id, option => option.Ignore());
            CreateMap<BullBearRunDto, CurrentBullBearRunDto>()
                     .ForMember(c => c.Id, option => option.Ignore());

            CreateMap<CurrentBullBearRun, CurrentBullBearRunDto>()
                     .ForMember(c => c.Id, option => option.Ignore());
            CreateMap<CurrentBullBearRunDto, CurrentBullBearRun>()
                     .ForMember(c => c.Id, option => option.Ignore());
        }
    }
}
