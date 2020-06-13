using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Linq;
using System.Threading.Tasks;

namespace DBRuns
{

    public struct Roles
    {
        public const string ADMIN = "ADMIN";
        public const string MANAGER = "MANAGER";
        public const string USER = "USER";
    }




    public class BizLogic
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

    }

}
