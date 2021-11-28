using AutoMapper;
using sqs_processor.Entities;
using sqs_processor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Profiles
{
   public  class PeakRangeDetailsProfile : Profile
    {
        public PeakRangeDetailsProfile()
        {
            CreateMap<PeakRangeDetailDto, PeakRangeDetail>()
               .ForMember(c => c.Id, option => option.Ignore());
        }
    }
}
