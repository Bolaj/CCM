namespace CityChoir.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task<bool> TestConnectionAsync();
}