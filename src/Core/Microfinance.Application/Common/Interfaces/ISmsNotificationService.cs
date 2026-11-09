namespace Microfinance.Application.Common.Interfaces;

public interface ISmsNotificationService
{
    Task<bool> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
}