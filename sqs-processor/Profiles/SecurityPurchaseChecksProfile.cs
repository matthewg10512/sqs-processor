using AutoMapper;
using sqs_processor.Entities;
using sqs_processor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Profiles
{
   public class SecurityPurchaseChecksProfile : Profile
    {
        public SecurityPurchaseChecksProfile()
        {
            CreateMap<SecurityPurchaseCheckDto, SecurityPurchaseCheck>();
            CreateMap<SecurityPurchaseCheck, SecurityPurchaseCheckDto>();
            
        }
    }
}
