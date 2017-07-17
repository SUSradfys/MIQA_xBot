using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;

namespace sendMail
{
    public class Program
    {
        static void Main(string[] args)
        {

        }

        public static void send(string recipient, string subject, string body, string sender, string domain, string smtp)
        {
            string pass = null;
            SmtpClient smtpClient = new SmtpClient();
            NetworkCredential basicCredentials = new NetworkCredential(sender, pass, smtp);
            MailMessage message = new MailMessage();
            MailAddress fromAdress = new MailAddress(sender + "@" + domain);

            // setup the host, increase the timeout to 5 minutes
            smtpClient.Host = smtp;
            smtpClient.Port = 25;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = basicCredentials;
            smtpClient.Timeout = (60 * 5 * 1000);

            message.From = fromAdress;
            message.Subject = subject + " - " + DateTime.Now.Date.ToString().Split(' ')[0];
            message.Body = body.Replace("\r\n", "<br>");
            message.IsBodyHtml = true;
            message.To.Add(recipient);

            smtpClient.Send(message);
        }
    }
}
