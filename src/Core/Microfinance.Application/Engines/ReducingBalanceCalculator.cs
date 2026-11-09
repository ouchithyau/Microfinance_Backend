using Microfinance.Domain.Enums;

namespace Microfinance.Application.Engines;

public class ReducingBalanceCalculator : IAmortizationCalculator
{
    public InterestCalculationType SupportedType => InterestCalculationType.ReducingBalance;

    public List<InstallmentScheduleItem> CalculateSchedule(
        decimal principal,
        decimal annualInterestRate,
        int tenure,
        RepaymentFrequency frequency,
        DateTime startDate)
    {
        var schedule = new List<InstallmentScheduleItem>();

        decimal periodsPerYear = frequency switch
        {
            RepaymentFrequency.Daily => 365m,
            RepaymentFrequency.Weekly => 52m,
            RepaymentFrequency.BiWeekly => 26m,
            RepaymentFrequency.Monthly => 12m,
            _ => 12m
        };

        decimal periodicRate = (annualInterestRate / 100m) / periodsPerYear;
        decimal rateFactor = (decimal)Math.Pow((double)(1m + periodicRate), tenure);
        decimal installmentAmount = Math.Round(principal * (periodicRate * rateFactor) / (rateFactor - 1m), 2);

        decimal outstanding = principal;
        DateTime currentDueDate = startDate;

        for (int i = 1; i <= tenure; i++)
        {
            currentDueDate = frequency switch
            {
                RepaymentFrequency.Daily => currentDueDate.AddDays(1),
                RepaymentFrequency.Weekly => currentDueDate.AddDays(7),
                RepaymentFrequency.BiWeekly => currentDueDate.AddDays(14),
                RepaymentFrequency.Monthly => currentDueDate.AddMonths(1),
                _ => currentDueDate.AddMonths(1)
            };

            decimal interestForPeriod = Math.Round(outstanding * periodicRate, 2);
            decimal principalForPeriod = installmentAmount - interestForPeriod;

            if (i == tenure || principalForPeriod > outstanding)
            {
                principalForPeriod = outstanding;
            }

            outstanding -= principalForPeriod;

            schedule.Add(new InstallmentScheduleItem
            {
                InstallmentNumber = i,
                DueDate = currentDueDate,
                PrincipalDue = principalForPeriod,
                InterestDue = interestForPeriod,
                OutstandingPrincipal = Math.Max(0, outstanding)
            });

            if (outstanding <= 0) break;
        }

        return schedule;
    }
}