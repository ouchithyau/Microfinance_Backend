using Microfinance.Domain.Enums;

namespace Microfinance.Application.Engines;

public class FlatRateCalculator : IAmortizationCalculator
{
    public InterestCalculationType SupportedType => InterestCalculationType.FlatRate;

    public List<InstallmentScheduleItem> CalculateSchedule(
        decimal principal,
        decimal annualInterestRate,
        int tenure,
        RepaymentFrequency frequency,
        DateTime startDate)
    {
        var schedule = new List<InstallmentScheduleItem>();

        decimal years = frequency switch
        {
            RepaymentFrequency.Daily => (decimal)tenure / 365m,
            RepaymentFrequency.Weekly => (decimal)tenure / 52m,
            RepaymentFrequency.BiWeekly => (decimal)tenure / 26m,
            RepaymentFrequency.Monthly => (decimal)tenure / 12m,
            _ => (decimal)tenure / 12m
        };

        decimal totalInterest = Math.Round(principal * (annualInterestRate / 100m) * years, 2);
        decimal principalPerInstallment = Math.Round(principal / tenure, 2);
        decimal interestPerInstallment = Math.Round(totalInterest / tenure, 2);

        decimal remainingPrincipal = principal;
        decimal remainingInterest = totalInterest;
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

            decimal thisPrincipal = (i == tenure) ? remainingPrincipal : principalPerInstallment;
            decimal thisInterest = (i == tenure) ? remainingInterest : interestPerInstallment;

            remainingPrincipal -= thisPrincipal;
            remainingInterest -= thisInterest;

            schedule.Add(new InstallmentScheduleItem
            {
                InstallmentNumber = i,
                DueDate = currentDueDate,
                PrincipalDue = thisPrincipal,
                InterestDue = thisInterest,
                OutstandingPrincipal = Math.Max(0, remainingPrincipal)
            });
        }

        return schedule;
    }
}