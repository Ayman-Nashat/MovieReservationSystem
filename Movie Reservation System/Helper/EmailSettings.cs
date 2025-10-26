using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using Movie_Reservation_System.Settings;
using MovieReservationSystem.Core.Entities;
using MovieReservationSystem.Core.Service.Contract;

namespace Movie_Reservation_System.Helper
{
    public class EmailSettings : IMailService
    {
        private readonly MailSettings _options;
        public EmailSettings(IOptions<MailSettings> options)
        {
            _options = options.Value;
        }
        public void SendEmail(Email email)
        {
            // Validate input parameters
            if (email == null)
                throw new ArgumentNullException(nameof(email), "Email object cannot be null");

            var mail = new MimeMessage
            {
                Sender = MailboxAddress.Parse(_options.Email),
                Subject = email.Subject
            };

            mail.To.Add(MailboxAddress.Parse(email.To));
            mail.From.Add(new MailboxAddress(_options.DisplayName, _options.Email));

            var builder = new BodyBuilder();
            builder.TextBody = email.Body;

            mail.Body = builder.ToMessageBody();

            //establish connection

            using var smtp = new SmtpClient();

            smtp.Connect(_options.Host, _options.Port, MailKit.Security.SecureSocketOptions.StartTls);

            smtp.Authenticate(_options.Email, _options.Password);

            smtp.Send(mail);

            smtp.Disconnect(true);
        }
    }
}
