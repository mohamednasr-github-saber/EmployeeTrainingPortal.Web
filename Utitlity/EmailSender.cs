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
                Credentials = new NetworkCredential("mohamednasr123@gmail.com", "ilrc benb zjjh vqgw")
            };

            return client.SendMailAsync(
                new MailMessage("mohamednasr123@gmail.com", email, subject, htmlMessage)
                {
                    IsBodyHtml = true
                });
        }
    }
}
