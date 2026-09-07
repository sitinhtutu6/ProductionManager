using Microsoft.AspNetCore.Identity.UI.Services;
using ProductionManager.Data;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace ProductionApp.Web.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        private readonly ServerConfig _svconfig;
        private static readonly string KeyString = "ProductionApp_SecretKey_2026_@#$";

        public EmailSender(IConfiguration config, ServerConfig svconfig)
        {
            _config = config;
            _svconfig = svconfig;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Lấy cấu hình từ appsettings.json
            //string fromMail = "sitinhtutu6@gmail.com"; // Thay bằng email của bạn
            //string fromPassword = "apxy qgjp tcfn ugfd"; // Thay bằng App Password của Google

            string fromMail = _svconfig.SenderEmail;
            string fromPassword = _svconfig.SenderPassword;

            var message = new MailMessage();
            message.From = new MailAddress(fromMail);
            message.Subject = subject;
            message.To.Add(new MailAddress(email));
            message.Body = "<html><body>" + htmlMessage + "</body></html>";
            message.IsBodyHtml = true;

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromMail, fromPassword),
                EnableSsl = true,
            };

            await smtpClient.SendMailAsync(message);
        }
        
    }
}