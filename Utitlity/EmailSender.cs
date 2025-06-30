using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace EmployeeTrainingPortal.Utility
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("mohamedcalosha4@gmail.com", "msdzmeybggllsqeb")
            };

            return client.SendMailAsync(
                new MailMessage("mohamedcalosha4@gmail.com", email, subject, htmlMessage)
                {
                    IsBodyHtml = true
                });
        }
    }
}
