using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DBRunsE2ETests
{

    public struct Settings
    {
        public const string AppPath = @"C:\Progetti\TopTal_PROJ\DBRuns\bin\Debug\netcoreapp3.1\DBRuns.exe";

        public const string MailServer = "pop.gmail.com";
        public const int MailPort = 995;
        public const string MailUser = "ing.d.butler@gmail.com";
        public const string MailPwd = "soLLi987!";

        public const string AppUri = "https://localhost:5001/api/";
        public const string MailAddress = MailUser;
    }




    class Program
    {

        static async Task Main(string[] args)
        {

            string body;


            // APP LAUNCH
            System.Diagnostics.Process.Start(Settings.AppPath);



            #region SET FIRST USER (ADMIN)

            body =
                @"
                    {
                        ""Email"":""" + Settings.MailAddress + @"""
                    }
                ";

            await Utils.PostRequest(body);


            string VerificationLink = Utils.RetrieveVerificationLink();


            await Utils.GetRequest(VerificationLink);

            #endregion SET FIRST USER (ADMIN)

        }




        //private static async Task<List<Repository>> ProcessRepositories()
        //{
        //    client.DefaultRequestHeaders.Accept.Clear();
        //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
        //    client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

        //    var streamTask = client.GetStreamAsync("https://api.github.com/orgs/dotnet/repos");
        //    var repositories = await JsonSerializer.DeserializeAsync<List<Repository>>(await streamTask);
        //    return repositories;
        //}

    }

}
