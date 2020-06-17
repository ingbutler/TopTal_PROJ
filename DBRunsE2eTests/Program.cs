using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DBRuns.Models;
using Newtonsoft.Json;

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
            string queryString;
            string body;
            Process dbRuns = null;
            DateTime dateTimeMail;
            string VerificationLink;
            string contentStr;
            User user;

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


            //goto signIn;



            #region FIRST USER SIGNUP (ADMIN)

            Console.WriteLine("=====> FIRST USER SIGNING UP (ADMIN)");
            Console.WriteLine();

            dateTimeMail = DateTime.Now;

            body =
                @"
                    {
                        ""Email"":""" + Settings.Email + @""",
                        ""Password"":""" + Settings.Password + @"""
                    }
                ";
            await Utils.PostRequest("Users", "SignUp", null, body);

            Console.WriteLine("=====> FIRST USER SIGNED UP (ADMIN)");
            Console.WriteLine();


            Console.WriteLine("=====> VERIFYING FIRST USER'S MAIL");
            Console.WriteLine();

            VerificationLink = Utils.RetrieveVerificationLink(dateTimeMail);
            if(VerificationLink == "")
            {
                Console.WriteLine("No verification mail was found");
                return;
            }

            response = await Utils.GetRequest(VerificationLink, null);

            Console.WriteLine("=====> FIRST USER'S MAIL VERIFIED");
            Console.WriteLine();

            #endregion FIRST USER SIGNUP (ADMIN)




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




            #region ADMIN RETRIEVING THEIR OWN ACCOUNT

            Console.WriteLine("=====> ADMIN RETRIEVING THEIR OWN ACCOUNT");
            Console.WriteLine();

            queryString = "eMail=" + Settings.Email;
            response = await Utils.GetRequest("Users", "GetUserByEmail", GetBearerTokenHeader(response), queryString);
            contentStr = await response.Content.ReadAsStringAsync();
            user = JsonConvert.DeserializeObject<User>(contentStr);

            Console.WriteLine("=====> ADMIN RETRIEVED THEIR OWN ACCOUNT");
            Console.WriteLine();

            #endregion ADMIN RETRIEVING THEIR OWN ACCOUNT




            #region ADMIN SAVING THEIR OWN ACCOUNT WITH A DIFFERENT PASSWORD
        
            Console.WriteLine("=====> ADMIN SAVING THEIR OWN ACCOUNT WITH A DIFFERENT PASSWORD");
            Console.WriteLine();

            // To be able to create another user with same password...

            user.Email = "admin@toptal.com";
            body = JsonConvert.SerializeObject(user);

            response = await Utils.PutRequest("Users", null, user.Id.ToString(), GetBearerTokenHeader(response), body);

            Console.WriteLine("=====> ADMIN SAVED THEIR OWN ACCOUNT WITH A DIFFERENT PASSWORD");
            Console.WriteLine();

            #endregion ADMIN SAVING THEIR OWN ACCOUNT WITH A DIFFERENT PASSWORD




            #region NEW USER SIGNUP (ADMIN)

            Console.WriteLine("=====> NEW USER SIGNING UP (ADMIN)");
            Console.WriteLine();

            dateTimeMail = DateTime.Now;

            body =
                @"
                    {
                        ""Email"":""" + Settings.Email + @""",
                        ""Password"":""" + Settings.Password + @"""
                    }
                ";
            await Utils.PostRequest("Users", "SignUp", null, body);

            Console.WriteLine("=====> NEW USER SIGNED UP (ADMIN)");
            Console.WriteLine();


            Console.WriteLine("=====> VERIFYING NEW USER'S MAIL");
            Console.WriteLine();

            VerificationLink = Utils.RetrieveVerificationLink(dateTimeMail);
            if (VerificationLink == "")
            {
                Console.WriteLine("No verification mail was found");
                return;
            }

            response = await Utils.GetRequest(VerificationLink, null);

            Console.WriteLine("=====> NEW USER'S MAIL VERIFIED");
            Console.WriteLine();

            #endregion NEW USER SIGNUP (ADMIN)




            #region NEW USER FAILS SIGN-IN

            Console.WriteLine("=====> NEW USER FAILING SIGNIN");
            Console.WriteLine();

            body =
                @"
                    {
                        ""Email"":""" + Settings.Email + @""",
                        ""Password"":""WrongPwd""
                    }
                ";
            response = await Utils.PostRequest("Users", "SignIn", null, body);
            response = await Utils.PostRequest("Users", "SignIn", null, body);
            response = await Utils.PostRequest("Users", "SignIn", null, body);

            Console.WriteLine("=====> NEW USER FAILED SIGNIN FOR THREE TIMES");
            Console.WriteLine();

            #endregion NEW USER FAILS SIGN-IN




            #region NEW USER'S FURTHER SIGN-IN ATTEMPT

            Console.WriteLine("=====> NEW USER ATTEMPTING SIGNING IN AGAIN WITH CORRECT CREDENTIALS");
            Console.WriteLine();

            body =
                @"
                    {
                        ""Email"":""" + Settings.Email + @""",
                        ""Password"":""" + Settings.Password + @"""
                    }
                ";
            response = await Utils.PostRequest("Users", "SignIn", null, body);

            Console.WriteLine("=====> USER WAS BLOCKED");
            Console.WriteLine();

            #endregion NEW USER'S FURTHER SIGN-IN ATTEMPT




            #region ADMIN UNBLOCKING USER'S ACCOUNT

            Console.WriteLine("=====> ADMIN UNBLOCKING USER'S ACCOUNT");
            Console.WriteLine();

            queryString = "eMail=" + Settings.Email;
            response = await Utils.GetRequest("Users", "GetUserByEmail", GetBearerTokenHeader(response), queryString);
            contentStr = await response.Content.ReadAsStringAsync();
            user = JsonConvert.DeserializeObject<User>(contentStr);

            user.SignInFailCount = 0;
            body = JsonConvert.SerializeObject(user);

            response = await Utils.PutRequest("Users", null, user.Id.ToString(), GetBearerTokenHeader(response), body);

            Console.WriteLine("=====> ADMIN UNBLOCKED USER'S ACCOUNT");
            Console.WriteLine();

            #endregion ADMIN UNBLOCKING USER'S ACCOUNT




            #region NEW USER'S SUCCESSFUL SIGN-IN ATTEMPT

            Console.WriteLine("=====> NEW USER SIGNING IN SUCCESSFULLY");
            Console.WriteLine();

            body =
                @"
                    {
                        ""Email"":""" + Settings.Email + @""",
                        ""Password"":""" + Settings.Password + @"""
                    }
                ";
            response = await Utils.PostRequest("Users", "SignIn", null, body);

            Console.WriteLine("=====> NEW USER SIGNED IN SUCCESSFULLY");
            Console.WriteLine();

            #endregion NEW USER'S SUCCESSFUL SIGN-IN ATTEMPT




            #region ADMIN POSTING RUN

            Console.WriteLine("=====> ADMIN POSTING RUNS");
            Console.WriteLine();

            body =
                @"
                    {
                        ""Date"":""2020-06-14T19:06"",
                        ""Distance"":12000,
                        ""TimeRun"":3600,
                        ""Location"":""Campi Bisenzio,IT""
                    }
                ";
            response = await Utils.PostRequest("Runs", null, GetBearerTokenHeader(response), body);

            body =
                @"
                    {
                        ""Date"":""2020-06-15T12:07"",
                        ""Distance"":12000,
                        ""TimeRun"":3430,
                        ""Location"":""Campi Bisenzio,IT""
                    }
                ";
            response = await Utils.PostRequest("Runs", null, GetBearerTokenHeader(response), body);

            Console.WriteLine("=====> ADMIN POSTED RUNS");
            Console.WriteLine();

            #endregion ADMIN POSTING RUN




            #region LISTING RUNS + REPORT

            Console.WriteLine("=====> LISTING RUNS");
            Console.WriteLine();

            await Utils.GetRequest("Runs", null, GetBearerTokenHeader(response), null);

            Console.WriteLine("=====> RUNS LISTED");
            Console.WriteLine();

            Console.WriteLine("=====> RETRIEVING REPORT");
            Console.WriteLine();

            await Utils.GetRequest("Runs", "GetReport", GetBearerTokenHeader(response), null);

            Console.WriteLine("=====> REPORT RETRIEVED");
            Console.WriteLine();

            #endregion LISTING RUNS + REPORT




            #region ADMIN FILTERING USERS

            Console.WriteLine("=====> ADMIN FILTERING USERS (THEMSELVES)");
            Console.WriteLine();

            //string queryString = "filter=email eq '" + Settings.Email + "'&itemsPerPage=10&pageNumber=1";
            queryString = "filter=email eq '" + Settings.Email + "'";

            await Utils.GetRequest("Users", null, GetBearerTokenHeader(response), queryString);

            Console.WriteLine("=====> ADMIN FILTERED USERS (THEMSELVES)");
            Console.WriteLine();

            #endregion ADMIN FILTERING USERS






            if (!appDebug)
                dbRuns.Kill();
        }



        //private static List<KeyValuePair<string, string>> GetBearerTokenHeader(string token)
        //{
        //    return new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("authorization", "Bearer " + token) };
        //}



        private static List<KeyValuePair<string, string>> GetBearerTokenHeader(HttpResponseMessage response)
        {
            string token = response.Headers.GetValues("x-token").First();
            return new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("authorization", "Bearer " + token) };
        }

    }

}
