using sqs_processor.Entities;
using sqs_processor.Models;
using sqs_processor.ResourceParameters;
using System;
using System.Collections.Generic;
using System.Text;

namespace sqs_processor.Services.repos
{
    public interface IAuctionsRepository
    {
        public List<AuctionSearchWord> GetAuctionSearchWords();

        public List<AuctionSearchSiteRun> GetAuctionSearchSiteRuns();
        public List<AuctionPageLoadCheck> GetAuctionPageLoadChecks();


        public List<AuctionItem> GetAuctionItems(AuctionItemsResourceParameters auctionItemsResourceParameters);
        public List<AuctionCategorySite> GetAuctionCategorySites();
        public void UpsertAuctionSearchSiteRun(AuctionSearchSiteRunDto auctionSearchSiteRun);
        public void UpsertAuctionItems(List<AuctionItemDto> auctionItems);

        public List<AuctionSite> GetAuctionSites();

        public List<AuctionSiteCategoryWord> GetAuctionSiteCategoryWords();
        
        public List<AuctionScriptStep> GetAuctionScriptSteps();
        bool Save();
    }
}
