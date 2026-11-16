using Microfinance.Domain.Common;

namespace Microfinance.Domain.Entities.Branches;

public class BranchCashTransfer : BaseEntity
{
    public string TransferReference { get; set; } = string.Empty;
    public Guid FromBranchId { get; set; }
    public Guid ToBranchId { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransferDateUtc { get; set; }
    public string Remarks { get; set; } = string.Empty;
}