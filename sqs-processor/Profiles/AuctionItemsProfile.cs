using AutoMapper;
using sqs_processor.Entities;
using sqs_processor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Profiles
{
   public class AuctionItemsProfile : Profile
    {
        public AuctionItemsProfile()
        {
            CreateMap<AuctionItem, AuctionItemDto>();
            CreateMap<AuctionItemDto, AuctionItem>()
                .ForMember(d => d.DateCreated, opt => opt.MapFrom(x => DateTime.UtcNow))
            .ForMember(d => d.DateModified, opt => opt.MapFrom(x => DateTime.UtcNow));
            //.ForMember(d => d.DateCreated, opt => opt.(src >=DateTime.UtcNow))
            //.ForMember(d => d.DateModified, opt => opt.MapFrom(s => s.Item2.Id))
            //        ;
        }

    }
}
