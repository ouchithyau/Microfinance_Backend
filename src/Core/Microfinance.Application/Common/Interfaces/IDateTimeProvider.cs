namespace Microfinance.Application.Common.Interfaces;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateTime Today { get; }
}