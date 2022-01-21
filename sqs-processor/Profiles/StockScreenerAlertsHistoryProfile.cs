using AutoMapper;
using sqs_processor.Entities;
using sqs_processor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Profiles
{
   public class StockScreenerAlertsHistoryProfile : Profile
    {

        public StockScreenerAlertsHistoryProfile()
        {

            CreateMap<StockScreenerAlertsHistory, StockScreenerAlertsHistoryDto>();
            CreateMap<StockScreenerAlertsHistoryDto, StockScreenerAlertsHistory>();
        }
    }
}
