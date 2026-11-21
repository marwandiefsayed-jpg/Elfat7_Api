using Elfat7.Domain.Common;

namespace Elfat7.Domain.Entities;

public class OperatingExpense : BaseEntity
{
    public string ExpenseNumber { get; set; } = string.Empty;
    public int TreasuryId { get; set; }
    public Treasury? Treasury { get; set; }
    
    // تصنيف المصروف (نقل وشحن / عمالة وتعتيق / تعبئة وتغليف / كهرباء ومرافق / إيجار / صيانة / نثريات)
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
    public string Description { get; set; } = string.Empty;
    public string? PaidTo { get; set; }
    public string? Notes { get; set; }
}
