using Elfat7.Domain.Common;
using Elfat7.Domain.Enums;

namespace Elfat7.Domain.Entities;

public class SalesInvoice : BaseEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int? SalesOrderId { get; set; }
    public SalesOrder? SalesOrder { get; set; }
    
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public SalesInvoiceStatus Status { get; set; } = SalesInvoiceStatus.Draft;
    public InvoicePaymentStatus PaymentStatus { get; set; } = InvoicePaymentStatus.Unpaid;
    
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; } = 0;
    public decimal RemainingAmount => TotalAmount - PaidAmount;
    public string? Notes { get; set; }

    public ICollection<SalesInvoiceItem> Items { get; set; } = new List<SalesInvoiceItem>();
    public ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();
    public ICollection<SalesReturn> Returns { get; set; } = new List<SalesReturn>();
    public ICollection<CustomerPayment> Payments { get; set; } = new List<CustomerPayment>();
}

public class SalesInvoiceItem : BaseEntity
{
    public int SalesInvoiceId { get; set; }
    public SalesInvoice? SalesInvoice { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal UnitCost { get; set; } // التكلفة وقت البيع لحساب الربح
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal { get; set; }
    public decimal DeliveredQuantity { get; set; } = 0;
    public decimal RemainingQuantity => Quantity - DeliveredQuantity;
}
