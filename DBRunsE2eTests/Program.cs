using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
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

        public const string AppUriNoDebug = "https://localhost:5001/api/";
        public const string AppUriDebug = "https://localhost:44322/api/";
        public static string AppUri;

        public const string Email = MailUser;
        public const string Password = "pipPop";
    }




    class Program
    {

        static async Task Main(string[] args)
        {
            HttpResponseMessage response;
            List<KeyValuePair<string, string>> headers;
            string body;
            string token;
            Process dbRuns = null;


            bool appDebug = true;
            if (appDebug)
                Settings.AppUri = Settings.AppUriDebug;
            else
            {
                Settings.AppUri = Settings.AppUriNoDebug;

                // APP LAUNCH
                var psi = new ProcessStartInfo(Settings.AppPath);
                psi.UseShellExecute = true;
                dbRuns = Process.Start(psi);
            }


            goto signIn;



            #region SET FIRST USER (ADMIN)

            Console.WriteLine("=====> SETTING FIRST USER (ADMIN)");
            Console.WriteLine();

            DateTime dateTimeMail = DateTime.Now;

            body =
                @"
                    {
                        ""Email"":""" + Settings.Email + @""",
                        ""Password"":""" + Settings.Password + @"""
                    }
                ";
            await Utils.PostRequest("Users", "SignUp", null, body);

            Console.WriteLine("=====> FIRST USER (ADMIN) SET");
            Console.WriteLine();
            Console.WriteLine("=====> VERIFYING MAIL");
            Console.WriteLine();

            string VerificationLink = Utils.RetrieveVerificationLink(dateTimeMail);
            if(VerificationLink == "")
            {
                Console.WriteLine("No verification mail was found");
                return;
            }

            await Utils.GetRequest(VerificationLink, null);

            Console.WriteLine("=====> FIRST USER'S MAIL VERIFIED");
            Console.WriteLine();

            #endregion SET FIRST USER (ADMIN)




            #region ADMIN SIGN-IN
signIn:
            Console.WriteLine("=====> ADMIN SIGNING IN");
            Console.WriteLine();

            body =
                @"
                    {
                        ""Email"":""" + Settings.Email + @""",
                        ""Password"":""" + Settings.Password + @"""
                    }
                ";
            response = await Utils.PostRequest("Users", "SignIn", null, body);

            Console.WriteLine("=====> ADMIN SIGNED IN");
            Console.WriteLine();

            #endregion ADMIN SIGN-IN




            #region ADMIN POSTING RUN

            Console.WriteLine("=====> ADMIN POSTING RUNS");
            Console.WriteLine();

            token = response.Headers.GetValues("x-token").First();

            body =
                @"
                    {
                        ""Date"":""2020-06-14T19:06"",
                        ""Distance"":12000,
                        ""TimeRun"":3600,
                        ""Location"":""Campi Bisenzio,IT""
                    }
                ";
            response = await Utils.PostRequest("Runs", null, GetBearerTokenHeader(token), body);

            body =
                @"
                    {
                        ""Date"":""2020-06-15T12:07"",
                        ""Distance"":12000,
                        ""TimeRun"":3430,
                        ""Location"":""Campi Bisenzio,IT""
                    }
                ";
            response = await Utils.PostRequest("Runs", null, GetBearerTokenHeader(token), body);

            Console.WriteLine("=====> ADMIN POSTED RUNS");
            Console.WriteLine();

            #endregion ADMIN POSTING RUN




            #region LISTING RUNS

            Console.WriteLine("=====> LISTING RUNS");
            Console.WriteLine();

            await Utils.GetRequest("Runs", null, GetBearerTokenHeader(token), null);

            Console.WriteLine("=====> RUNS LISTED");
            Console.WriteLine();

            #endregion LISTING RUNS




            #region ADMIN FILTERING USERS

            Console.WriteLine("=====> ADMIN FILTERING USERS (THEMSELVES)");
            Console.WriteLine();

            string queryString = "filter=email eq '" + Settings.Email + "'";

            await Utils.GetRequest("Users", null, GetBearerTokenHeader(token), queryString);

            Console.WriteLine("=====> ADMIN FILTERED USERS (THEMSELVES)");
            Console.WriteLine();

            #endregion ADMIN FILTERING USERS




            #region ADMIN EDITING USER

            Console.WriteLine("=====> ADMIN EDITING USER (CHANGING EMAIL TO THEMSELVES)");
            Console.WriteLine();

            // To be able to create another user with same password...

            // ================> TODO

            Console.WriteLine("=====> ADMIN EDITED USER (CHANGING EMAIL TO THEMSELVES)");
            Console.WriteLine();

            #endregion ADMIN EDITING USER






            if (!appDebug)
                dbRuns.Kill();
        }



        private static List<KeyValuePair<string, string>> GetBearerTokenHeader(string token)
        {
            return new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("authorization", "Bearer " + token) };
        }

    }

}
