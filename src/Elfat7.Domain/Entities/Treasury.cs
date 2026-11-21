using Elfat7.Domain.Common;

namespace Elfat7.Domain.Entities;

public class Treasury : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Balance { get; set; } = 0; // الرصيد الحالي
    public bool IsActive { get; set; } = true;

    public ICollection<TreasuryTransaction> Transactions { get; set; } = new List<TreasuryTransaction>();
}
