namespace Microfinance.Application.Engines;

public class InstallmentScheduleItem
{
    public int InstallmentNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal PrincipalDue { get; set; }
    public decimal InterestDue { get; set; }
    public decimal TotalDue => PrincipalDue + InterestDue;
    public decimal OutstandingPrincipal { get; set; }
}