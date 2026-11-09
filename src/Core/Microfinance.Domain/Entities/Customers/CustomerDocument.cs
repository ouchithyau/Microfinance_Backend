using Microfinance.Domain.Common;

namespace Microfinance.Domain.Entities.Customers;

public class CustomerDocument : AuditableEntity
{
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public string DocumentType { get; set; } = string.Empty; // NIC Front, NIC Back, Customer Photo, Utility Bill
    public string FilePath { get; set; } = string.Empty;     // Storage URL or file path
    public string ContentType { get; set; } = string.Empty;
    public bool IsVerified { get; set; } = false;
}