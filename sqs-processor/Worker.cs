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
using Newtonsoft.Json;
using sqs_processor.Processes;
using sqs_processor.Services.amazon;
using sqs_processor.Services.Factories;
using sqs_processor.Services.Network;
using sqs_processor.Services.Network.Dividends;
using sqs_processor.Services.Network.Earnings;
using sqs_processor.Services.repos;

namespace sqs_processor
{

    class SQSMessage
    {

        public string Body { get; set; }

    }

    public class Worker : BackgroundService
    {
        private const string assemblyNamespace = "sqs_processor.Processes.";
        private readonly ILogger<Worker> _logger;
        
        private readonly IServiceFactory _serviceFactory;
        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            
            _serviceFactory = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IServiceFactory>();
        }

        private void CreateWorkerClass(string objectToInstantiate)
        {
            var objectType = Type.GetType(objectToInstantiate);
            if (objectType != null)
            {
            
                ConstructorInfo[] constructorInfoObj = objectType.GetConstructors();
                
                IProcess t;
                t = Activator.CreateInstance(objectType, _serviceFactory) as IProcess;
                t.RunTask();
            }

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

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
                            Console.WriteLine($"Message: {message.Body}");
                            
                            try
                            {

                                SQSMessage sqsMessage = JsonConvert.DeserializeObject<SQSMessage>(message.Body);
                                Console.WriteLine($"Message: {message.Body}");
                                string taskProcess = sqsMessage.Body;

                                Console.WriteLine($"Message: {sqsMessage.Body}");
                                string objectToInstantiate = assemblyNamespace + taskProcess;//"ProcessUpdateSecurity";

                                try
                                {
                                    //  string objectToInstantiate = assemblyNamespace + "ProcessUpdateSecurity";
                                    CreateWorkerClass(objectToInstantiate);
                                  
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"CreateWorkerClass Error: {ex.Message}");
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
                                Console.WriteLine($"General Error Message: {ex.Message}");
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



//                    CreateWorkerClass("sqs_processor.Processes.ProcessPeakRangeDetails");
                    
                   //CreateWorkerClass("sqs_processor.Processes.ProcessUpdateSecurity");
                  // CreateWorkerClass("sqs_processor.Processes.ProcessUpdateSecurityProfiles");
                    //CreateWorkerClass("sqs_processor.Processes.ProcessHistoricalPrices");
                    
                   // CreateWorkerClass("sqs_processor.Processes.ProcessSecurityPurchaseChecks");

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Messages error " + ex.Message);
                }


                // var t=(IProcess) System.Reflection.Assembly.GetExecutingAssembly().CreateInstance("ProcessUpdateSecurity");


               


                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}


/*
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
               */


/*
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
                */
