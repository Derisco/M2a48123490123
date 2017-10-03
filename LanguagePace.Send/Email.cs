using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Configuration;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LanguagePace.Send
{
    /// <summary>
    ///     Handles Email functions
    /// </summary>
    static public class Email
    {


        /// <summary>
        ///     Sends an email with the message string passed in.
        /// </summary>
        /// <returns>If message was sent or not</returns>
        static public Task<bool> Send(string to, string subject, string body, bool isHtml = true)
        {
            SmtpClient smtpClient = new SmtpClient(ConfigurationManager.AppSettings["SMTPServer"], Int32.Parse(ConfigurationManager.AppSettings["SMTPServerPort"]));

            smtpClient.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["EmailClientName"], ConfigurationManager.AppSettings["EmailClientPassword"]);

            smtpClient.UseDefaultCredentials = true;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.EnableSsl = true;

            MailMessage mail = new MailMessage();

            mail.From = new MailAddress(ConfigurationManager.AppSettings["EmailClientName"], "LanguagePace");
            mail.To.Add(new MailAddress(to));

            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = isHtml;

            try
            {
                smtpClient.Send(mail);
            }
            catch
            {
                // Try again after 3 seconds, incase it was just a network hicup
                Thread.Sleep(3000);
                try
                {
                    smtpClient.Send(mail);
                }
                catch (Exception e)
                {
                    if (ConfigurationManager.AppSettings["Debugging"] == "true")
                    {
                        Console.WriteLine("Email failed to send: ", e.Message);
                    }
                    return Task.FromResult(false);
                }
            }

            return Task.FromResult(true);
        }



    }
}
