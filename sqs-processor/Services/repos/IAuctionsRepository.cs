using sqs_processor.Entities;
using sqs_processor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Services.repos
{
    public interface IAuctionsRepository
    {
        public List<AuctionSearchWord> GetAuctionSearchWords();

        public List<AuctionSearchSiteRun> GetAuctionSearchSiteRuns();
        public void UpsertAuctionSearchSiteRun(AuctionSearchSiteRunDto auctionSearchSiteRun);
        public void UpsertAuctionItems(List<AuctionItemDto> auctionItems);

        public List<AuctionSite> GetAuctionSites();
        bool Save();
    }
}
