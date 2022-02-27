using AutoMapper;
using sqs_processor.Entities;
using sqs_processor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Profiles
{
    class AuctionSearchSiteRunsProfile : Profile
    {
        public AuctionSearchSiteRunsProfile()
        {
            CreateMap<AuctionSearchSiteRun, AuctionSearchSiteRunDto>();
            CreateMap<AuctionSearchSiteRunDto, AuctionSearchSiteRun>();
        }
    }
}