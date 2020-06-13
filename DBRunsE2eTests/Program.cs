using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var psi = new ProcessStartInfo(Settings.AppPath);
            psi.UseShellExecute = true;
            Process.Start(psi);



            #region SET FIRST USER (ADMIN)

            body =
                @"
                    {
                        ""Email"":""" + Settings.MailAddress + @"""
                    }
                ";

            await Utils.PostRequest("Users", "SignUp", body);


            // Check needed that the right email is read, that is, if it has been received after it has been created
            string VerificationLink = Utils.RetrieveVerificationLink();


            await Utils.GetRequest(VerificationLink);

            #endregion SET FIRST USER (ADMIN)

        }

    }

}
