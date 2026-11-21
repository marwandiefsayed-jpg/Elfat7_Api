using Elfat7.Domain.Common;
using Elfat7.Domain.Enums;

namespace Elfat7.Domain.Entities;

public class CustomerPayment : BaseEntity
{
    public string ReceiptNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int TreasuryId { get; set; }
    public Treasury? Treasury { get; set; }
    public int? SalesInvoiceId { get; set; }
    public SalesInvoice? SalesInvoice { get; set; }
    
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}
