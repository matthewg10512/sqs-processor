using AutoMapper;
using sqs_processor.Entities;
using sqs_processor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Profiles
{
    public class DividendsProfile : Profile
    {
        public DividendsProfile()
        {
            CreateMap<Tuple<Dividend, Security>, DividendSecurityDto>()
                  .ForMember(d => d.security, opt => opt.MapFrom(s => s.Item2))
        .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Item2.Id))
        .ForMember(d => d.SecurityId, opt => opt.MapFrom(s => s.Item1.SecurityId))
        .ForMember(d => d.symbol, opt => opt.MapFrom(s => s.Item2.Symbol))
        .ForMember(d => d.AnnouncementDate, opt => opt.MapFrom(s => s.Item1.AnnouncementDate))
        .ForMember(d => d.Frequency, opt => opt.MapFrom(s => s.Item1.Frequency))
        .ForMember(d => d.Amount, opt => opt.MapFrom(s => s.Item1.Amount))
        .ForMember(d => d.Yield, opt => opt.MapFrom(s => s.Item1.Yield))

        .ForMember(d => d.ExDividendDate, opt => opt.MapFrom(s => s.Item1.ExDividendDate))
        .ForMember(d => d.RecordDate, opt => opt.MapFrom(s => s.Item1.RecordDate))

        .ForMember(d => d.PayableDate, opt => opt.MapFrom(s => s.Item1.PayableDate))
        ;


            /*
             *         public int Id { get; set; }
            public int SecurityId { get; set; }
            public string symbol { get; set; }
            public DateTime AnnouncementDate { get; set; }
            public string Frequency { get; set; }
            [Column(TypeName = "decimal(18,2)")]
            public decimal Amount { get; set; }
            [Column(TypeName = "decimal(18,2)")]
            public decimal Yield { get; set; }
            public DateTime ExDividendDate { get; set; }
            public DateTime RecordDate { get; set; }
            public DateTime PayableDate { get; set; }
            public SecurityDto security { get; set; }
            */


            CreateMap<Dividend, DividendDto>();
            CreateMap<DividendDto, Dividend>()
                .ForMember(c => c.Id, option => option.Ignore());
        }
    }
}
