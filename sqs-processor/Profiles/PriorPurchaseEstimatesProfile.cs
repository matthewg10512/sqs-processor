using AutoMapper;
using sqs_processor.Entities;
using sqs_processor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Profiles
{
   public class PriorPurchaseEstimatesProfile : Profile
    {
        public PriorPurchaseEstimatesProfile()
        {
            CreateMap<PriorPurchaseEstimateDto, PriorPurchaseEstimate>();
            CreateMap<PriorPurchaseEstimate, PriorPurchaseEstimateDto>();
            
        }
    }
}
