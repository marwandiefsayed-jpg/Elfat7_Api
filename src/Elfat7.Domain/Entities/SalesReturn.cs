using Elfat7.Domain.Common;

namespace Elfat7.Domain.Entities;

public class SalesReturn : BaseEntity
{
    public string ReturnNumber { get; set; } = string.Empty;
    public int SalesInvoiceId { get; set; }
    public SalesInvoice? SalesInvoice { get; set; }
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    
    public DateTime ReturnDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }

    public ICollection<SalesReturnItem> Items { get; set; } = new List<SalesReturnItem>();
}

public class SalesReturnItem : BaseEntity
{
    public int SalesReturnId { get; set; }
    public SalesReturn? SalesReturn { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
