using Microfinance.Domain.Common;

namespace Microfinance.Domain.Entities.Identity;

public class ApplicationRole : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
}