using AutoMapper;
using PuppeteerSharp;
using PuppeteerSharp.Input;
using sqs_processor.Entities;
using sqs_processor.Models;
using sqs_processor.ResourceParameters;
using sqs_processor.Services.Factories;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace sqs_processor.Processes
{
    class ProcessAuctionSingleSiteEnd : IProcess
    {
        private static bool processingRecords;
        private readonly IUnitOfWork _unitOfWork;
        private static  IMapper _mapper;
        public ProcessAuctionSingleSiteEnd(IServiceFactory serviceFactory)
        {
            _unitOfWork = serviceFactory.GetUnitOfWorkFactoryService().GetUnitOfWork();

            _mapper = serviceFactory.GetMapperService();
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

            /*
            Process[] procs = Process.GetProcessesByName("chrome");
            Console.WriteLine("Process Count: " + procs.Count().ToString());
            foreach (var proc in procs)
            {
                
                proc.Kill();
            }
            */
            int counterBreakMax = 5000;
            int counterBreak = 0;



            /*
            AuctionItemsResourceParameters auctionItemsRP = new AuctionItemsResourceParameters();
            
            auctionItemsRP.AuctionEndProcessed = false;
            auctionItemsRP.AuctionEndDateRangeMin = DateTime.UtcNow;
            var auctionItemsss = _unitOfWork.auctionRepository.GetAuctionItems(auctionItemsRP);
            Dictionary<string, int> auctionWords = new Dictionary<string, int>();
            foreach(var auctionItem in auctionItemsss)
            {
                string[] productNames = auctionItem.ProductName.Split(' ');

                if(auctionItem.AuctionSearchWordId != 260) { continue; }

                foreach(var productName in productNames)
                {
                    if (auctionWords.ContainsKey(productName))
                    {
                        auctionWords[productName] = auctionWords[productName] + 1;
                    }
                    else
                    {
                        auctionWords[productName] = 1;

                    }
                }
            }
            var auctionRefined = auctionWords.Where(x=>x.Value>2).OrderBy(x => x.Value).ToList();
      
            */

            AuctionItemsResourceParameters auctionItemsResourceParameters = new AuctionItemsResourceParameters();
            auctionItemsResourceParameters.AuctionSiteId = 1;
            auctionItemsResourceParameters.AuctionEndProcessed = false;
            auctionItemsResourceParameters.AuctionEndDateRangeMax = DateTime.UtcNow;
            var auctionItems = _unitOfWork.auctionRepository.GetAuctionItems(auctionItemsResourceParameters);


            List<AuctionSite> auctionSites = _unitOfWork.auctionRepository.GetAuctionSites();
            List<AuctionItemDto> processAuctionItems = _mapper.Map<List<AuctionItemDto>>(auctionItems).ToList();
        
            List<AuctionItemDto> auctionUpdateItems = new List<AuctionItemDto>();
            auctionItems = auctionItems.OrderBy(x => x.AuctionEndDate).ToList();




            try
            {

                var options = new LaunchOptions { Headless = true };
                Console.WriteLine("Downloading chromium");

                await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);


                using (var browser = await Puppeteer.LaunchAsync(options))
                using (var page = await browser.NewPageAsync())
                {

                    foreach (var processAuctionItem in processAuctionItems)
                    {
                        if (counterBreak > counterBreakMax)
                        {
                            break;
                        }


                        await page.GoToAsync(processAuctionItem.Url);

                        int countCheck = 0;
                        string innerHTMLVal = await page.EvaluateFunctionAsync<string>("() => {return document.documentElement.innerHTML;} ");
                        bool pageHasLoaded = false;
                        string[] urls = processAuctionItem.Url.Split("/");
                        string itemId = urls[urls.Length - 1];
                        while ((!pageHasLoaded
                               )
                               && countCheck < 5)
                        {

                            bool currentGroupingPageLoaded = false;


                            currentGroupingPageLoaded = innerHTMLVal.Contains(itemId);

                            if (currentGroupingPageLoaded)
                            {
                                pageHasLoaded = true;
                            }

                            Thread.Sleep(2000);
                            innerHTMLVal = await page.EvaluateFunctionAsync<string>("() => {return document.documentElement.innerHTML;} ");
                            countCheck += 1;
                        }

                        try

                        {
                            string jsCode = GetJSONCode(auctionSites, processAuctionItem.AuctionSiteId);
                            SiteAuctionItemDto siteResultsCurrentPull = await page.EvaluateFunctionAsync<SiteAuctionItemDto>(jsCode);
                            string hrefValue = await page.EvaluateFunctionAsync<string>("() => {return window.location.href;} ");
                            string innerHTML = await page.EvaluateFunctionAsync<string>("() => {return document.documentElement.innerHTML;} ");


                            decimal output;
                            int intOutPut;
                            bool valueChanged = false;

                            if (siteResultsCurrentPull.TimeLeft != null && siteResultsCurrentPull.TimeLeft != "")
                            {
                                valueChanged = true;
                                DateTime? timeLeft = GetAuctionDate(siteResultsCurrentPull.TimeLeft);
                                if (timeLeft.HasValue && (timeLeft > processAuctionItem.AuctionEndDate || !processAuctionItem.AuctionEndDate.HasValue))
                                {
                                    processAuctionItem.AuctionEndDate = timeLeft;
                                }
                            }

                            if (siteResultsCurrentPull.AuctionEnded == "Auction Ended")
                            {
                                valueChanged = true;
                                processAuctionItem.AuctionEndProcessed = true;
                                if (!processAuctionItem.AuctionEndDate.HasValue) { processAuctionItem.AuctionEndDate = DateTime.UtcNow; }
                                if (processAuctionItem.AuctionEndDate > DateTime.UtcNow)
                                {
                                    processAuctionItem.AuctionEndDate = DateTime.UtcNow;
                                }
                            }

                            if (siteResultsCurrentPull.TotalBids != null && siteResultsCurrentPull.TotalBids != "")
                            {
                                valueChanged = true;
                                Int32.TryParse(siteResultsCurrentPull.TotalBids, out intOutPut);
                                processAuctionItem.TotalBids = intOutPut;
                            }

                            if (siteResultsCurrentPull.ItemPrice != null && siteResultsCurrentPull.ItemPrice != "")
                            {
                                valueChanged = true;
                                decimal.TryParse(siteResultsCurrentPull.ItemPrice, out output);
                                processAuctionItem.ItemPrice = output;
                            }

                            if (siteResultsCurrentPull.ItemShipping != null && siteResultsCurrentPull.ItemShipping != "")
                            {
                                valueChanged = true;
                                decimal.TryParse(siteResultsCurrentPull.ItemShipping, out output);
                                processAuctionItem.ItemShipping = output;
                            }



                            //timeleft
                            if (valueChanged)
                            {
                                auctionUpdateItems.Add(processAuctionItem);
                            }
                            else
                            {
                                processAuctionItem.AuctionEndDate = DateTime.UtcNow.AddDays(10);
                                auctionUpdateItems.Add(processAuctionItem);
                            }
                        }
                        catch (Exception ex)
                        {
                            string error = ex.Message;
                        }



                        if (auctionUpdateItems.Count > 9)
                        {
                            int partitionLoop = 0;
                            int partitionInterval = 100;

                            while (auctionUpdateItems.Count >= partitionLoop)
                            {
                                _unitOfWork.auctionRepository.UpsertAuctionItems(auctionUpdateItems.Skip(partitionLoop).Take(partitionInterval).ToList());
                                partitionLoop += partitionInterval;
                            }
                            counterBreak += auctionUpdateItems.Count;
                            auctionUpdateItems = new List<AuctionItemDto>();
                        
                        }


                    }



                    if (auctionUpdateItems.Count > 0)
                    {
                        int partitionLoop = 0;
                        int partitionInterval = 100;
                        while (auctionUpdateItems.Count >= partitionLoop)
                        {
                            _unitOfWork.auctionRepository.UpsertAuctionItems(auctionUpdateItems.Skip(partitionLoop).Take(partitionInterval).ToList());
                            partitionLoop += partitionInterval;
                        }
                    }


                    processingRecords = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                processingRecords = false;
                //  string testError = auctionSearchWord.SearchWord + " " + ex.Message;
            }
                
            }


      private static string GetJSONCode(List<AuctionSite> auctionSites, int auctionSiteId)
        {
            string jsCode = "";

            var auctionSite = auctionSites.Where(x => x.Id == auctionSiteId).FirstOrDefault();
            if(auctionSite != null) { jsCode = auctionSite.JsCodeSinglePage; }

            return jsCode;
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
 *using AutoMapper;
using PuppeteerSharp;
using PuppeteerSharp.Input;
using sqs_processor.Entities;
using sqs_processor.Models;
using sqs_processor.ResourceParameters;
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
    class ProcessAuctionSingleSiteEnd : IProcess
    {
        private static bool processingRecords;
        private readonly IUnitOfWork _unitOfWork;
        private static  IMapper _mapper;
        public ProcessAuctionSingleSiteEnd(IServiceFactory serviceFactory)
        {
            _unitOfWork = serviceFactory.GetUnitOfWorkFactoryService().GetUnitOfWork();

            _mapper = serviceFactory.GetMapperService();
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
           
            int counterBreakMax = 5000;
            int counterBreak = 0;
            AuctionItemsResourceParameters auctionItemsResourceParameters = new AuctionItemsResourceParameters();
            auctionItemsResourceParameters.AuctionSiteId = 1;
            auctionItemsResourceParameters.AuctionEndProcessed = false;
            auctionItemsResourceParameters.AuctionEndDateRangeMax = DateTime.UtcNow;
            var auctionItems = _unitOfWork.auctionRepository.GetAuctionItems(auctionItemsResourceParameters);


            List<AuctionSite> auctionSites = _unitOfWork.auctionRepository.GetAuctionSites();
            List<AuctionItemDto> processAuctionItems = _mapper.Map<List<AuctionItemDto>>(auctionItems).ToList();
        
            List<AuctionItemDto> auctionUpdateItems = new List<AuctionItemDto>();
            auctionItems = auctionItems.OrderBy(x => x.AuctionEndDate).ToList();

                foreach (var processAuctionItem in processAuctionItems)
                {
                    if (counterBreak > counterBreakMax)
                    {
                        break;
                    }

                    
                    

                    try
                    {

                        var options = new LaunchOptions { Headless = true };
                        Console.WriteLine("Downloading chromium");

                        await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);

                         
                        using (var browser = await Puppeteer.LaunchAsync(options))
                        using (var page = await browser.NewPageAsync())
                        {

                        await page.GoToAsync(processAuctionItem.Url);

                        int countCheck = 0;
                        string innerHTMLVal = await page.EvaluateFunctionAsync<string>("() => {return document.documentElement.innerHTML;} ");
                        bool pageHasLoaded = false;
                        string[] urls = processAuctionItem.Url.Split("/");
                        string itemId = urls[urls.Length - 1];
                        while ((!pageHasLoaded
                               )
                               && countCheck < 5)
                        {

                            bool currentGroupingPageLoaded = false;


                            currentGroupingPageLoaded = innerHTMLVal.Contains(itemId);// && innerHTMLVal.Contains(processAuctionItem.ProductName);

                            if (currentGroupingPageLoaded)
                            {
                                pageHasLoaded = true;
                            }

                            Thread.Sleep(2000);
                            innerHTMLVal = await page.EvaluateFunctionAsync<string>("() => {return document.documentElement.innerHTML;} ");
                            countCheck += 1;
                        }




                        try
                            
                        {
                            string jsCode = GetJSONCode(auctionSites, processAuctionItem.AuctionSiteId );
                            SiteAuctionItemDto siteResultsCurrentPull = await page.EvaluateFunctionAsync<SiteAuctionItemDto>(jsCode);
                            string hrefValue = await page.EvaluateFunctionAsync<string>("() => {return window.location.href;} ");
                            string innerHTML = await page.EvaluateFunctionAsync<string>("() => {return document.documentElement.innerHTML;} ");


                            decimal output;
                            int intOutPut;
                            if(siteResultsCurrentPull.AuctionEnded== "Auction Ended")
                            {
                                processAuctionItem.AuctionEndProcessed = true;
                                if(processAuctionItem.AuctionEndDate > DateTime.UtcNow)
                                {
                                    processAuctionItem.AuctionEndDate = DateTime.UtcNow;
                                }
                            }

                            if (siteResultsCurrentPull.TotalBids != null && siteResultsCurrentPull.TotalBids != "" )
                            {
                                Int32.TryParse(siteResultsCurrentPull.TotalBids, out intOutPut);
                                processAuctionItem.TotalBids = intOutPut;
                            }

                            if (siteResultsCurrentPull.ItemPrice != null && siteResultsCurrentPull.ItemPrice != "")
                            {
                                decimal.TryParse(siteResultsCurrentPull.ItemPrice, out output);
                                processAuctionItem.ItemPrice = output;
                            }

                            if (siteResultsCurrentPull.ItemShipping != null && siteResultsCurrentPull.ItemShipping != "")
                            {
                                decimal.TryParse(siteResultsCurrentPull.ItemShipping, out output);
                                processAuctionItem.ItemShipping = output;
                            }

                            if(siteResultsCurrentPull.TimeLeft != null && siteResultsCurrentPull.TimeLeft != "")
                            {
                                DateTime timeLeft = GetAuctionDate(siteResultsCurrentPull.TimeLeft);
                                if (timeLeft > processAuctionItem.AuctionEndDate)
                                {
                                    processAuctionItem.AuctionEndDate = timeLeft;
                                }
                            }
                          
                            //timeleft

                             auctionUpdateItems.Add(processAuctionItem);
                        }
                        catch (Exception ex)
                        {
                            string error = ex.Message;
                        }
                    }

                  
                    if (auctionUpdateItems.Count > 9)
                    {
                        int partitionLoop = 0;
                        int partitionInterval = 100;

                        while (auctionUpdateItems.Count >= partitionLoop)
                        {
                            _unitOfWork.auctionRepository.UpsertAuctionItems(auctionUpdateItems.Skip(partitionLoop).Take(partitionInterval).ToList());
                            partitionLoop += partitionInterval;
                        }
                        auctionUpdateItems = new List<AuctionItemDto>();
                    }


                    


                }
                    catch (Exception ex)
                    {
                      //  string testError = auctionSearchWord.SearchWord + " " + ex.Message;
                    }
                }
            }


        



      private static string GetJSONCode(List<AuctionSite> auctionSites, int auctionSiteId)
        {
            string jsCode = "";

            var auctionSite = auctionSites.Where(x => x.Id == auctionSiteId).FirstOrDefault();
            if(auctionSite != null) { jsCode = auctionSite.JsCodeSinglePage; }

            return jsCode;
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



*/