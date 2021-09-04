using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace sqs_processor.Services.Network
{
    class WebClientWrapper : IWebClientWrapper
    {

        public string GetCSVStringAsync(string url)
        {
            string results = string.Empty;
            try
            {
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                // Use the existing 'ProtocolVersion' , and display it onto the console.	

                // Set the 'ProtocolVersion' property of the 'HttpWebRequest' to 'Version1.0' .
                myHttpWebRequest.ProtocolVersion = HttpVersion.Version11;
                // Assign the response object of 'HttpWebRequest' to a 'HttpWebResponse' variable.
                HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                Console.WriteLine("\nThe 'ProtocolVersion' of the protocol changed to {0}", myHttpWebRequest.ProtocolVersion);
                Console.WriteLine("\nThe protocol version of the response object is {0}", myHttpWebResponse.ProtocolVersion);

            }
            catch (Exception ex)
            {

            }
            return results;
        }
        public string GetHTMLString(string url)
        {
            string html = String.Empty;
            try
            {
                //                ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                // ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | (SecurityProtocolType)3072 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls11;
                System.Net.ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Ssl3;
                ServicePointManager.Expect100Continue = true;
                WebRequest request = WebRequest.Create(url);

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    Stream data = response.GetResponseStream();

                    using (StreamReader sr = new StreamReader(data))
                    {
                        html = sr.ReadToEnd();
                    }
                    data = null;
                    request = null;


                }
            }
            catch (Exception ex)
            {

            }
            return html;
        }
    }
}
