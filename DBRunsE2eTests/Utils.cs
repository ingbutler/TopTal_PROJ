using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenPop.Mime;
using OpenPop.Pop3;

namespace DBRunsE2ETests
{

    class Utils
    {

        private static readonly HttpClient client = new HttpClient();

        static Utils()
        {
            client.DefaultRequestHeaders
                  .Accept
                  .Add(new MediaTypeWithQualityHeaderValue("application/json"));    //ACCEPT header
        }



        public static string RetrieveVerificationLink(DateTime dateSent)
        {
            var client = new Pop3Client();

            for (int j = 0; j < 10; j++)
            {
                client.Connect(Settings.MailServer, Settings.MailPort, true);
                client.Authenticate(Settings.MailUser, Settings.MailPwd);

                var count = client.GetMessageCount();
                bool mailFound = false;

                for (int i = count; i > 0; i--)
                {
                    var header = client.GetMessageHeaders(i);
                    if (header.DateSent >= TimeZoneInfo.ConvertTimeToUtc(dateSent)
                        && header.Subject == "Please confirm your account")
                    {
                        Message message = client.GetMessage(count);

                        MessagePart html = message.FindFirstHtmlVersion();
                        if (html != null)
                        {
                            string content = html.GetBodyAsText();
                            content = content.Replace("<a href='", "");
                            content = content.Replace("'>Please click Here to confirm your email</a>", "");
                            return content;
                        }

                        mailFound = true;
                        break;
                    }
                }

                if (mailFound)
                    break;

                client.Disconnect();
                Thread.Sleep(5000);     // Five seconds' wait before next attempt
            }

            return "";
        }



        public static async Task<HttpResponseMessage> PostRequest(string controller, string action, List<KeyValuePair<string, string>> headers, string body)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(Settings.AppUri + controller + (String.IsNullOrEmpty(action) ? "" : ("/" + action)));

            if (headers != null)
                headers.ForEach(x => request.Headers.Add(x.Key, x.Value));

            request.Content =
                new StringContent(
                    body,
                    Encoding.UTF8,
                    "application/json"
                );

            HttpResponseMessage response = await client.SendAsync(request);
            Console.WriteLine($"Response: {response}");
            string contentStr = await response.Content.ReadAsStringAsync();
            if(!String.IsNullOrEmpty(contentStr))
                Console.Write($"Response content: {JValue.Parse(contentStr).ToString(Newtonsoft.Json.Formatting.Indented)}");
            Console.WriteLine();
            Console.WriteLine();

            return response;
        }



        public static async Task GetRequest(string controller, string action, List<KeyValuePair<string, string>> headers, string queryString)
        {
            string requestUri = Settings.AppUri + controller + (String.IsNullOrEmpty(action) ? "" : ("/" + action)) + (String.IsNullOrEmpty(queryString) ? "" : ("?" + queryString));
            await GetRequest(requestUri, headers);
        }



        public static async Task GetRequest(string requestUri, List<KeyValuePair<string, string>> headers)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri(requestUri);

            if (headers != null)
                headers.ForEach(x => request.Headers.Add(x.Key, x.Value));

            HttpResponseMessage response = await client.SendAsync(request);
            Console.WriteLine($"Response: {response}");
            string contentStr = await response.Content.ReadAsStringAsync();
            if (!String.IsNullOrEmpty(contentStr))
                Console.Write($"Response content: {JValue.Parse(contentStr).ToString(Newtonsoft.Json.Formatting.Indented)}");
            Console.WriteLine();
            Console.WriteLine();
        }

    }

}