using Elfat7.Domain.Common;
using Elfat7.Domain.Enums;

namespace Elfat7.Domain.Entities;

public class TreasuryTransaction : BaseEntity
{
    public string TransactionNumber { get; set; } = string.Empty;
    public int TreasuryId { get; set; }
    public Treasury? Treasury { get; set; }
    
    public TreasuryTransactionType TransactionType { get; set; }
    public TreasuryTransactionCategory Category { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
