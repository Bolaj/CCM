using CityChoir.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CityChoir.Infrastructure.Services;

public class EmailService : IEmailService
{
    private static readonly HttpClient HttpClient = new();
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<bool> TestConnectionAsync()
    {
        var emailSettings = _config.GetSection("Email");

        if (IsResendConfigured(emailSettings))
            return await TestResendConnectionAsync(emailSettings);

        var host = emailSettings["SmtpServer"];
        var port = int.Parse(emailSettings["SmtpPort"] ?? "587");
        var username = emailSettings["Username"];
        var enableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "false");

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username))
        {
            Console.WriteLine("SMTP configuration is incomplete. Set Email:Username and Email:AccessToken.");
            return false;
        }

        try
        {
            using var smtp = new SmtpClient();

            var secureOption = enableSsl
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTls;

            await smtp.ConnectAsync(host, port, secureOption);
            await AuthenticateAsync(smtp, emailSettings);
            await smtp.DisconnectAsync(true);

            Console.WriteLine($"SMTP connection successful to {host}:{port}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("SMTP connection failed");
            Console.WriteLine(ex.Message);

            if (ex.InnerException != null)
                Console.WriteLine($"Inner: {ex.InnerException.Message}");

            return false;
        }
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var emailSettings = _config.GetSection("Email");

        if (IsResendConfigured(emailSettings))
        {
            await SendWithResendAsync(emailSettings, to, subject, body);
            return;
        }

        var fromName = emailSettings["FromName"];
        var fromEmail = emailSettings["FromEmail"];
        var host = emailSettings["SmtpServer"];
        var port = int.Parse(emailSettings["SmtpPort"] ?? "587");
        var username = emailSettings["Username"];
        var enableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "false");

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(fromEmail))
            throw new InvalidOperationException("Email configuration is incomplete. Set Email:Username and Email:FromEmail.");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail!));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        message.Body = new BodyBuilder
        {
            HtmlBody = body
        }.ToMessageBody();

        try
        {
            using var smtp = new SmtpClient();

            var secureOption = enableSsl
                ? SecureSocketOptions.SslOnConnect   // port 465
                : SecureSocketOptions.StartTls;     // port 587

            await smtp.ConnectAsync(host, port, secureOption);
            await AuthenticateAsync(smtp, emailSettings);
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

    private bool IsResendConfigured(IConfigurationSection emailSettings) =>
        string.Equals(emailSettings["Provider"], "Resend", StringComparison.OrdinalIgnoreCase);

    private Task<bool> TestResendConnectionAsync(IConfigurationSection emailSettings)
    {
        var apiKey = emailSettings["ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.WriteLine("Resend configuration is incomplete. Set Email:ApiKey.");
            return Task.FromResult(false);
        }

        Console.WriteLine("Resend sending configuration found. The API key will be validated when an email is sent.");
        return Task.FromResult(true);
    }

    private async Task SendWithResendAsync(
        IConfigurationSection emailSettings,
        string to,
        string subject,
        string body)
    {
        var apiKey = emailSettings["ApiKey"];
        var fromEmail = emailSettings["FromEmail"];
        var fromName = emailSettings["FromName"];

        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(fromEmail))
            throw new InvalidOperationException("Resend configuration is incomplete. Set Email:ApiKey and Email:FromEmail.");

        var from = string.IsNullOrWhiteSpace(fromName)
            ? fromEmail
            : $"{fromName} <{fromEmail}>";

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(new
        {
            from,
            to = new[] { to },
            subject,
            html = body
        });

        using var response = await HttpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Resend rejected the email ({(int)response.StatusCode}): {responseBody}");
        }

        Console.WriteLine($"Email sent through Resend to {to}");
    }

    private async Task AuthenticateAsync(SmtpClient smtp, IConfigurationSection emailSettings)
    {
        var authMethod = (emailSettings["AuthenticationMethod"] ?? "Password").Trim();
        var username = emailSettings["Username"];
        var password = emailSettings["Password"];
        var accessToken = emailSettings["AccessToken"];

        switch (authMethod.ToLowerInvariant())
        {
            case "oauth2":
            case "modernauth":
            case "xoauth2":
                if (string.IsNullOrWhiteSpace(accessToken))
                    throw new InvalidOperationException("OAuth2 access token is missing. Set Email:AccessToken or switch AuthenticationMethod to Password.");

                await smtp.AuthenticateAsync(new SaslMechanismOAuth2(username!, accessToken));
                break;

            default:
                if (string.IsNullOrWhiteSpace(password))
                    throw new InvalidOperationException("SMTP password is missing. Set Email:Password or switch AuthenticationMethod to OAuth2.");

                await smtp.AuthenticateAsync(username!, password);
                break;
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