﻿using AutoMapper;
using sqs_processor.Entities;
using sqs_processor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Profiles
{
   public  class BullRunsProfile : Profile
    {
        public BullRunsProfile()
        {
            CreateMap<BullBearRun, BullBearRunDto>()
                     .ForMember(c => c.Id, option => option.Ignore());
            CreateMap<BullBearRunDto, BullBearRun>()
                     .ForMember(c => c.Id, option => option.Ignore());
        }
    }
}
