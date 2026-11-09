using Microfinance.Domain.Enums;

namespace Microfinance.Application.Engines;

public interface IAmortizationCalculator
{
    InterestCalculationType SupportedType { get; }

    List<InstallmentScheduleItem> CalculateSchedule(
        decimal principal,
        decimal annualInterestRate,
        int tenure,
        RepaymentFrequency frequency,
        DateTime startDate);
}