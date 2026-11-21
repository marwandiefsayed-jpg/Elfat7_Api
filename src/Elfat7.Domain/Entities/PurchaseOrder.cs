using Elfat7.Domain.Common;
using Elfat7.Domain.Enums;

namespace Elfat7.Domain.Entities;

public class PurchaseOrder : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpectedDeliveryDate { get; set; }
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
    
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }

    public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
    public ICollection<GoodsReceipt> GoodsReceipts { get; set; } = new List<GoodsReceipt>();
}

public class PurchaseOrderItem : BaseEntity
{
    public int PurchaseOrderId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public decimal ReceivedQuantity { get; set; } = 0;
    public decimal RemainingQuantity => Quantity - ReceivedQuantity;
}
