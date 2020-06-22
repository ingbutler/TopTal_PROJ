using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DBRuns
{

    public class Utils
    {

        public static async Task<string> SendMailAsync(
            string host,
            int port,
            string user,
            string pwd,
            bool enableSsl,
            bool isBodyHtml,
            string subject,
            List<string> tos,
            List<string> ccs,
            List<string> bccs,
            string from,
            string sender,
            List<string> replyTos,
            string body,
            AlternateView alternateView,
            Attachment attachment,
            MailPriority? mailPriority
        )
        {
            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Host = host;
            smtpClient.Port = port;
            smtpClient.Credentials = new System.Net.NetworkCredential(user, pwd);
            smtpClient.EnableSsl = enableSsl;

            MailMessage mailMsg = new MailMessage();
            mailMsg.IsBodyHtml = isBodyHtml;
            mailMsg.Subject = subject;

            foreach (string t in tos)
                mailMsg.To.Add(new MailAddress(t));

            if (ccs != null)
            {
                foreach (string s in ccs)
                    mailMsg.CC.Add(s);
            }

            if (bccs != null)
            {
                foreach (string s in bccs)
                    mailMsg.Bcc.Add(s);
            }

            mailMsg.From = new MailAddress(from);

            if (sender != null)
                mailMsg.Sender = new MailAddress(sender);

            if (mailPriority != null)
                mailMsg.Priority = mailPriority.Value;
            else
                mailMsg.Priority = MailPriority.Normal;

            if (replyTos != null)
            {
                foreach (string rt in replyTos)
                    mailMsg.ReplyToList.Add(new MailAddress(rt));
            }

            if (alternateView != null)
                mailMsg.AlternateViews.Add(alternateView);
            else
                mailMsg.Body = body;

            if (attachment != null)
                mailMsg.Attachments.Add(attachment);

            string retValue = "";
            try
            {
                await smtpClient.SendMailAsync(mailMsg);
            }
            catch (Exception e)
            {
                retValue = e.Message + ((e.InnerException == null) ? "" : (" - " + e.InnerException.Message));
            }

            return retValue;
        }



        public static string GetMd5Hash(string input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
        }



        public static bool VerifyMd5Hash(string input, string hash)
        {
            // Verify a hash against a string.

            // Hash the input.
            string hashOfInput = GetMd5Hash(input);

            // Create a StringComparer and compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
                return true;
            else
                return false;
        }



        public static Guid GetUserId(ClaimsPrincipal user)
        {
            return new Guid(((ClaimsIdentity)user.Identity).FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
        }



        public static string GetUserRole(ClaimsPrincipal user)
        {
            return ((ClaimsIdentity)user.Identity).FindFirst(System.Security.Claims.ClaimTypes.Role).Value;
        }



        public static string ParseFilter(string filter, Type type, out List<string> parms)
        {
            const string ISCOLUMN = "isColumn";
            const string ISCOND = "isCond";
            const string ISPARM = "isParm";


            parms = new List<string>();


            if (string.IsNullOrEmpty(filter))
                return "";


            Dictionary<string, bool> columns = type.GetProperties().ToDictionary(x => x.Name.ToLower(), y => (y.PropertyType.Name == typeof(DateTime).Name));

            Dictionary<string, string> conds =
                new Dictionary<string, string>
                {
                    { " eq ", "=" },
                    { " ne ", "<>" },
                    { " lt ", "<" },
                    { " le ", "<=" },
                    { " gt ", ">" },
                    { " ge ", ">=" },
                };

            HashSet<string> notPars = new HashSet<string>();

            foreach (string col in columns.Keys)
                notPars.Add(col);

            foreach (string cond in conds.Keys)
                notPars.Add(cond);

            notPars.Add(" and ");
            notPars.Add(" or ");

            string pattern = @"(\(|\)";

            foreach (var notPar in notPars)
                pattern += "|" + notPar;

            pattern += ")";

            notPars.Add("(");
            notPars.Add(")");


            // SEGMENTING THE FILTER

            List<KeyValuePair<string, string>> filterSegments = new List<KeyValuePair<string, string>>();
            int start = 0;

            foreach (Match match in Regex.Matches(filter, pattern, RegexOptions.IgnoreCase))
            {
                if (match.Index != start)
                {
                    string other = filter.Substring(start, (match.Index - start)).Trim();
                    if (other != "")
                    {
                        // Removing single quotes
                        if (other[0] == '\'')
                            other = other.Substring(1);
                        if (other[other.Length - 1] == '\'')
                            other = other.Substring(0, other.Length - 1);

                        filterSegments.Add(new KeyValuePair<string, string>(other, ISPARM));
                    }
                }

                if (conds.ContainsKey(match.Value.ToLower()))
                    filterSegments.Add(new KeyValuePair<string, string>(match.Value.ToLower(), ISCOND));      // Filter segment is a conditional operator
                else if (columns.ContainsKey(match.Value.ToLower()))
                    filterSegments.Add(new KeyValuePair<string, string>(match.Value.ToLower(), ISCOLUMN));      // Filter segment is a column
                else
                    filterSegments.Add(new KeyValuePair<string, string>(match.Value, notPars.Contains(match.Value.ToLower()) ? "" : ISPARM));

                start = match.Index + match.Length;
            }

            if (filter.Length > (start))
            {
                //filterSegments.Add(new KeyValuePair<string, string>(filter.Substring(start, filter.Length - start).Trim(), ISPARM));

                string other = filter.Substring(start, filter.Length - start).Trim();
                if (other != "")
                {
                    // Removing single quotes
                    if (other[0] == '\'')
                        other = other.Substring(1);
                    if (other[other.Length - 1] == '\'')
                        other = other.Substring(0, other.Length - 1);

                    filterSegments.Add(new KeyValuePair<string, string>(other, ISPARM));
                }
            }


            string lastColumn = "";
            int lastColIndex = -1;

            for (int j = 0; j < filterSegments.Count; j++)
            {
                if (filterSegments[j].Value == ISCOLUMN)
                {
                    lastColumn = filterSegments[j].Key;
                    lastColIndex = j;
                }
                else if (filterSegments[j].Value == ISPARM)
                {
                    if (columns[lastColumn] && (filterSegments[j].Key.Length == 10))
                        // If the column has type "DateTime" and the length of the parameter is 10
                        filterSegments[lastColIndex] = new KeyValuePair<string, string>("convert(date, " + lastColumn + ")", ISCOLUMN);
                }
            }


            string newFilter = "";

            int i = -1;
            foreach (var sgm in filterSegments)
            {
                if (sgm.Value == ISPARM)
                {
                    i++;
                    newFilter += "{" + i.ToString() + "}";
                    parms.Add(sgm.Key);
                }
                else if (sgm.Value == ISCOND)
                    newFilter += conds[sgm.Key];
                else
                    newFilter += sgm.Key;

                newFilter += " ";
            }

            return newFilter;
        }

    }

}
