using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace sqs_processor.Services.amazon
{
    class AmazonUtility : IAmazonUtility
    {
       public async Task SendSNSMessage(string snsURL, string message)
        {
            Console.WriteLine("SendSNSMessage message" + message);
            var snsClient = new AmazonSimpleNotificationServiceClient(RegionEndpoint.GetBySystemName("us-east-2"));
            
            snsURL = snsURL.Replace(" ", "");
            try
            {
                var request = new PublishRequest
                {
                    Message = message,
                    TargetArn = snsURL

                };
              var info =   await snsClient.PublishAsync(request);
                Console.WriteLine("SendSNSMessage info" + info.HttpStatusCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine("SendSNSMessage error" + ex.Message);
            }

        }
    }
}
