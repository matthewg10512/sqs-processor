using HtmlAgilityPack;
using PuppeteerSharp;
using PuppeteerSharp.Input;
using sqs_processor.Entities;
using sqs_processor.Models;
using sqs_processor.Services.Factories;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace sqs_processor.Processes
{
    public enum  ScreenerType
    {
        PuppeteerURL =1,
        PuppeteerJavaScript =2,
        WebClient =3
    }


    public class ProcessAuctionSiteDataRetrieval : IProcess
    {
        private static bool processingRecords;
        private readonly IUnitOfWork _unitOfWork;
        public ProcessAuctionSiteDataRetrieval(IServiceFactory serviceFactory)
        {
            _unitOfWork = serviceFactory.GetUnitOfWorkFactoryService().GetUnitOfWork();
        }

        public void RunTask()
        {

            processingRecords = true;
            Task detail = GetAuctionItems(_unitOfWork);

            while (processingRecords)
            {

            }




        }
        public static string AssignCategoryId(AuctionSearchWord auctionSearchWord, List<AuctionSiteCategoryWord> auctionSiteCategoryWords, List<AuctionCategorySite> auctionCategorySites)
        {
            string categoryId = "";
            var categoryWords = auctionSiteCategoryWords.Where(x => x.AuctionSearchWordId == auctionSearchWord.Id).FirstOrDefault();
            if (categoryWords != null)
            {
                var categorySite = auctionCategorySites.Where(x => x.Id == categoryWords.AuctionCategoryId).FirstOrDefault();
                if (categorySite != null)
                {
                    categoryId = categorySite.SiteCategoryId.ToString();
                }
            }
            return categoryId;
        }

        public static async Task ProcessScriptStep(AuctionScriptStep searchScriptType, Page page, AuctionSearchWord auctionSearchWord, string categoryId)
        {
            if (searchScriptType.ScriptAction == 1)
            {
                var jsCodeSearch = searchScriptType.JsCode
                    .Replace("searchCallReplace", auctionSearchWord.SearchWord);

                if (jsCodeSearch.Contains("categoryIdReplace") && categoryId == "")
                {
                    return;
                }
                jsCodeSearch = jsCodeSearch.Replace("categoryIdReplace", categoryId);


                await page.EvaluateFunctionAsync(jsCodeSearch);
            }
            else
            {
                await page.ClickAsync(searchScriptType.JsCode, new ClickOptions() { Button = MouseButton.Left });
            }
        }

        public static async Task GetAuctionItems(IUnitOfWork _unitOfWork)
        {
            List<AuctionSearchWord> auctionSearchWords = _unitOfWork.auctionRepository.GetAuctionSearchWords();//search words to go through
            List<AuctionSite> auctionSites = _unitOfWork.auctionRepository.GetAuctionSites(); //list of auction sites
            List<AuctionSearchSiteRun> auctionSearchSiteRuns = _unitOfWork.auctionRepository.GetAuctionSearchSiteRuns();//list of auction sites and search words when they were run
            List<AuctionScriptStep> auctionScriptSteps = _unitOfWork.auctionRepository.GetAuctionScriptSteps(); //list of script steps tied to the 

            List<AuctionCategorySite> auctionCategorySites = _unitOfWork.auctionRepository.GetAuctionCategorySites();
            List<AuctionSiteCategoryWord> auctionSiteCategoryWords = _unitOfWork.auctionRepository.GetAuctionSiteCategoryWords();//sites and categories tied to the search words

            List<AuctionPageLoadCheck> auctionPageLoadChecks = _unitOfWork.auctionRepository.GetAuctionPageLoadChecks();




            /*
            var client = new WebClient();
            var content = client.DownloadString("https://www.shopthesalvationarmy.com/Browse?FullTextQuery=sea&page=0");

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(content);
            HtmlNodeCollection nodes = doc.DocumentNode.ChildNodes;
            */




            string detail = "";
            /*
            foreach (HtmlNode row in nodes)
            {
                var textinfo = row.Name;
                HtmlNodeCollection nodesinfo = row.ChildNodes;

                foreach (HtmlNode nodeinfo in nodesinfo)
                {
                     textinfo = nodeinfo.Name;
                }


                }
            */

            string details = "";// ContextStaticAttribute;



            // return;

            int counterBreakMax = 40000;
            int counterBreak = 0;
            foreach (var auctionSite in auctionSites)
            {
                if (counterBreak > counterBreakMax)
                {
                    break;
                }
                var auctionSitePageLoadChecks = auctionPageLoadChecks.Where(x => x.AuctionSiteId == auctionSite.Id).ToList();
                var siteAuctionScriptSteps = auctionScriptSteps.Where(x => x.AuctionSiteId == auctionSite.Id);

                if (auctionSite.Id != 3)
                {
                    //continue;
                }
                foreach (var auctionSearchWord in auctionSearchWords)
                {
                    if (counterBreak > counterBreakMax)
                    {
                        break;
                    }
                    string categoryId = AssignCategoryId(auctionSearchWord, auctionSiteCategoryWords, auctionCategorySites);

                    if (auctionSearchWord.SearchWord != "sea")
                    {
                        // continue;
                    }
                    DateTime currentDate = DateTime.UtcNow.AddDays(-2);
                    if (auctionSearchSiteRuns.Where(x => x.AuctionSiteId == auctionSite.Id && x.AuctionSearchWordId == auctionSearchWord.Id && x.DateSearch > currentDate).Count() > 0)
                    {
                        continue;
                    }





                    try
                    {
                        List<SiteAuctionItemDto> totalSearchresults = new List<SiteAuctionItemDto>();
                        switch (auctionSite.ScrapingType)
                        {

                            case (int)ScreenerType.WebClient:
                                totalSearchresults = ProcessWebClient(auctionSite, auctionSearchWord);

                                break;

                            case (int)ScreenerType.PuppeteerURL:
                            case (int)ScreenerType.PuppeteerJavaScript:
                                {
                                    var options = new LaunchOptions { Headless = true };
                                    Console.WriteLine("Downloading chromium");
                                    await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
                                    using (var browser = await Puppeteer.LaunchAsync(options))
                                    using (var page = await browser.NewPageAsync())
                                    {
                                        bool processNextPage = true;
                                        int pageRecords = 0;
                                        while (processNextPage)
                                        {
                                            Thread.Sleep(20000);
                                            //alc
                                            string url = auctionSite.SearchURL.Replace(auctionSite.SearchWordReplace, auctionSearchWord.SearchWord)
                                                .Replace(auctionSite.PageReplace, (auctionSite.StartPaging + pageRecords).ToString());

                                            if (categoryId != null && categoryId != "")
                                            {
                                                url = url.Replace("categoryRecReplace", categoryId);
                                            }
                                            else
                                            {
                                                url = url.Replace("categoryRecReplace", "");
                                            }
                                            Console.WriteLine("Navigating to " + url);
                                            // await page.GoToAsync(url);

                                            try
                                            {
                                                if (pageRecords == 0)
                                                {
                                                    switch (auctionSite.ScrapingType)
                                                    {
                                                        case (int)ScreenerType.PuppeteerURL:
                                                            await page.GoToAsync(url);
                                                            // Thread.Sleep(10000);
                                                            break;
                                                        case (int)ScreenerType.PuppeteerJavaScript:
                                                            await page.GoToAsync(url);
                                                            Thread.Sleep(8000);
                                                            var searchScriptTypes = siteAuctionScriptSteps.Where(x => x.ActionGroupType == 1);//search
                                                            searchScriptTypes = searchScriptTypes.OrderBy(x => x.StepOrder).ToList();
                                                            foreach (var searchScriptType in searchScriptTypes)
                                                            {
                                                                await ProcessScriptStep(searchScriptType, page, auctionSearchWord, categoryId);
                                                                Thread.Sleep(searchScriptType.ThreadSleep);
                                                            }

                                                            break;
                                                    }
                                                }
                                                else
                                                {//Next Page
                                                    switch (auctionSite.ScrapingType)
                                                    {
                                                        case (int)ScreenerType.PuppeteerURL:
                                                            await page.GoToAsync(url);
                                                            // Thread.Sleep(10000);
                                                            break;

                                                        case (int)ScreenerType.PuppeteerJavaScript:
                                                            //await page.ClickAsync("[class='pagination__next icon-link']", new ClickOptions() { Button = MouseButton.Left });


                                                            var searchScriptTypes = siteAuctionScriptSteps.Where(x => x.ActionGroupType == 2);//search
                                                            searchScriptTypes = searchScriptTypes.OrderBy(x => x.StepOrder).ToList();
                                                            foreach (var searchScriptType in searchScriptTypes)
                                                            {
                                                                await ProcessScriptStep(searchScriptType, page, auctionSearchWord, categoryId);
                                                                Thread.Sleep(searchScriptType.ThreadSleep);
                                                            }
                                                            break;
                                                    }

                                                }
                                                if (auctionSitePageLoadChecks.Count > 0)
                                                {
                                                    auctionSitePageLoadChecks = auctionSitePageLoadChecks.OrderBy(x => x.WordGrouping).ToList();
                                                    int countCheck = 0;
                                                    string innerHTMLVal = await page.EvaluateFunctionAsync<string>("() => {return document.documentElement.innerHTML;} ");
                                                    bool pageHasLoaded = false;
                                                    while ((!pageHasLoaded
                                                           )
                                                           && countCheck < 5)
                                                    {
                                                        bool currentGroupingPageLoaded = CheckCurrentGroupingPageLoaded(auctionSitePageLoadChecks, innerHTMLVal);
                                                        if (currentGroupingPageLoaded)
                                                        {
                                                            pageHasLoaded = true;
                                                        }
                                                        Thread.Sleep(2000);
                                                        innerHTMLVal = await page.EvaluateFunctionAsync<string>("() => {return document.documentElement.innerHTML;} ");
                                                        countCheck += 1;
                                                    }
                                                    if (countCheck >= 5) { countCheck += 1; }


                                                }
                                                else
                                                {
                                                    Thread.Sleep(10000);
                                                }


                                            }
                                            catch(Exception ex) {     }


                                            try
                                            {
                                                

                                                SiteAuctionItemDto[] siteResultsCurrentPull = await page.EvaluateFunctionAsync<SiteAuctionItemDto[]>(auctionSite.JsCode);
                                                string hrefValue = await page.EvaluateFunctionAsync<string>("() => {return window.location.href;} ");

                                                Console.WriteLine(hrefValue);
                                                if (siteResultsCurrentPull == null || siteResultsCurrentPull.Length < 40 || totalSearchresults.Count > 500)
                                                {
                                                    processNextPage = false;
                                                }
                                                pageRecords += 1;
                                                //BOT_MAX_PAGE_NUMBER_EXCEEDED

                                                //string CheckinnerHTML = await page.EvaluateFunctionAsync<string>("() => {return document.documentElement.innerHTML;} ");
                                                var missingRecords = FindMissingRecords(totalSearchresults, siteResultsCurrentPull);

                                                if (missingRecords.Count == 0)
                                                {
                                                    processNextPage = false;
                                                }
                                                else
                                                {

                                                    totalSearchresults.AddRange(missingRecords);
                                                }

                                            }
                                            catch (Exception ex)
                                            {
                                                processNextPage = false;
                                                string error = ex.Message;
                                            }

                                        }




                                    }
                                }
                                break;

                        }

                        counterBreak += totalSearchresults.Count;

                        ProcessAuctionSiteItems(totalSearchresults, auctionSite, auctionSearchWord, _unitOfWork);

                        UpdateAuctionSearchSite(auctionSearchWord, auctionSite, _unitOfWork);


                    }
                    catch (Exception ex)
                    {
                        string testError = auctionSearchWord.SearchWord + " " + ex.Message;
                    }
                }
            }

            processingRecords = false;

        }

        private static List<SiteAuctionItemDto> ProcessWebClient(AuctionSite auctionSite, AuctionSearchWord auctionSearchWord){

            List<SiteAuctionItemDto> totalSearchresults = new List<SiteAuctionItemDto>();
            
            
            bool processNextPage = true;
            int pageRecords = 0;
            while (processNextPage)
            {
                Thread.Sleep(20000);
                List<SiteAuctionItemDto> siteResultsCurrentPull = new List<SiteAuctionItemDto>();
                string url = auctionSite.SearchURL.Replace("searchCallReplace", auctionSearchWord.SearchWord)
                    .Replace("pageRecordsReplace", (auctionSite.StartPaging + pageRecords).ToString());
                HtmlWeb web = new HtmlWeb();
                HtmlDocument document = web.Load(url);
                HtmlNode[] links= new HtmlNode[0];
                if (document.DocumentNode.SelectNodes("//section") != null && document.DocumentNode.SelectNodes("//section").Count > 0)
                {
                    links = document.DocumentNode.SelectNodes("//section").ToArray();
                }
                else
                {
                    return totalSearchresults;
                }

                foreach (var link in links)
                {
                    SiteAuctionItemDto siteAuction = new SiteAuctionItemDto();
                    HtmlNode[] datas = link.SelectNodes(".//div[contains(@class, 'row')]").ToArray();
                    HtmlNode[] h1Title = link.SelectNodes(".//h1[contains(@class, 'title')]").ToArray();

                    if (h1Title.Length > 0)
                    {
                        //InnerText = "\r\n                    \r\n                    Huge Lot Of 106 Brand New DVD Movies/Shows                    \r\n                "
                        siteAuction.ProductName = h1Title[0].InnerText.Replace("                    ", "").Replace(Environment.NewLine, "").Trim();

                        HtmlNode[] productLink = h1Title[0].SelectNodes(".//a").ToArray();
                        if (productLink.Length > 0)
                        {
                            var productURl = productLink[0].GetAttributes().Where(x => x.Name == "href").FirstOrDefault();
                            if (productURl != null)
                            {
                                siteAuction.ItemUrl = auctionSite.PrependProductSiteUrl + productURl.Value;
                            }
                        }
                    }

                    HtmlNode[] spanValues = link.SelectNodes(".//span").ToArray();
                    //status-type


                    foreach (var spanValue in spanValues)
                    {
                        var dataActionTime = spanValue.GetAttributes().Where(x => x.Name == "data-action-time").FirstOrDefault();

                        if (dataActionTime != null)
                        {
                            //string dateEnd
                            siteAuction.TimeLeft = dataActionTime.Value;
                        }

                        var classInfo = string.Join(" ", spanValue.GetClasses().ToArray());
                        if (classInfo == "awe-rt-HideOnEnd awe-rt-AcceptedListingActionCount")
                        {
                            siteAuction.TotalBids = spanValue.InnerText;
                        }
                        if (classInfo == "NumberPart")
                        {
                            siteAuction.ItemPrice = spanValue.InnerText;
                        }
                        if (classInfo == "label label-primary status-type")
                        {
                            if (spanValue.InnerText == "Fixed Price")
                            {
                                siteAuction.BuyNow = "buynow";
                            }
                        }

                        //awe-rt-HideOnEnd awe-rt-AcceptedListingActionCount
                    }

                    foreach (var data in datas)
                    {
                        HtmlNode[] imgDetail = data.SelectNodes(".//img").ToArray();
                        siteAuction.ImageUrl = imgDetail[0].Attributes["src"].Value;
                        HtmlNode[] nodeInfos = data.ChildNodes.ToArray();
                    }
                    siteResultsCurrentPull.Add(siteAuction);
                }

                if (siteResultsCurrentPull == null || siteResultsCurrentPull.Count < 40 || totalSearchresults.Count > 500)
                {
                    processNextPage = false;
                }
                pageRecords += 1;
                //BOT_MAX_PAGE_NUMBER_EXCEEDED

                //string CheckinnerHTML = await page.EvaluateFunctionAsync<string>("() => {return document.documentElement.innerHTML;} ");
                var missingRecords = FindMissingRecords(totalSearchresults, siteResultsCurrentPull.ToArray());

                if (missingRecords.Count == 0)
                {
                    processNextPage = false;
                }
                else
                {

                    totalSearchresults.AddRange(missingRecords);
                }


            }
            return totalSearchresults;
        }
        private static bool CheckCurrentGroupingPageLoaded(List<AuctionPageLoadCheck> auctionSitePageLoadChecks,string innerHTMLVal)
        {
            bool currentGroupingPageLoaded = false;
            int currentGrouping = -1;
            foreach (var auctionSitePageLoadCheck in auctionSitePageLoadChecks)
            {
                if (auctionSitePageLoadCheck.WordGrouping != currentGrouping)
                {
                    currentGrouping = auctionSitePageLoadCheck.WordGrouping;
                    if (currentGroupingPageLoaded)
                    {
                        currentGroupingPageLoaded = true;
                        break;
                    }
                    currentGroupingPageLoaded = innerHTMLVal.Contains(auctionSitePageLoadCheck.WordCheck);
                }
                else
                {
                    currentGroupingPageLoaded = currentGroupingPageLoaded && innerHTMLVal.Contains(auctionSitePageLoadCheck.WordCheck);
                }
            }
  
            return currentGroupingPageLoaded;

        }


        private static void UpdateAuctionSearchSite(AuctionSearchWord auctionSearchWord,AuctionSite auctionSite, IUnitOfWork _unitOfWork)
        {
            AuctionSearchSiteRunDto searchSiteRun = new AuctionSearchSiteRunDto();
            searchSiteRun.AuctionSearchWordId = auctionSearchWord.Id;
            searchSiteRun.AuctionSiteId = auctionSite.Id;
            _unitOfWork.auctionRepository.UpsertAuctionSearchSiteRun(searchSiteRun);
        }

        private static List<SiteAuctionItemDto>  FindMissingRecords(List<SiteAuctionItemDto> totalSearchresults, SiteAuctionItemDto[] siteResultsCurrentPull)
        {

            var matchingRecords = totalSearchresults.Join(siteResultsCurrentPull, x => x.ItemUrl, y => y.ItemUrl, (newRecs, curRecs) => new { newRecs, curRecs }).Select(x => x.curRecs);
            var matchingRecordCount = matchingRecords.Count();
            var currentMissingRecords = siteResultsCurrentPull.Except(matchingRecords).ToList();
            var otherMatchingRecords = totalSearchresults.Join(currentMissingRecords, x => x.ImageUrl, y => y.ImageUrl, (newRecs, curRecs) => new { newRecs, curRecs }).Select(x => x.curRecs);
            var otherMatchingRecordCount = otherMatchingRecords.Count();
            
            var missingRecords = currentMissingRecords.Except(otherMatchingRecords).ToList();
            return missingRecords;
        }
        private static void ProcessAuctionSiteItems(List<SiteAuctionItemDto> totalSearchresults, AuctionSite auctionSite, AuctionSearchWord auctionSearchWord, IUnitOfWork _unitOfWork)
        {
            List<AuctionItemDto> auctionItems = new List<AuctionItemDto>();
            if (totalSearchresults.Count > 0)
            {

                foreach (var totalresult in totalSearchresults)
                {
                    try
                    {
                        AuctionItemDto auctionItem = new AuctionItemDto();
                        auctionItem.AuctionSiteId = auctionSite.Id;
                        auctionItem.AuctionSearchWordId = auctionSearchWord.Id;//to do change later to but the auction search id

                        auctionItem.ImageUrl = totalresult.ImageUrl;
                        decimal output;
                        if (totalresult.ItemPrice == null || totalresult.ItemPrice == "")
                        {
                            totalresult.ItemPrice = "0";
                        }
                        decimal.TryParse(totalresult.ItemPrice.Replace("$", ""), out output);
                        auctionItem.ItemPrice = output;


                        if(totalresult.ItemShipping != null && totalresult.ItemShipping != "")
                        {
                            decimal.TryParse(totalresult.ItemShipping.Replace("$", ""), out output);
                            auctionItem.ItemShipping = output;
                        }
                        else
                        {

                        }
                        if(totalresult.BuyNow != null && totalresult.BuyNow == "buynow")
                        {
                           auctionItem.BuyNow = true;
                        }
                        else { auctionItem.BuyNow = false; }


                            auctionItem.Url = totalresult.ItemUrl;
                        int totalBidsOutPut;
                        if (totalresult.TotalBids == null || totalresult.TotalBids == "")
                        {
                            totalresult.TotalBids = "0";
                        }
                        Int32.TryParse(totalresult.TotalBids.Replace("Bids: ", ""), out totalBidsOutPut);

                        auctionItem.TotalBids = totalBidsOutPut;

                        auctionItem.ProductName = totalresult.ProductName.Replace("{","").Replace("}","");
                        //totalresult.TimeLeft

                        DateTime ? currendate = GetAuctionDate(totalresult.TimeLeft);

                        auctionItem.AuctionEndDate = currendate;
                        auctionItem.AuctionEndProcessed = false;
                        auctionItems.Add(auctionItem);
                    }
                    catch (Exception ex)
                    {
                        string excep = ex.Message;
                    }
                }


                int partitionLoop = 0;
                int partitionInterval = 100;

                while (auctionItems.Count >= partitionLoop)
                {
                    _unitOfWork.auctionRepository.UpsertAuctionItems(auctionItems.Skip(partitionLoop).Take(partitionInterval).ToList());
                    partitionLoop += partitionInterval;
                }


            }

        }
        private static DateTime? GetAuctionDate(string timeLeft)
        {

            DateTime currendate = DateTime.UtcNow;


            if (timeLeft == null || timeLeft == "")
            {
                return null;
            }


            DateTime dateResult;
            bool isDate = DateTime.TryParse(timeLeft, out dateResult);
            if (isDate)
            {
                return dateResult;
            }


            if (timeLeft.Contains("d"))//add days
            {
                string[] days = timeLeft.Split('d');
                currendate = currendate.AddDays(Int32.Parse(days[0]));
            }

            if (timeLeft.Contains("h"))//add hours
            {
                string[] hours = timeLeft.Split('h');
                string hour = hours[0].Length == 1 ? hours[0] : hours[0].Substring(hours[0].Length - 2, 2);

                currendate = currendate.AddHours(Int32.Parse(hour));
            }

            if (timeLeft.Contains("m"))//add hours
            {
                string[] minutes = timeLeft.Split('m');
                string minute = minutes[0].Length == 1 ? minutes[0] : minutes[0].Substring(minutes[0].Length - 2, 2);

                currendate = currendate.AddMinutes(Int32.Parse(minute));
            }
            return currendate;
        }













    }
}



/*
                                if (pageRecords == 1 && auctionSite.Id == 2)
                                {
                                    /*
                                    await page.EvaluateFunctionAsync("() => {var searchBar = document.getElementById('gh-ac'); searchBar.value = '" + auctionSearchWord.SearchWord + "';}");
                                    Thread.Sleep(1000);

                                    await page.ClickAsync("[id='gh-btn']", new ClickOptions() { Button = MouseButton.Left });

                                    //await page.EvaluateFunctionAsync("() => {var btnSubmit = document.getElementById('gh-btn');btnSubmit.click();}");
                                    Thread.Sleep(10000);

                                    //var titles = document.getElementsByClassName('srp-refine__category__item'); var ainfo = titles[0].getElementsByTagName('a');ainfo[0].click();
                                    //ainfo[0].innerText
                                    //ainfo[0].href
                                    //document.getElementById('gh-cat').value = '11450'
                                    await page.EvaluateFunctionAsync("() => {var titles = document.getElementsByClassName('srp-refine__category__item'); var ainfo = titles[0].getElementsByTagName('a');ainfo[0].click();}");

                                    //await page.EvaluateFunctionAsync("() => {window.location.href = 'https://www.ebay.com/sch/281/i.html?_from=R40&amp;_nkw=acne&amp;_oac=1';}");
                                    
                                    
                                    Thread.Sleep(10000);
                                    * /
                                }
                                else
                                {

                                }
                                */


/*
string innerHTMLVal = await page.EvaluateFunctionAsync<string>("() => {return document.documentElement.innerHTML;} ");
if((!(innerHTMLVal.Contains("Showing") && innerHTMLVal.Contains("Showing"))) &&
    !innerHTMLVal.Contains("Oops! We couldn’t find that product but you might be interested in something below"))
{
    int countCheck = 0;
    while(( (!(innerHTMLVal.Contains("Showing") && innerHTMLVal.Contains("Showing"))) 
        &&
        !innerHTMLVal.Contains("Oops! We couldn’t find that product but you might be interested in something below")
        )
        && countCheck < 5)
    {
        Thread.Sleep(2000);
        innerHTMLVal = await page.EvaluateFunctionAsync<string>("() => {return document.documentElement.innerHTML;} ");
        countCheck += 1;
    }
}
*/


/*
                                    bool captchaRequired = await page.EvaluateFunctionAsync<bool>(captchaCheck);
                                    if (captchaRequired)
                                    {
                                        Thread.Sleep(30000);

                                    }
                                    */


/*

                                if (pageRecords == 1 || auctionSite.Id == 1)
                                {
                                    
                                    //await page.GoToAsync("https://www.ebay.com");
                                }
                                else
                                {
                                    try
                                    {
                                        //await page.ClickAsync("[class='pagination__item icon-link'][[class='pagination__item icon-link'].length]", new ClickOptions() { Button = MouseButton.Left });

                                        //"var aItems = document.getElementsByClassName('pagination__item'); var itemsLength = aItems.length; for (var iItem = 0; iItem < itemsLength; iItem++) {if(aItems[iItem].innerText = '" + pageRecords.ToString() + "') {aItems[iItem].click();}  }"

                                        //await page.EvaluateFunctionAsync("() => {var aItems = document.getElementsByClassName('pagination__item'); var itemsLength = aItems.length; for (var iItem = 0; iItem < itemsLength; iItem++) {if(aItems[iItem].innerText = '" + pageRecords.ToString() + "') {aItems[iItem].click();}  }}");

                                        
                                    }
                                    catch (Exception ex)
                                    {
                                        processNextPage = false;
                                    }

                                    //  await page.EvaluateFunctionAsync("() => {var links = document.getElementsByClassName('pagination__next icon-link');if(links.length > 0) {links[0].click();}}");

                                }
                                //"https://shopgoodwill.com/categories/listing?st=" + searchCall + "&sg=&c=&s=&lp=0&hp=999999&sbn=&spo=false&snpo=false&socs=false&sd=false&sca=false&caed=2%2F7%2F2022&cadb=7&scs=false&sis=false&col=1&p=" + pageRecords.ToString() + "&ps=40&desc=false&ss=0&UseBuyerPrefs=true&sus=false&cln=1&catIds=&pn=&wc=false&mci=false&hmt=false&layout=grid");
                                // Type into search box.
                                */


/*
                                           await page.EvaluateFunctionAsync("() => {var searchBar = document.getElementById('gh-ac'); searchBar.value = '" + auctionSearchWord.SearchWord + "';}");
                                           Thread.Sleep(1000);


                                           await page.EvaluateFunctionAsync("() => {document.getElementById('gh-cat').value = '11450';}");
                                           Thread.Sleep(1000);


                                           await page.ClickAsync("[id='gh-btn']", new ClickOptions() { Button = MouseButton.Left });

                                           */
//await page.EvaluateFunctionAsync("() => {var btnSubmit = document.getElementById('gh-btn');btnSubmit.click();}");
//Thread.Sleep(10000);

//var titles = document.getElementsByClassName('srp-refine__category__item'); var ainfo = titles[0].getElementsByTagName('a');ainfo[0].click();
//ainfo[0].innerText
//ainfo[0].href
//document.getElementById('gh-cat').value = '11450'
// await page.EvaluateFunctionAsync("() => {var titles = document.getElementsByClassName('srp-refine__category__item'); var ainfo = titles[0].getElementsByTagName('a');ainfo[0].click();}");

//await page.EvaluateFunctionAsync("() => {window.location.href = 'https://www.ebay.com/sch/281/i.html?_from=R40&amp;_nkw=acne&amp;_oac=1';}");


//Thread.Sleep(10000);