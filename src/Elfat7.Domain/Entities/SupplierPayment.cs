using Elfat7.Domain.Common;
using Elfat7.Domain.Enums;

namespace Elfat7.Domain.Entities;

public class SupplierPayment : BaseEntity
{
    public string PaymentNumber { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public int TreasuryId { get; set; }
    public Treasury? Treasury { get; set; }
    
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}
