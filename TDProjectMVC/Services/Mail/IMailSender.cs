namespace TDProjectMVC.Services.Mail
{
    public interface IMailSender
    {
        Task<bool> SendEmailAsync(string email, string subject, string message);
    }
}
//via5m8new @gmail.com