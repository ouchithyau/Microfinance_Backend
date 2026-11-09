using Microfinance.Domain.Common;

namespace Microfinance.Domain.Entities.Customers;

public class Guarantor : AuditableEntity
{
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string NicNumber { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty; // Spouse, Neighbor, Group Member
    public decimal MonthlyIncome { get; set; }
}