using CityChoir.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace CityChoir.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var emailSettings = _config.GetSection("Email");

        var fromName = emailSettings["FromName"];
        var fromEmail = emailSettings["FromEmail"];
        var host = emailSettings["SmtpServer"];
        var port = int.Parse(emailSettings["SmtpPort"] ?? "587");
        var username = emailSettings["Username"];
        var password = emailSettings["Password"];
        var enableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "false");

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            throw new InvalidOperationException("Email configuration is incomplete.");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        message.Body = new BodyBuilder
        {
            HtmlBody = body
        }.ToMessageBody();

        try
        {
            using var smtp = new SmtpClient();

            //  DEV ONLY — remove in production when SSL is properly configured
            smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

            var secureOption = enableSsl
                ? SecureSocketOptions.SslOnConnect   // port 465
                : SecureSocketOptions.StartTls;     // port 587

            await smtp.ConnectAsync(host, port, secureOption);
            await smtp.AuthenticateAsync(username, password);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);

            Console.WriteLine($"Email sent to {to}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Email sending failed");
            Console.WriteLine(ex.Message);

            if (ex.InnerException != null)
                Console.WriteLine($"Inner: {ex.InnerException.Message}");

            LogFallbackEmail(to, subject, body);
        }
    }

    private void LogFallbackEmail(string to, string subject, string body)
    {
        Console.WriteLine("\n========== EMAIL FALLBACK ==========");
        Console.WriteLine($"To: {to}");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine("Body:");
        Console.WriteLine(body);
        Console.WriteLine("====================================\n");
    }
}