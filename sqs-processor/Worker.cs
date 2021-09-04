using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using sqs_processor.Processes;
using sqs_processor.Services.amazon;
using sqs_processor.Services.Network;
using sqs_processor.Services.Network.Dividends;
using sqs_processor.Services.Network.Earnings;
using sqs_processor.Services.repos;

namespace sqs_processor
{
    public class Worker : BackgroundService
    {
        private const string assemblyNamespace = "sqs_processor.Processes.";
        private readonly ILogger<Worker> _logger;
        private readonly ISecuritiesRepository _securityRepository;
        private readonly IGetSecurityService _securityService;
        private readonly IGetEarningsService _earningService;
        private readonly IGetDividendsServices _dividendsService;
        private readonly IAmazonUtility _amazonUtility;
        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _securityRepository = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<ISecuritiesRepository>();
            _securityService = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IGetSecurityService>();
            _earningService = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IGetEarningsService>();
            _dividendsService = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IGetDividendsServices>();
            _amazonUtility = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IAmazonUtility>();
        }

        private void CreateWorkerClass(string objectToInstantiate)
        {
            var objectType = Type.GetType(objectToInstantiate);
            if (objectType != null)
            {
                bool needsISecurityRepository = false;
                bool needsIGetSecurityService = false;
                bool needsIGetEarningService = false;
                bool needsIGetDividendsService = false;
                bool needsIAmazonUtility = false;
                ConstructorInfo[] constructorInfoObj = objectType.GetConstructors();
                //BindingFlags.Instance | BindingFlags.Public, null,
                //CallingConventions.HasThis, types, null);

                //if(objectType.declared.dec)
                var parameters = constructorInfoObj[0].GetParameters();
                foreach (var parameter in parameters)
                {
                    if (parameter.ParameterType.Name == "ISecuritiesRepository")
                    {
                        needsISecurityRepository = true;
                    }
                    if (parameter.ParameterType.Name == "IGetSecurityService")
                    {
                        needsIGetSecurityService = true;

                    }
                    if (parameter.ParameterType.Name == "IGetEarningsService")
                    {
                        needsIGetEarningService = true;

                    }
                    if (parameter.ParameterType.Name == "IGetDividendsServices")
                    {
                        needsIGetDividendsService = true;

                    }
                    if (parameter.ParameterType.Name == "IAmazonUtility")
                    {
                        needsIAmazonUtility = true;

                    }
                    


                }
                //info[0].ParameterType.Name
                IProcess t;
                if (needsISecurityRepository && !needsIGetSecurityService && !needsIGetEarningService && !needsIGetDividendsService && !needsIAmazonUtility)
                {
                    t = Activator.CreateInstance(objectType, _securityRepository) as IProcess;
                }
                else if (needsISecurityRepository && needsIGetSecurityService)
                {
                    t = Activator.CreateInstance(objectType, _securityRepository, _securityService) as IProcess;
                }
                else if (needsISecurityRepository && needsIGetEarningService)
                {
                    t = Activator.CreateInstance(objectType, _securityRepository, _earningService) as IProcess;
                }
                else if (needsISecurityRepository && needsIGetDividendsService)
                {
                    t = Activator.CreateInstance(objectType, _securityRepository, _dividendsService) as IProcess;
                }
                else if (needsISecurityRepository && needsIAmazonUtility)
                {
                    t = Activator.CreateInstance(objectType, _securityRepository, _amazonUtility) as IProcess;
                }

                else
                {
                    throw new Exception("Constructor not defined");
                }
                // set a property value

                t.RunTask();
            }

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("no Messages" + !stoppingToken.IsCancellationRequested);
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                Console.WriteLine("there is messages" + !stoppingToken.IsCancellationRequested);

                try
                {
                    var sqsUrl = "https://sqs.us-east-2.amazonaws.com/930271955226/finacialservices-queue";// aswSection.GetSection("SQSUrl").Value;

                    AmazonSQSClient amazonSQSClient = new AmazonSQSClient(Amazon.RegionEndpoint.USEast2);

                    //Receive request
                    ReceiveMessageRequest receiveMessageRequest = new ReceiveMessageRequest(sqsUrl);
                    var response = await amazonSQSClient.ReceiveMessageAsync(receiveMessageRequest, stoppingToken);

                    if (response.Messages.Any())
                    {
                        foreach (Message message in response.Messages)
                        {
                            Console.WriteLine($"Message received");
                            Console.WriteLine($"Message: {message.Body}");
                            string taskProcess = message.Body;
                            try
                            {
                                string objectToInstantiate = assemblyNamespace + taskProcess;//"ProcessUpdateSecurity";

                                try
                                {
                                    //  string objectToInstantiate = assemblyNamespace + "ProcessUpdateSecurity";



                                    CreateWorkerClass(objectToInstantiate);
                                  
                                }
                                catch (Exception ex)
                                {

                                }
                                /*
                                var objectType = Type.GetType(objectToInstantiate);
                                if (objectType != null)
                                {
                                    var t = Activator.CreateInstance(objectType, _securityRepository, _securityService) as IProcess;

                                    // set a property value

                                    t.RunTask();
                                }
                                */

                            }
                            catch (Exception ex)
                            {

                            }



                            //Deleting message
                            var deleteMessageRequest = new DeleteMessageRequest(sqsUrl, message.ReceiptHandle);
                            await amazonSQSClient.DeleteMessageAsync(deleteMessageRequest, stoppingToken);

                            Console.WriteLine($"Message deleted");
                        }
                    }
                    else
                    {
                        Console.WriteLine("no Messages");
                    }

                    

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Messages error " + ex.Message);
                }


                // var t=(IProcess) System.Reflection.Assembly.GetExecutingAssembly().CreateInstance("ProcessUpdateSecurity");


                CreateWorkerClass("sqs_processor.Processes.ProcessPreferredGainers");


                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}
