using AutoMapper;
using sqs_processor.DbContexts;
using sqs_processor.Entities;
using sqs_processor.Services.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sqs_processor.Models;
using sqs_processor.ResourceParameters;

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

        public List<AuctionPageLoadCheck> GetAuctionPageLoadChecks()
        {
            return _context.AuctionPageLoadChecks.ToList();
        }


        public List<AuctionSiteCategoryWord> GetAuctionSiteCategoryWords()
        {
            return _context.AuctionSiteCategoryWords.ToList();
        }

        public List<AuctionScriptStep> GetAuctionScriptSteps()
        {
            return _context.AuctionScriptSteps.ToList();
        }
        public List<AuctionSite> GetAuctionSites()
        {
            return _context.AuctionSites.ToList();
        }
        public List<AuctionItem> GetCurrentAuctionItems()
        {
            return _context.AuctionItems.ToList();
        }

        private List<AuctionItem> GetNewAuctionItems(List<AuctionItem> newAuctionItems)
        {

            string[] auctionUrls = newAuctionItems.Select(x => x.Url).ToArray();

            return _context.AuctionItems.Where(x=> auctionUrls.Contains(x.Url)).ToList();
        }
        public void UpsertAuctionItems(List<AuctionItemDto> auctionItems)
        {


            List<AuctionItem> newAuctionItems = _mapper.Map<List<AuctionItem>>(auctionItems).ToList();

            List<AuctionItem> currentRecords = GetNewAuctionItems(newAuctionItems);


            var existingRecords = newAuctionItems
               .Join(currentRecords, x => x.Url, y => y.Url, (newRecs, curRecs) => new { newRecs, curRecs }).Select(x => x.newRecs).ToList();

            var newRecords = newAuctionItems.Except(existingRecords).ToList();

            _utility.AddRecords(newRecords, _context);


            var matchedItems = GetExistingAuctionItems(newAuctionItems, currentRecords);

            var itemsMatch = matchedItems.Join(currentRecords, x => x.Url, y => y.Url, (newRecs, curRecs) => new { newRecs, curRecs })
                .Where(x => x.curRecs.TotalBids == x.newRecs.TotalBids
                && x.curRecs.ItemPrice == x.newRecs.ItemPrice
                && x.curRecs.ImageUrl == x.newRecs.ImageUrl
                && x.curRecs.ItemShipping == x.newRecs.ItemShipping
                && x.curRecs.AuctionEndProcessed == x.newRecs.AuctionEndProcessed
                && x.curRecs.AuctionEndDate == x.newRecs.AuctionEndDate
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
                   AuctionEndDate = x.newRecs.AuctionEndDate != null ? x.newRecs.AuctionEndDate : x.curRecs.AuctionEndDate,
                   TotalBids = x.newRecs.TotalBids,
                   ProductName = x.curRecs.ProductName,
                   ItemPrice = x.newRecs.ItemPrice,
                   AuctionSearchWordId = x.curRecs.AuctionSearchWordId,
                   AuctionSiteId = x.curRecs.AuctionSiteId,
                   DateCreated = x.curRecs.DateCreated,
                   DateModified = x.newRecs.DateModified,
                   ImageUrl = x.newRecs.ImageUrl,
                   ItemShipping = x.newRecs.ItemShipping,
                   AuctionEndProcessed = x.curRecs.AuctionEndProcessed ? x.curRecs.AuctionEndProcessed : x.newRecs.AuctionEndProcessed,
                   BuyNow = x.newRecs.BuyNow

               }

               ).ToList();
        }

        public List<AuctionCategorySite> GetAuctionCategorySites()
        {
            return _context.AuctionCategorySites.ToList();
        }
        public List<AuctionSearchSiteRun> GetAuctionSearchSiteRuns()
        {
            return _context.AuctionSearchSiteRuns.ToList();
        }


        public List<AuctionItem> GetAuctionItems(AuctionItemsResourceParameters auctionItemsResourceParameters)
        {
            if (!auctionItemsResourceParameters.AuctionEndDateRangeMax.HasValue
              &&
              !auctionItemsResourceParameters.AuctionEndDateRangeMin.HasValue &&
              auctionItemsResourceParameters.ProductName == null &&
              auctionItemsResourceParameters.ProductName == string.Empty &&
              !auctionItemsResourceParameters.ItemPriceMin.HasValue &&
              !auctionItemsResourceParameters.ItemPriceMax.HasValue &&
              !auctionItemsResourceParameters.TotalBidsMin.HasValue &&
              !auctionItemsResourceParameters.TotalBidsMax.HasValue &&
              !auctionItemsResourceParameters.AuctionSiteId.HasValue &&
              !auctionItemsResourceParameters.AuctionSearchWordId.HasValue &&

              !auctionItemsResourceParameters.DateModifiedRangeMin.HasValue &&
              !auctionItemsResourceParameters.DateModifiedRangeMax.HasValue &&
              !auctionItemsResourceParameters.DateCreatedRangeMin.HasValue &&
              !auctionItemsResourceParameters.DateCreatedRangeMax.HasValue &&
              !auctionItemsResourceParameters.AuctionEndProcessed.HasValue 




              )
            {
                return _context.AuctionItems.ToList();
            }


            var collection = _context.AuctionItems as IQueryable<AuctionItem>;

            if (auctionItemsResourceParameters.AuctionEndDateRangeMax.HasValue)
            {
                collection = collection.Where(x => x.AuctionEndDate <= auctionItemsResourceParameters.AuctionEndDateRangeMax.Value);
            }
            if (auctionItemsResourceParameters.AuctionEndDateRangeMin.HasValue)
            {
                collection = collection.Where(x => x.AuctionEndDate >= auctionItemsResourceParameters.AuctionEndDateRangeMin.Value);
            }

            if (auctionItemsResourceParameters.ItemPriceMax.HasValue)
            {
                collection = collection.Where(x => x.ItemPrice <= auctionItemsResourceParameters.ItemPriceMax.Value);
            }
            if (auctionItemsResourceParameters.ItemPriceMin.HasValue)
            {
                collection = collection.Where(x => x.ItemPrice >= auctionItemsResourceParameters.ItemPriceMin.Value);
            }

            if (auctionItemsResourceParameters.TotalBidsMax.HasValue)
            {
                collection = collection.Where(x => x.TotalBids <= auctionItemsResourceParameters.TotalBidsMax.Value);
            }
            if (auctionItemsResourceParameters.TotalBidsMin.HasValue)
            {
                collection = collection.Where(x => x.TotalBids >= auctionItemsResourceParameters.TotalBidsMin.Value);
            }

            if (auctionItemsResourceParameters.AuctionSiteId.HasValue)
            {
                collection = collection.Where(x => x.AuctionSiteId == auctionItemsResourceParameters.AuctionSiteId.Value);
            }

            if (auctionItemsResourceParameters.AuctionSearchWordId.HasValue)
            {
                collection = collection.Where(x => x.AuctionSearchWordId == auctionItemsResourceParameters.AuctionSearchWordId.Value);
            }

            if (auctionItemsResourceParameters.AuctionEndProcessed.HasValue)
            {
                collection = collection.Where(x => x.AuctionEndProcessed == auctionItemsResourceParameters.AuctionEndProcessed.Value);
            }
            if (auctionItemsResourceParameters.DateModifiedRangeMin.HasValue)
            {
                collection = collection.Where(x => x.DateModified >= auctionItemsResourceParameters.DateModifiedRangeMin.Value);
            }
            

            List<AuctionItem> auctionItems = collection.ToList();

            if (auctionItemsResourceParameters.ProductName != null && auctionItemsResourceParameters.ProductName != string.Empty)
            {
                string[] productNames = auctionItemsResourceParameters.ProductName.Split(" ");
                auctionItems = auctionItems.Where(x => productNames.Any(a => x.ProductName.Contains(a))).ToList();
                //collection = collection.Where(x => x.ProductName.Contains(auctionItemsResourceParameters.ProductName));
            }

            return auctionItems.ToList();
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
