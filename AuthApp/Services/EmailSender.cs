using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;


namespace AuthApp.Services
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender : IEmailSender
    {
        // IConfiguration defintion method
        public static IConfiguration UserConfig { get; set; }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            // define File to retrieve the Configuration
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            // Get the file building
            UserConfig = builder.Build();

            // Retrieve the string values - $"{}"
            string accEmail = $"{UserConfig["mailInformation:mailAccount"]}";
            string password = $"{UserConfig["mailInformation:mailPassword"]}";

            var loginInfo = new NetworkCredential(accEmail, password);
            var msg = new MailMessage();
            var smtpClient = new SmtpClient("smtp.gmail.com", 587);

            msg.From = new MailAddress(accEmail);
            msg.To.Add(new MailAddress(email));
            msg.Subject = subject;
            msg.Body = message;
            msg.IsBodyHtml = true;

            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = loginInfo;
            smtpClient.Send(msg);

            return Task.CompletedTask;
        }
    }
}
