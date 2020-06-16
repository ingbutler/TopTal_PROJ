using System;
using System.Collections.Generic;
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



        public static string ParseFilter(string filter, List<string> columns, out string[] parms)
        {
            Dictionary<string, string> conds =
                new Dictionary<string, string>
                {
                    { "eq", "=" },
                    { "ne", "<>" },
                    { "lt", "<" },
                    { "le", "<=" },
                    { "gt", ">" },
                    { "ge", ">" },
                };
            string pattern = @"(AND|OR|\(|\)|eq|lt|gt";

            foreach (string column in columns)
                pattern += "|" + column;

            pattern += ")";


            int start = 0;
            int end = 0;
            bool prevIsCondition = false;
            int i = 0;
            List<string> prms = new List<string>();
            string newFilter = "";

            foreach (Match match in Regex.Matches(filter, pattern, RegexOptions.IgnoreCase))
            {
                if (conds.ContainsKey(match.Value))
                {
                    // Beginning of a parameter value found
                    newFilter += filter.Substring(start, (match.Index - start)).TrimEnd() + " " + conds[match.Value];
                    start = match.Index + match.Value.Length;
                    prevIsCondition = true;
                }
                else if (prevIsCondition)
                {
                    i++;
                    end = match.Index;      // First match after condition: marks the end of a parameter value

                    AddParam(prms, filter, start, end);
                    newFilter += " {" + i.ToString() + "} " + match;
                    prevIsCondition = false;
                    start = end + match.Value.Length;
                }
            }

            if(prevIsCondition)
            {
                end = filter.Length;
                AddParam(prms, filter, start, end);
                newFilter += " {" + i.ToString() + "}";
            }
            else
                newFilter += filter.Substring(start, filter.Length - start);
            
            parms = prms.ToArray();

            return newFilter;
        }



        private static void AddParam(List<string> prms, string filter, int start, int end)
        {
            string prm = filter.Substring(start, end - start).Trim();
            
            // Removing single quotes
            if (prm[0] == '\'')
                prm = prm.Substring(1);
            if (prm[prm.Length - 1] == '\'')
                prm = prm.Substring(0, prm.Length - 1);

            prms.Add(prm);
        }

    }

}
