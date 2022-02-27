using PuppeteerSharp;
using PuppeteerSharp.Input;
using sqs_processor.Entities;
using sqs_processor.Models;
using sqs_processor.Services.Factories;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace sqs_processor.Processes
{
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

        public static async Task GetAuctionItems(IUnitOfWork _unitOfWork)
        {
            List<AuctionSearchWord> auctionSearchWords = _unitOfWork.auctionRepository.GetAuctionSearchWords();
            List<AuctionSite> auctionSites = _unitOfWork.auctionRepository.GetAuctionSites();
            List<AuctionSearchSiteRun> auctionSearchSiteRuns = _unitOfWork.auctionRepository.GetAuctionSearchSiteRuns();



            foreach (var auctionSite in auctionSites)
            {

                if (auctionSite.Id == 2)
                {
                   continue;
                }
                foreach (var auctionSearchWord in auctionSearchWords)
                {
                    if(auctionSearchWord.SearchWord != "acne")
                    {
                       // continue;
                    }
                    DateTime currentDate = DateTime.UtcNow.AddDays(-2);
                    if (auctionSearchSiteRuns.Where(x => x.AuctionSiteId == auctionSite.Id && x.AuctionSearchWordId == auctionSearchWord.Id && x.DateSearch > currentDate).Count() > 0)
                    {
                       // continue;
                    }

                    try
                    {

                        var options = new LaunchOptions { Headless = true };
                        Console.WriteLine("Downloading chromium");

                        await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
                        

                        using (var browser = await Puppeteer.LaunchAsync(options))
                        using (var page = await browser.NewPageAsync())
                        {
                            List<SiteAuctionItemDto> totalresultsTest = new List<SiteAuctionItemDto>();
                            bool processNextPage = true;
                            int pageRecords = 1;
                            while (processNextPage)
                            {
                                //alc
                                string url = auctionSite.SearchURL.Replace(auctionSite.SearchWordReplace, auctionSearchWord.SearchWord).Replace(auctionSite.PageReplace, pageRecords.ToString());
                                Console.WriteLine("Navigating to " + url);
                                // await page.GoToAsync(url);
                                if (pageRecords == 1 || auctionSite.Id == 1)
                                {
                                    await page.GoToAsync(url);
                                    //await page.GoToAsync("https://www.ebay.com");
                                }
                                else
                                {
                                    try
                                    {
                                        //await page.ClickAsync("[class='pagination__item icon-link'][[class='pagination__item icon-link'].length]", new ClickOptions() { Button = MouseButton.Left });

                                        //"var aItems = document.getElementsByClassName('pagination__item'); var itemsLength = aItems.length; for (var iItem = 0; iItem < itemsLength; iItem++) {if(aItems[iItem].innerText = '" + pageRecords.ToString() + "') {aItems[iItem].click();}  }"

                                        //await page.EvaluateFunctionAsync("() => {var aItems = document.getElementsByClassName('pagination__item'); var itemsLength = aItems.length; for (var iItem = 0; iItem < itemsLength; iItem++) {if(aItems[iItem].innerText = '" + pageRecords.ToString() + "') {aItems[iItem].click();}  }}");

                                        await page.ClickAsync("[class='pagination__next icon-link']", new ClickOptions() { Button = MouseButton.Left });
                                    }
                                    catch (Exception ex)
                                    {
                                        processNextPage = false;
                                    }

                                    //  await page.EvaluateFunctionAsync("() => {var links = document.getElementsByClassName('pagination__next icon-link');if(links.length > 0) {links[0].click();}}");

                                }
                                //"https://shopgoodwill.com/categories/listing?st=" + searchCall + "&sg=&c=&s=&lp=0&hp=999999&sbn=&spo=false&snpo=false&socs=false&sd=false&sca=false&caed=2%2F7%2F2022&cadb=7&scs=false&sis=false&col=1&p=" + pageRecords.ToString() + "&ps=40&desc=false&ss=0&UseBuyerPrefs=true&sus=false&cln=1&catIds=&pn=&wc=false&mci=false&hmt=false&layout=grid");
                                // Type into search box.

                                Thread.Sleep(2000);

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

                                if (pageRecords == 1 && auctionSite.Id == 2)
                                {

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
                                }
                                else
                                {

                                }


                                try
                                {

                                    /*
                                    bool captchaRequired = await page.EvaluateFunctionAsync<bool>(captchaCheck);
                                    if (captchaRequired)
                                    {
                                        Thread.Sleep(30000);

                                    }
                                    */
                                    SiteAuctionItemDto[] resultsTest = await page.EvaluateFunctionAsync<SiteAuctionItemDto[]>(auctionSite.JsCode);
                                    string hrefValue = await page.EvaluateFunctionAsync<string>("() => {return window.location.href;} ");
                                    int liCount = await page.EvaluateFunctionAsync<int>("() => {return document.getElementsByTagName('li').length;} ");

                                    string innerHTML = await page.EvaluateFunctionAsync<string>("() => {return document.documentElement.innerHTML;} ");
                                    
                                    Console.WriteLine(hrefValue + " liCount " + liCount.ToString());
                                    if (resultsTest == null || resultsTest.Length < 40 || totalresultsTest.Count > 500)
                                    {
                                        processNextPage = false;
                                    }
                                    pageRecords += 1;
                                    var info2 = 12;
                                    //BOT_MAX_PAGE_NUMBER_EXCEEDED
                                    var matchingRecords = totalresultsTest.Join(resultsTest, x => x.ItemUrl, y => y.ItemUrl, (newRecs, curRecs) => new { newRecs, curRecs }).Select(x => x.curRecs);
                                    var matchingRecordCount = matchingRecords.Count();
                                    var currentMissingRecords = resultsTest.Except(matchingRecords).ToList();

                                    var otherMatchingRecords = totalresultsTest.Join(currentMissingRecords, x => x.ImageUrl, y => y.ImageUrl, (newRecs, curRecs) => new { newRecs, curRecs }).Select(x => x.curRecs);
                                    var otherMatchingRecordCount = otherMatchingRecords.Count();
                                    var missingRecords = currentMissingRecords.Except(otherMatchingRecords).ToList();
                                    if (missingRecords.Count == 0)
                                    {
                                        processNextPage = false;
                                    }
                                    else
                                    {

                                        totalresultsTest.AddRange(missingRecords);
                                    }

                                }
                                catch (Exception ex)
                                {
                                    processNextPage = false;
                                    string error = ex.Message;
                                }

                            }
                            List<AuctionItemDto> auctionItems = new List<AuctionItemDto>();
                            if (totalresultsTest.Count > 0)
                            {

                                foreach (var totalresult in totalresultsTest)
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

                                        auctionItem.Url = totalresult.ItemUrl;
                                        int totalBidsOutPut;
                                        if (totalresult.TotalBids == null || totalresult.TotalBids == "")
                                        {
                                            totalresult.TotalBids = "0";
                                        }
                                        Int32.TryParse(totalresult.TotalBids.Replace("Bids: ", ""), out totalBidsOutPut);

                                        auctionItem.TotalBids = totalBidsOutPut;

                                        auctionItem.ProductName = totalresult.ProductName;
                                        //totalresult.TimeLeft

                                        DateTime currendate = GetAuctionDate(totalresult.TimeLeft);

                                        auctionItem.AuctionEndDate = currendate;
                                        //3d 5h
                                        //2h 5m

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
                            Console.WriteLine("Press any key to continue...");
                           



                        }
                        AuctionSearchSiteRunDto searchSiteRun = new AuctionSearchSiteRunDto();
                        searchSiteRun.AuctionSearchWordId = auctionSearchWord.Id;
                        searchSiteRun.AuctionSiteId = auctionSite.Id;
                       _unitOfWork.auctionRepository.UpsertAuctionSearchSiteRun(searchSiteRun);

                    }
                    catch (Exception ex)
                    {
                        string testError = auctionSearchWord.SearchWord + " " + ex.Message;
                    }
                }
            }

            processingRecords = false;

        }

        private static DateTime GetAuctionDate(string timeLeft)
        {

            DateTime currendate = DateTime.UtcNow;


            if (timeLeft == null || timeLeft == "")
            {
                return currendate.AddYears(5);
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
