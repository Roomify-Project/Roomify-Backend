namespace Roomify.GP.Core.Service.Contract
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}
