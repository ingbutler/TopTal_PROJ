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
using Microsoft.Extensions.Diagnostics.HealthChecks;
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
        public const string ManagerDefaultPassword = "mngrDefPwd";

        public const string AdminEmail = "admin@toptal.com";
        public const string ManagerEmail = "manager@toptal.com";
        public const string SecondUserEmail = "second@toptal.com";
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


            goto resumeHere;


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
            response = await Utils.PostRequest("Users", "SignUp", null, body);

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

            user.Email = Settings.AdminEmail;
            body = JsonConvert.SerializeObject(user);

            response = await Utils.PutRequest("Users", null, user.Id.ToString(), GetBearerTokenHeader(response), body);

            Console.WriteLine("=====> ADMIN SAVED THEIR OWN ACCOUNT WITH A DIFFERENT PASSWORD");
            Console.WriteLine();

            #endregion ADMIN SAVING THEIR OWN ACCOUNT WITH A DIFFERENT PASSWORD



resumeHere:
            #region NEW USER SIGNUP

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
            response = await Utils.PostRequest("Users", "SignUp", null, body);

            Console.WriteLine("=====> NEW USER SIGNED UP");
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

            #endregion NEW USER SIGNUP




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




            #region ADMIN SIGN-IN

            Console.WriteLine("=====> ADMIN SIGNING IN");
            Console.WriteLine();

            body =
                @"
                    {
                        ""Email"":""" + Settings.AdminEmail + @""",
                        ""Password"":""" + Settings.Password + @"""
                    }
                ";
            response = await Utils.PostRequest("Users", "SignIn", null, body);

            Console.WriteLine("=====> ADMIN SIGNED IN");
            Console.WriteLine();

            #endregion ADMIN SIGN-IN




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
return;



            #region ADMIN SIGN-IN

            Console.WriteLine("=====> ADMIN SIGNING IN");
            Console.WriteLine();

            body =
                @"
                    {
                        ""Email"":""" + Settings.AdminEmail + @""",
                        ""Password"":""" + Settings.Password + @"""
                    }
                ";
            response = await Utils.PostRequest("Users", "SignIn", null, body);

            Console.WriteLine("=====> ADMIN SIGNED IN");
            Console.WriteLine();

            #endregion ADMIN SIGN-IN




            #region ADMIN ADDING FIRST RUN TO FIRST USER

            Console.WriteLine("=====> ADMIN ADDING FIRST RUN TO FIRST USER");
            Console.WriteLine();

            queryString = "eMail=" + Settings.Email;
            response = await Utils.GetRequest("Users", "GetUserByEmail", GetBearerTokenHeader(response), queryString);
            contentStr = await response.Content.ReadAsStringAsync();
            user = JsonConvert.DeserializeObject<User>(contentStr);

            body =
                @"
                    {
                        ""Date"":""2020-06-01T19:06"",
                        ""Distance"":5800,
                        ""TimeRun"":1480,
                        ""Location"":""Sesto Fiorentino,IT""
                    }
                ";
            queryString = "userId=" + user.Id.ToString();
            response = await Utils.PostRequest("Runs", null, null, queryString, GetBearerTokenHeader(response), body);

            Console.WriteLine("=====> ADMIN ADDED FIRST RUN TO FIRST USER");
            Console.WriteLine();

        #endregion ADMIN ADDING FIRST RUN TO FIRST USER




            #region ADMIN LISTING FIRST USER'S RUNS

            Console.WriteLine("=====> ADMIN LISTING FIRST USER'S RUNS");
            Console.WriteLine();

            queryString = "eMail=" + Settings.Email;
            response = await Utils.GetRequest("Users", "GetUserByEmail", GetBearerTokenHeader(response), queryString);
            contentStr = await response.Content.ReadAsStringAsync();
            user = JsonConvert.DeserializeObject<User>(contentStr);

            queryString = "filter=userId eq '" + user.Id + "'";
            response = await Utils.GetRequest("Runs", null, GetBearerTokenHeader(response), queryString);

            Console.WriteLine("=====> ADMIN LISTED FIRST USER'S RUNS");
            Console.WriteLine();

        #endregion ADMIN LISTING FIRST USER'S RUNS




            #region ADMIN CREATING MANAGER

            Console.WriteLine("=====> ADMIN CREATING MANAGER");
            Console.WriteLine();

            body =
                @"
                    {
                        ""Email"":""" + Settings.ManagerEmail + @""",
                        ""Password"":""" + Settings.ManagerDefaultPassword + @""",
                        ""Role"":""MANAGER"",
                        ""IsVerified"":true,
                        ""SignInFailCount"":0
                    }
                ";
            response = await Utils.PostRequest("Users", null, GetBearerTokenHeader(response), body);

            Console.WriteLine("=====> ADMIN CREATED MANAGER");
            Console.WriteLine();

            #endregion ADMIN CREATING MANAGER




            #region MANAGER CHANGING THEIR OWN PASSWORD

            Console.WriteLine("=====> MANAGER CHANGING THEIR OWN PASSWORD");
            Console.WriteLine();

            body =
                @"
                    {
                        ""Email"":""" + Settings.ManagerEmail + @""",
                        ""Password"":""" + Settings.ManagerDefaultPassword + @""",
                        ""NewPassword"":""" + Settings.Password + @"""
                    }
                ";
            response = await Utils.PostRequest("Users", "ChangePassword", null, body);

            Console.WriteLine("=====> MANAGER CHANGED THEIR OWN PASSWORD");
            Console.WriteLine();

        #endregion MANAGER CHANGING THEIR OWN PASSWORD




            #region MANAGER SIGN-IN

            Console.WriteLine("=====> MANAGER SIGNING IN");
            Console.WriteLine();

            body =
                @"
                    {
                        ""Email"":""" + Settings.ManagerEmail + @""",
                        ""Password"":""" + Settings.Password + @"""
                    }
                ";
            response = await Utils.PostRequest("Users", "SignIn", null, body);

            Console.WriteLine("=====> MANAGER SIGNED IN");
            Console.WriteLine();

            #endregion MANAGER SIGN-IN




            #region MANAGER CREATING SECOND USER

            Console.WriteLine("=====> MANAGER CREATING SECOND USER");
            Console.WriteLine();

            body =
                @"
                    {
                        ""Email"":""" + Settings.SecondUserEmail + @""",
                        ""Password"":""" + Settings.Password + @""",
                        ""Role"":""USER"",
                        ""IsVerified"":false,
                        ""SignInFailCount"":0
                    }
                ";
            response = await Utils.PostRequest("Users", null, GetBearerTokenHeader(response), body);

            Console.WriteLine("=====> MANAGER CREATED SECOND USER");
            Console.WriteLine();

            #endregion MANAGER CREATING SECOND USER




            #region MANAGER UPDATING SECOND USER (setting IsVerified)

            queryString = "eMail=" + Settings.SecondUserEmail;
            response = await Utils.GetRequest("Users", "GetUserByEmail", GetBearerTokenHeader(response), queryString);

            Console.WriteLine("=====> MANAGER UPDATING SECOND USER (setting IsVerified)");
            Console.WriteLine();

            contentStr = await response.Content.ReadAsStringAsync();
            user = JsonConvert.DeserializeObject<User>(contentStr);

            user.IsVerified = true;
            body = JsonConvert.SerializeObject(user);

            response = await Utils.PutRequest("Users", null, user.Id.ToString(), GetBearerTokenHeader(response), body);

            Console.WriteLine("=====> MANAGER UPDATED SECOND USER");
            Console.WriteLine();

            // Reading updated record
            queryString = "eMail=" + Settings.SecondUserEmail;
            response = await Utils.GetRequest("Users", "GetUserByEmail", GetBearerTokenHeader(response), queryString);

            Console.WriteLine();
            Console.WriteLine();

        #endregion MANAGER UPDATING SECOND USER (setting IsVerified)




            #region SECOND USER SIGN-IN

            Console.WriteLine("=====> SECOND USER SIGNING IN");
            Console.WriteLine();

            body =
                @"
                    {
                        ""Email"":""" + Settings.SecondUserEmail + @""",
                        ""Password"":""" + Settings.Password + @"""
                    }
                ";
            response = await Utils.PostRequest("Users", "SignIn", null, body);

            Console.WriteLine("=====> SECOND USER SIGNED IN");
            Console.WriteLine();

            #endregion SECOND USER SIGN-IN




            #region SECOND USER POSTING THEIR OWN RUNS

            Console.WriteLine("=====> SECOND USER POSTING THEIR OWN RUNS");
            Console.WriteLine();

            body =
                @"
                    {
                        ""Date"":""2020-06-11T19:06"",
                        ""Distance"":7600,
                        ""TimeRun"":1520,
                        ""Location"":""Firenze,IT""
                    }
                ";
            response = await Utils.PostRequest("Runs", null, GetBearerTokenHeader(response), body);

            body =
                @"
                    {
                        ""Date"":""2020-06-12T19:06"",
                        ""Distance"":12500,
                        ""TimeRun"":3640,
                        ""Location"":""Firenze,IT""
                    }
                ";
            response = await Utils.PostRequest("Runs", null, GetBearerTokenHeader(response), body);

            body =
                @"
                    {
                        ""Date"":""2020-06-14T19:06"",
                        ""Distance"":12700,
                        ""TimeRun"":3600,
                        ""Location"":""Firenze,IT""
                    }
                ";
            response = await Utils.PostRequest("Runs", null, GetBearerTokenHeader(response), body);

            body =
                @"
                    {
                        ""Date"":""2020-06-15T12:07"",
                        ""Distance"":13200,
                        ""TimeRun"":3430,
                        ""Location"":""Firenze,IT""
                    }
                ";
            response = await Utils.PostRequest("Runs", null, GetBearerTokenHeader(response), body);

            body =
                @"
                    {
                        ""Date"":""2020-06-18T19:06"",
                        ""Distance"":16200,
                        ""TimeRun"":5780,
                        ""Location"":""Firenze,IT""
                    }
                ";
            response = await Utils.PostRequest("Runs", null, GetBearerTokenHeader(response), body);

            Console.WriteLine("=====> SECOND USER POSTED THEIR OWN RUNS");
            Console.WriteLine();

            response = await Utils.GetRequest("Runs", null, GetBearerTokenHeader(response), null);
            Console.WriteLine();
            Console.WriteLine();

        #endregion SECOND USER POSTING THEIR OWN RUNS




            #region FIRST USER SIGN-IN

            Console.WriteLine("=====> FIRST USER SIGNING IN");
            Console.WriteLine();

            body =
                @"
                    {
                        ""Email"":""" + Settings.Email + @""",
                        ""Password"":""" + Settings.Password + @"""
                    }
                ";
            response = await Utils.PostRequest("Users", "SignIn", null, body);

            Console.WriteLine("=====> FIRST USER SIGNED IN");
            Console.WriteLine();

            #endregion FIRST USER SIGN-IN




            #region FIRST USER POSTING RUNS

            Console.WriteLine("=====> FIRST USER POSTING RUNS");
            Console.WriteLine();

            body =
                @"
                    {
                        ""Date"":""2020-06-11T19:06"",
                        ""Distance"":6000,
                        ""TimeRun"":1520,
                        ""Location"":""Sesto Fiorentino,IT""
                    }
                ";
            response = await Utils.PostRequest("Runs", null, GetBearerTokenHeader(response), body);

            body =
                @"
                    {
                        ""Date"":""2020-06-12T19:06"",
                        ""Distance"":12000,
                        ""TimeRun"":3640,
                        ""Location"":""Campi Bisenzio,IT""
                    }
                ";
            response = await Utils.PostRequest("Runs", null, GetBearerTokenHeader(response), body);

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

            body =
                @"
                    {
                        ""Date"":""2020-06-18T19:06"",
                        ""Distance"":15000,
                        ""TimeRun"":5780,
                        ""Location"":""Campi Bisenzio,IT""
                    }
                ";
            response = await Utils.PostRequest("Runs", null, GetBearerTokenHeader(response), body);

            Console.WriteLine("=====> FIRST USER POSTED RUNS");
            Console.WriteLine();

            response = await Utils.GetRequest("Runs", null, GetBearerTokenHeader(response), null);
            Console.WriteLine();
            Console.WriteLine();

            #endregion FIRST USER POSTING RUNS




            #region FIRST USER FILTERING RUNS + REPORT

            Console.WriteLine("=====> FIRST USER FILTERING RUNS");
            Console.WriteLine();

            queryString = "filter=(location eq 'Sesto Fiorentino,IT' OR (Date ge '2020-06-14' and DATE le '2020-06-18')) and timerun ne 3430";
            response = await Utils.GetRequest("Runs", null, GetBearerTokenHeader(response), queryString);

            Console.WriteLine("=====> FIRST USER FILTERED RUNS");
            Console.WriteLine();

            Console.WriteLine("=====> RETRIEVING REPORT FOR YEAR 2019");
            Console.WriteLine();

            queryString = "year=2019";
            response = await Utils.GetRequest("Runs", "GetReport", GetBearerTokenHeader(response), queryString);

            Console.WriteLine("=====> REPORT RETRIEVED FOR YEAR 2019");
            Console.WriteLine();

            Console.WriteLine("=====> RETRIEVING REPORT FOR YEAR 2020 PAGE 1");
            Console.WriteLine();

            queryString = "year=2020&itemsperpage=1&pagenumber=1";
            response = await Utils.GetRequest("Runs", "GetReport", GetBearerTokenHeader(response), queryString);

            Console.WriteLine("=====> REPORT RETRIEVED FOR YEAR 2020 PAGE 1");
            Console.WriteLine();

            Console.WriteLine("=====> RETRIEVING REPORT FOR YEAR 2020 PAGE 2");
            Console.WriteLine();

            queryString = "year=2020&itemsperpage=1&pagenumber=2";
            response = await Utils.GetRequest("Runs", "GetReport", GetBearerTokenHeader(response), queryString);

            Console.WriteLine("=====> REPORT RETRIEVED FOR YEAR 2020 PAGE 2");
            Console.WriteLine();

        #endregion FIRST USER LISTING RUNS + REPORT




            #region MANAGER SIGN-IN

            Console.WriteLine("=====> MANAGER SIGNING IN");
            Console.WriteLine();

            body =
                @"
                    {
                        ""Email"":""" + Settings.ManagerEmail + @""",
                        ""Password"":""" + Settings.Password + @"""
                    }
                ";
            response = await Utils.PostRequest("Users", "SignIn", null, body);

            Console.WriteLine("=====> MANAGER SIGNED IN");
            Console.WriteLine();

            #endregion MANAGER SIGN-IN




            #region MANAGER DELETING SECOND USER AND ALL THEIR RUNS

            Console.WriteLine("=====> MANAGER DELETING SECOND USER AND ALL THEIR RUNS");
            Console.WriteLine();

            queryString = "eMail=" + Settings.SecondUserEmail;
            response = await Utils.GetRequest("Users", "GetUserByEmail", GetBearerTokenHeader(response), queryString);
            contentStr = await response.Content.ReadAsStringAsync();
            user = JsonConvert.DeserializeObject<User>(contentStr);
            if (user == null)
            {
                Console.WriteLine("=====> SECOND USER DOES NOT EXIST");
                Console.WriteLine();
            }
            else
            {
                response = await Utils.DeleteRequest("Users", null, user.Id.ToString(), GetBearerTokenHeader(response));
                Console.WriteLine("=====> MANAGER DELETED SECOND USER AND ALL THEIR RUNS");
                Console.WriteLine();
            }

            queryString = "eMail=" + Settings.SecondUserEmail;
            response = await Utils.GetRequest("Users", "GetUserByEmail", GetBearerTokenHeader(response), queryString);
            contentStr = await response.Content.ReadAsStringAsync();
            user = JsonConvert.DeserializeObject<User>(contentStr);
            if (user == null)
            {
                Console.WriteLine("=====> SECOND USER NO LONGER EXISTS");
                Console.WriteLine();
            }

            #endregion MANAGER DELETING SECOND USER




            #region MANAGER POSTING THEIR OWN RUNS

            Console.WriteLine("=====> MANAGER POSTING THEIR OWN RUNS");
            Console.WriteLine();

            body =
                @"
                    {
                        ""Date"":""2020-06-11T19:06"",
                        ""Distance"":6000,
                        ""TimeRun"":1520,
                        ""Location"":""Scandicci,IT""
                    }
                ";
            response = await Utils.PostRequest("Runs", null, GetBearerTokenHeader(response), body);

            body =
                @"
                    {
                        ""Date"":""2020-06-12T19:06"",
                        ""Distance"":12000,
                        ""TimeRun"":3640,
                        ""Location"":""Scandicci,IT""
                    }
                ";
            response = await Utils.PostRequest("Runs", null, GetBearerTokenHeader(response), body);

            body =
                @"
                    {
                        ""Date"":""2020-06-14T19:06"",
                        ""Distance"":12000,
                        ""TimeRun"":3600,
                        ""Location"":""Scandicci,IT""
                    }
                ";
            response = await Utils.PostRequest("Runs", null, GetBearerTokenHeader(response), body);

            body =
                @"
                    {
                        ""Date"":""2020-06-15T12:07"",
                        ""Distance"":12000,
                        ""TimeRun"":3430,
                        ""Location"":""Scandicci,IT""
                    }
                ";
            response = await Utils.PostRequest("Runs", null, GetBearerTokenHeader(response), body);

            body =
                @"
                    {
                        ""Date"":""2020-06-18T19:06"",
                        ""Distance"":15000,
                        ""TimeRun"":5780,
                        ""Location"":""Scandicci,IT""
                    }
                ";
            response = await Utils.PostRequest("Runs", null, GetBearerTokenHeader(response), body);

            Console.WriteLine("=====> MANAGER POSTED THEIR OWN RUNS");
            Console.WriteLine();

            response = await Utils.GetRequest("Runs", null, GetBearerTokenHeader(response), null);
            Console.WriteLine();
            Console.WriteLine();

        #endregion MANAGER POSTING THEIR OWN RUNS




            #region ADMIN SIGN-IN

            Console.WriteLine("=====> ADMIN SIGNING IN");
            Console.WriteLine();

            body =
                @"
                    {
                        ""Email"":""" + Settings.AdminEmail + @""",
                        ""Password"":""" + Settings.Password + @"""
                    }
                ";
            response = await Utils.PostRequest("Users", "SignIn", null, body);

            Console.WriteLine("=====> ADMIN SIGNED IN");
            Console.WriteLine();

            #endregion ADMIN SIGN-IN




            #region ADMIN VIEWING MANAGER'S REPORT

            Console.WriteLine("=====> ADMIN VIEWING MANAGER'S REPORT");
            Console.WriteLine();

            queryString = "eMail=" + Settings.ManagerEmail;
            response = await Utils.GetRequest("Users", "GetUserByEmail", GetBearerTokenHeader(response), queryString);
            contentStr = await response.Content.ReadAsStringAsync();
            user = JsonConvert.DeserializeObject<User>(contentStr);

            queryString = "year=2020&userId=" + user.Id.ToString();
            response = await Utils.GetRequest("Runs", "GetReport", GetBearerTokenHeader(response), queryString);

            Console.WriteLine("=====> ADMIN VIEWED MANAGER'S REPORT");
            Console.WriteLine();

            #endregion ADMIN VIEWING USER'S REPORT




            #region ADMIN DOWNGRADING MANAGER'S PERFORMANCE

            Console.WriteLine("=====> ADMIN DOWNGRADING MANAGER'S PERFORMANCE");
            Console.WriteLine();

            queryString = "eMail=" + Settings.ManagerEmail;
            response = await Utils.GetRequest("Users", "GetUserByEmail", GetBearerTokenHeader(response), queryString);
            contentStr = await response.Content.ReadAsStringAsync();
            user = JsonConvert.DeserializeObject<User>(contentStr);

            queryString = "filter=userId eq '" + user.Id + "'";
            response = await Utils.GetRequest("Runs", null, GetBearerTokenHeader(response), queryString);
            contentStr = await response.Content.ReadAsStringAsync();
            ItemList<Run> ilr = JsonConvert.DeserializeObject<ItemList<Run>>(contentStr);
            // Suppose no more than one page of Runs
            foreach (Run run in ilr.items)
            {
                run.TimeRun = run.TimeRun * 2;
                body = JsonConvert.SerializeObject(run);
                response = await Utils.PutRequest("Runs", null, run.Id.ToString(), GetBearerTokenHeader(response), body);
            }

            Console.WriteLine("=====> ADMIN DOWNGRADED MANAGER'S PERFORMANCE");
            Console.WriteLine();

            #endregion ADMIN DOWNGRADING MANAGER'S PERFORMANCE




            #region ADMIN VIEWING MANAGER'S REPORT

            Console.WriteLine("=====> ADMIN VIEWING MANAGER'S REPORT");
            Console.WriteLine();

            queryString = "eMail=" + Settings.ManagerEmail;
            response = await Utils.GetRequest("Users", "GetUserByEmail", GetBearerTokenHeader(response), queryString);
            contentStr = await response.Content.ReadAsStringAsync();
            user = JsonConvert.DeserializeObject<User>(contentStr);

            queryString = "year=2020&userId=" + user.Id.ToString();
            response = await Utils.GetRequest("Runs", "GetReport", GetBearerTokenHeader(response), queryString);

            Console.WriteLine("=====> ADMIN VIEWED MANAGER'S REPORT");
            Console.WriteLine();

            #endregion ADMIN VIEWING USER'S REPORT




            #region ADMIN DELETING MANAGER'S FIRST RUN

            Console.WriteLine("=====> ADMIN DELETING MANAGER'S FIRST RUN");
            Console.WriteLine();

            queryString = "eMail=" + Settings.ManagerEmail;
            response = await Utils.GetRequest("Users", "GetUserByEmail", GetBearerTokenHeader(response), queryString);
            contentStr = await response.Content.ReadAsStringAsync();
            user = JsonConvert.DeserializeObject<User>(contentStr);

            queryString = "filter=userId eq '" + user.Id + "'";
            response = await Utils.GetRequest("Runs", null, GetBearerTokenHeader(response), queryString);
            contentStr = await response.Content.ReadAsStringAsync();
            Run firstRun = JsonConvert.DeserializeObject<ItemList<Run>>(contentStr).items.FirstOrDefault();
            response = await Utils.DeleteRequest("Runs", null, firstRun.Id.ToString(), GetBearerTokenHeader(response));

            Console.WriteLine("=====> ADMIN DELETING MANAGER'S FIRST RUN");
            Console.WriteLine();

            #endregion ADMIN DELETING MANAGER'S FIRST RUN




            #region ADMIN VIEWING MANAGER'S REPORT

            Console.WriteLine("=====> ADMIN VIEWING MANAGER'S REPORT");
            Console.WriteLine();

            queryString = "eMail=" + Settings.ManagerEmail;
            response = await Utils.GetRequest("Users", "GetUserByEmail", GetBearerTokenHeader(response), queryString);
            contentStr = await response.Content.ReadAsStringAsync();
            user = JsonConvert.DeserializeObject<User>(contentStr);

            queryString = "year=2020&userId=" + user.Id.ToString();
            response = await Utils.GetRequest("Runs", "GetReport", GetBearerTokenHeader(response), queryString);

            Console.WriteLine("=====> ADMIN VIEWED MANAGER'S REPORT");
            Console.WriteLine();

            #endregion ADMIN VIEWING USER'S REPORT




            #region ADMIN BULK DELETING MANAGER'S REMAINING RUNS

            Console.WriteLine("=====> ADMIN BULK DELETING MANAGER'S REMAINING RUNS");
            Console.WriteLine();

            queryString = "eMail=" + Settings.ManagerEmail;
            response = await Utils.GetRequest("Users", "GetUserByEmail", GetBearerTokenHeader(response), queryString);
            contentStr = await response.Content.ReadAsStringAsync();
            user = JsonConvert.DeserializeObject<User>(contentStr);

            response = await Utils.DeleteRequest("Runs", "DeleteByUser", user.Id.ToString(), GetBearerTokenHeader(response));

            Console.WriteLine("=====> ADMIN BULK DELETING MANAGER'S REMAINING RUNS");
            Console.WriteLine();

            #endregion ADMIN BULK DELETING MANAGER'S REMAINING RUNS




            #region ADMIN VIEWING MANAGER'S REPORT

            Console.WriteLine("=====> ADMIN VIEWING MANAGER'S REPORT");
            Console.WriteLine();

            queryString = "eMail=" + Settings.ManagerEmail;
            response = await Utils.GetRequest("Users", "GetUserByEmail", GetBearerTokenHeader(response), queryString);
            contentStr = await response.Content.ReadAsStringAsync();
            user = JsonConvert.DeserializeObject<User>(contentStr);

            queryString = "year=2020&userId=" + user.Id.ToString();
            response = await Utils.GetRequest("Runs", "GetReport", GetBearerTokenHeader(response), queryString);

            Console.WriteLine("=====> ADMIN VIEWED MANAGER'S REPORT");
            Console.WriteLine();

            #endregion ADMIN VIEWING USER'S REPORT




            if (!appDebug)
                dbRuns.Kill();
        }




        private static List<KeyValuePair<string, string>> GetBearerTokenHeader(HttpResponseMessage response)
        {
            string token = response.Headers.GetValues("x-token").First();
            return new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("authorization", "Bearer " + token) };
        }

    }

}
