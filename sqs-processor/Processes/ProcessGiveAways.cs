using AutoMapper;
using PuppeteerSharp;
using sqs_processor.Services.Factories;
using sqs_processor.Services.repos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace sqs_processor.Processes
{
    public class Instagram
    {
        
        public string DateCreated { get; set; }
        public string PostDate { get; set; }
        public string PostRawData { get; set; }
        public string InstagramUserId { get; set; }
        public string RewardType { get; set; }
        public string IsActionable { get; set; }
        public string InstagramPostURL { get; set; }
        public string Following { get; set; }
        public string GiveAwayExpires { get; set; }
        public string ProcessedPost { get; set; }
    }

    public class ProcessGiveAways : IProcess
    {
        private static bool processingRecords;
        private readonly IUnitOfWork _unitOfWork;
        private static IMapper _mapper;
        public ProcessGiveAways(IServiceFactory serviceFactory)
        {
            _unitOfWork = serviceFactory.GetUnitOfWorkFactoryService().GetUnitOfWork();

            _mapper = serviceFactory.GetMapperService();
        }

        public void RunTask()
        {




            processingRecords = true;
            Task detail = GiveAwayProcess(_unitOfWork);

            while (processingRecords)
            {

            }





        }

        public static async Task GiveAwayProcess(IUnitOfWork _unitOfWork)
        {
            try
            {
                var options = new LaunchOptions { Headless = false };
                Console.WriteLine("Downloading chromium");

                await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);


                using (var browser = await Puppeteer.LaunchAsync(options))
                using (var page = await browser.NewPageAsync())
                {

                    await page.GoToAsync("https://www.instagram.com/");

                    Thread.Sleep(5000);

                    await page.TypeAsync("input[name='username']", "smartalex302@aol.com");
                    Thread.Sleep(1000);
                    await page.TypeAsync("input[name='password']", "Inscaliber07");
                    Thread.Sleep(1000);
                    await page.ClickAsync("button[type='submit']");
                    Thread.Sleep(10000);


                    //await page.TypeAsync("input[name='username']", "smartalex302@aol.com");
                    await page.GoToAsync("https://www.instagram.com/");
                    Thread.Sleep(10000);
                    // aOOlW HoLwm
                    //<button class="aOOlW   HoLwm " tabindex="0">Not Now</button>

                    await page.EvaluateFunctionAsync<string>("() => {var notNowClick = document.getElementsByClassName('aOOlW   HoLwm '); notNowClick[0].click();}");

                    Thread.Sleep(2000);
                    await page.GoToAsync("https://www.instagram.com/explore/tags/disneygiveaway/");
                    Thread.Sleep(2000);
                    await page.EvaluateFunctionAsync<string>("() => {var records = document.getElementsByTagName('a');  records[0].click();}");

                    


                    Thread.Sleep(2000);
                    int postLoop = 20;
                    int postCounter = 0;
                    List<Instagram> instagramPosts = new List<Instagram>();
                    while (postLoop> postCounter)
                    {
                        Instagram siteResultsCurrentPull = await page.EvaluateFunctionAsync<Instagram>("() => {var instagramPost = {}; try{ var postDetails = document.getElementsByClassName('MOdxS '); instagramPost.postRawData = postDetails[0].innerText; var userName=document.getElementsByClassName('sqdOP yWX7d     _8A5w5   ZIAjV ');instagramPost.instagramUserId = userName[0].innerText;var postDate=document.getElementsByClassName('_1o9PC');instagramPost.postDate =  postDate[0].title;instagramPost.instagramPostURL = document.location.href;}catch(ex){}return instagramPost;}");
                        instagramPosts.Add(siteResultsCurrentPull);
                        
                        await page.EvaluateFunctionAsync("() => {var clickNext = document.getElementsByClassName(' l8mY4 feth3');var clickbtn = clickNext[0].getElementsByTagName('button');clickbtn[0].click();}");
                        //document.documentElement.innerHTML.indexOf('_1o9PC')


                        int countCheck = 0;
                        string innerHTMLVal = await page.EvaluateFunctionAsync<string>("() => {return document.documentElement.innerHTML;} ");
                        bool pageHasLoaded = false;
                        
                        
                        while ((!pageHasLoaded
                               )
                               && countCheck < 5)
                        {
                            bool currentGroupingPageLoaded = false;
                            currentGroupingPageLoaded = innerHTMLVal.Contains("_1o9PC");// && innerHTMLVal.Contains(processAuctionItem.ProductName);
                            if (currentGroupingPageLoaded)
                            {
                                pageHasLoaded = true;
                            }
                            Thread.Sleep(1000);
                            innerHTMLVal = await page.EvaluateFunctionAsync<string>("() => {return document.documentElement.innerHTML;} ");
                            countCheck += 1;
                        }






                        postCounter +=1;
                    }

                    Thread.Sleep(2000);
                }
            }
            catch (Exception ex) {
            
            }

            processingRecords = false;
        }

        }
}
