using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace TDProjectMVC.Services.Mail
{
    public class MailSender : IMailSender
    {
        public async Task<bool> SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                var client = new SmtpClient("smtp.gmail.com", 587)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("via5m8new@gmail.com", "fffdviqanzsonubo"),
                    Timeout = 10000 // 10 seconds timeout
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("via5m8new@gmail.com"),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception)
            {
                // Log the exception here
                return false;
            }
        }
    }
}