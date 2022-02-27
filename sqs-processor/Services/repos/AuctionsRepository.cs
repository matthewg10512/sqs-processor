using AutoMapper;
using sqs_processor.DbContexts;
using sqs_processor.Entities;
using sqs_processor.Services.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sqs_processor.Models;

namespace sqs_processor.Services.repos
{
    public class AuctionsRepository : IAuctionsRepository
    {
        private readonly SecuritiesLibraryContext _context;
        private readonly IMapper _mapper;
        private readonly IUtility _utility;
        public AuctionsRepository(SecuritiesLibraryContext context,
            //IConfiguration config,
            IMapper mapper, IUtility utility)
        {

            _context = context ?? throw new ArgumentNullException(nameof(context));
            // _config = config ?? throw new ArgumentNullException(nameof(config));
            _mapper = mapper;
            _utility = utility;
        }

        public bool Save()
        {

            return (_context.SaveChanges() >= 0);
        }


        public List<AuctionSearchWord> GetAuctionSearchWords()
        {
            return _context.AuctionSearchWords.ToList();
        }

        public List<AuctionSite> GetAuctionSites()
        {
            return _context.AuctionSites.ToList();
        }
        public List<AuctionItem> GetCurrentAuctionItems()
        {
            return _context.AuctionItems.ToList();
        }
        public void UpsertAuctionItems(List<AuctionItemDto> auctionItems)
        {


            List<AuctionItem> newAuctionItems = _mapper.Map<List<AuctionItem>>(auctionItems).ToList();

            List<AuctionItem> currentRecords = GetCurrentAuctionItems();


            var existingRecords = newAuctionItems
               .Join(currentRecords, x => x.Url, y => y.Url, (newRecs, curRecs) => new { newRecs, curRecs }).Select(x => x.newRecs).ToList();

            var newRecords = newAuctionItems.Except(existingRecords).ToList();

            _utility.AddRecords(newRecords, _context);


            var matchedItems = GetExistingAuctionItems(newAuctionItems, currentRecords);

            var itemsMatch = matchedItems.Join(currentRecords, x => x.Url, y => y.Url, (newRecs, curRecs) => new { newRecs, curRecs })
                .Where(x => x.curRecs.TotalBids == x.newRecs.TotalBids
                && x.curRecs.ItemPrice == x.newRecs.ItemPrice
                && x.curRecs.ImageUrl == x.newRecs.ImageUrl
                )

                .Select(x => x.newRecs).ToList();
            var recordsToUpdate = matchedItems.Except(itemsMatch).ToList();

            _utility.UpdateRecords(recordsToUpdate, _context);

        }
        public List<AuctionItem> GetExistingAuctionItems(List<AuctionItem> newAuctionItems, List<AuctionItem> currentRecords)
        {
            return newAuctionItems
               .Join(currentRecords, x => x.Url, y => y.Url, (newRecs, curRecs) => new { newRecs, curRecs }).Select(x =>

               new AuctionItem
               {
                   Id = x.curRecs.Id,
                   Url = x.curRecs.Url,
                   AuctionEndDate = x.curRecs.AuctionEndDate,
                   TotalBids = x.newRecs.TotalBids,
                   ProductName = x.curRecs.ProductName,
                   ItemPrice = x.newRecs.ItemPrice,
                   AuctionSearchWordId = x.curRecs.AuctionSearchWordId,
                   AuctionSiteId = x.curRecs.AuctionSiteId,
                   DateCreated = x.curRecs.DateCreated,
                   DateModified = x.newRecs.DateModified,
                   ImageUrl = x.newRecs.ImageUrl
               }

               ).ToList();
        }

        public List<AuctionSearchSiteRun> GetAuctionSearchSiteRuns()
        {
            return _context.AuctionSearchSiteRuns.ToList();
        }

        public void UpsertAuctionSearchSiteRun(AuctionSearchSiteRunDto auctionSearchSiteRun)
        {
            AuctionSearchSiteRun newAuctionSearchSiteRun = _mapper.Map<AuctionSearchSiteRun>(auctionSearchSiteRun);
            newAuctionSearchSiteRun.DateSearch = DateTime.UtcNow;
            var auctionSearchSiteRunRec = _context.AuctionSearchSiteRuns.Where(x => x.AuctionSiteId == newAuctionSearchSiteRun.AuctionSiteId &&
             x.AuctionSearchWordId == newAuctionSearchSiteRun.AuctionSearchWordId).FirstOrDefault();

            if (auctionSearchSiteRunRec == null)
            {
                _context.AuctionSearchSiteRuns.Add(newAuctionSearchSiteRun);
            }
            else
            {
                auctionSearchSiteRunRec.DateSearch = DateTime.UtcNow;
                _context.Update(auctionSearchSiteRunRec);
            }

            Save();
        }
    }
}
