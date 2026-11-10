using Microsoft.Extensions.Logging;
using Microfinance.Application.Common.Interfaces;

namespace Microfinance.Infrastructure.Notifications;

public class SmsNotificationService : ISmsNotificationService
{
    private readonly ILogger<SmsNotificationService> _logger;

    public SmsNotificationService(ILogger<SmsNotificationService> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[SMS GATEWAY SIMULATION] To: {PhoneNumber} | Message: {Message}", phoneNumber, message);
        return Task.FromResult(true);
    }
}