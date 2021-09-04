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

            var snsClient = new AmazonSimpleNotificationServiceClient(RegionEndpoint.GetBySystemName("us-east-2"));
            
            snsURL = snsURL.Replace(" ", "");
            try
            {
                var request = new PublishRequest
                {
                    Message = message,
                    TargetArn = snsURL

                };
                await snsClient.PublishAsync(request);
            }
            catch (Exception ex)
            {
                
            }

        }
    }
}
