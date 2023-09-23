using AutoMapper;
using sqs_processor.Entities;
using sqs_processor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.ResourceParameters
{
    public class StockSplitHistoriesResourceParameters
    {


        public DateTime? HistoricDateLow { get; set; }

        public DateTime? HistoricDateHigh { get; set; }

    }
}
