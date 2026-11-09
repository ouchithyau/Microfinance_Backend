using Microfinance.Domain.Common;
using Microfinance.Domain.Entities.Groups;
using Microfinance.Domain.Entities.Identity;

namespace Microfinance.Domain.Entities.Customers;

public class Customer : AuditableEntity, IAggregateRoot
{
    public string CustomerCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string NicNumber { get; set; } = string.Empty; // National Identity Card
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string GramaNiladhariDivision { get; set; } = string.Empty;
    public bool IsKycVerified { get; set; } = false;
    public bool IsBlacklisted { get; set; } = false;

    // Center & Group References
    public Guid BranchId { get; set; }
    public Branch? Branch { get; set; }

    public Guid? CenterId { get; set; }
    public Center? Center { get; set; }

    public Guid? SolidarityGroupId { get; set; }
    public SolidarityGroup? SolidarityGroup { get; set; }

    // Navigation properties
    public ICollection<CustomerDocument> Documents { get; set; } = new List<CustomerDocument>();
    public ICollection<Guarantor> Guarantors { get; set; } = new List<Guarantor>();
}