using AutoMapper;
using sqs_processor.Entities;
using sqs_processor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Profiles
{
    class EarningsProfile : Profile
    {

        public EarningsProfile()
        {


            CreateMap<Tuple<Earning, Security>, EarningSecurityDto>()
                 .ForMember(d => d.security, opt => opt.MapFrom(s => s.Item2))
       .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Item2.Id))
       .ForMember(d => d.SecurityId, opt => opt.MapFrom(s => s.Item1.SecurityId))
       .ForMember(d => d.symbol, opt => opt.MapFrom(s => s.Item2.Symbol))
       .ForMember(d => d.ActualEarningsDate, opt => opt.MapFrom(s => s.Item1.ActualEarningsDate))
       .ForMember(d => d.EPSEstimate, opt => opt.MapFrom(s => s.Item1.EPSEstimate))
       .ForMember(d => d.ReportedEPS, opt => opt.MapFrom(s => s.Item1.ReportedEPS))
       .ForMember(d => d.GAAPEPS, opt => opt.MapFrom(s => s.Item1.GAAPEPS))

       .ForMember(d => d.RevenueEstimate, opt => opt.MapFrom(s => s.Item1.RevenueEstimate))
       .ForMember(d => d.ActualRevenue, opt => opt.MapFrom(s => s.Item1.ActualRevenue))

       .ForMember(d => d.ReportTime, opt => opt.MapFrom(s => s.Item1.ReportTime))
       ;




            // CreateMap<Earning, dynamic>();
            /*
            CreateMap<Earning, PreferredEarningDto>();

            CreateMap<Tuple<Earning, Security>, PreferredEarningDto>()
    .ForMember(d => d.Symbol, opt => opt.MapFrom(s => s.Item2.Symbol))
    .ForMember(d => d.ActualEarningsDate, opt => opt.MapFrom(s => s.Item1.ActualEarningsDate))
    .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Item2.Name))
    .ForMember(d => d.EPSEstimate, opt => opt.MapFrom(s => s.Item1.EPSEstimate))

    .ForMember(d => d.GAAPEPS, opt => opt.MapFrom(s => s.Item1.GAAPEPS))
    .ForMember(d => d.ReportedEPS, opt => opt.MapFrom(s => s.Item1.ReportedEPS))
    .ForMember(d => d.StockId, opt => opt.MapFrom(s => s.Item1.StockId))
    .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Item1.Id))
    .ForMember(d => d.FiscalQuaterEnd, opt => opt.MapFrom(s => s.Item1.FiscalQuaterEnd))


    ;
            */

            CreateMap<EarningDto, Earning>();
            CreateMap<Earning, EarningDto>();
        }
    }
}
