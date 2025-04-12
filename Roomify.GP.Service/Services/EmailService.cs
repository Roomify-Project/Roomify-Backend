using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using Roomify.GP.Core.Service.Contract;
using Roomify.GP.Core.Settings;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Roomify.GP.Service.Services
{
    public class EmailService : IEmailService
    {
        private readonly MailSettings _mailSettings;

        public EmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("Email address 'to' cannot be null or empty", nameof(to));

            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_mailSettings.From);
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_mailSettings.SmtpHost, _mailSettings.SmtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_mailSettings.Username, _mailSettings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
