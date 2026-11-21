using Elfat7.Domain.Common;
using Elfat7.Domain.Enums;

namespace Elfat7.Domain.Entities;

public class SalesOrder : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Draft;
    
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }

    public ICollection<SalesOrderItem> Items { get; set; } = new List<SalesOrderItem>();
    public ICollection<SalesInvoice> SalesInvoices { get; set; } = new List<SalesInvoice>();
}

public class SalesOrderItem : BaseEntity
{
    public int SalesOrderId { get; set; }
    public SalesOrder? SalesOrder { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
