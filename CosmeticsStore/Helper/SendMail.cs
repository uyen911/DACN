using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Twilio.TwiML.Messaging;
using System.Net.Mail;
namespace CosmeticsStore.Helper
{
    public class SendMail
    {
        internal static void SendEmail(string email, string v)
        {
            throw new NotImplementedException();
        }

        internal static void SendEmail(string email, string v1, string v2)
        {
            throw new NotImplementedException();
        }

        public bool SendEmail(string to, string subject, string body, string attachFile)

        {
            try
            {
                MailMessage msg = new MailMessage("2105CT0027@dhv.edu.vn", to, subject, body);
                using (var client = new SmtpClient("", 0))
                {
                    client.EnableSsl = true;
                    if (!string.IsNullOrEmpty(attachFile)) { 
                        Attachment attachment = new Attachment(attachFile);
                        msg.Attachments.Add(attachment);
                    }

                    NetworkCredential credential = new NetworkCredential("2105CT0027@dhv.edu.vn", "P.uyen911");

                    client.UseDefaultCredentials = false;

                    client.Credentials = credential;

                    client.Send(msg);
                }
            }

            catch (Exception)
            {
                return false;
            }

            return true;
        }

    }
}



        






