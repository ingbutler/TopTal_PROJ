using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using OpenPop.Mime;
using OpenPop.Pop3;

namespace DBRunsE2ETests
{

    class Utils
    {

        private static readonly HttpClient client = new HttpClient();



        public static string RetrieveVerificationLink()
        {
            var client = new Pop3Client();
            client.Connect(Settings.MailServer, Settings.MailPort, true);
            client.Authenticate(Settings.MailUser, Settings.MailPwd);

            var count = client.GetMessageCount();
            Message message = client.GetMessage(count);

            if (message.Headers.Subject == "Please confirm your account")
            {
                MessagePart html = message.FindFirstHtmlVersion();
                if (html != null)
                {
                    string content = html.GetBodyAsText();
                    return content.Substring(9, 80);
                }
            }

            return "";
        }



        public static async Task PostRequest(string controller, string action, string body)
        {
            client.DefaultRequestHeaders
                  .Accept
                  .Add(new MediaTypeWithQualityHeaderValue("application/json"));    //ACCEPT header

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "relativeAddress");

            request.Content =
                new StringContent(
                    body,
                    Encoding.UTF8,
                    "application/json"
                );

            await client.PostAsync(Settings.AppUri + controller + (String.IsNullOrEmpty(action) ? "" : ("/" + action)), request.Content)
                  .ContinueWith(responseTask =>
                  {
                      Console.WriteLine("Response: {0}", responseTask.Result);
                  });
        }



        public static async Task GetRequest(string requestUri)
        {
            await client.GetAsync(requestUri);
        }

    }

}