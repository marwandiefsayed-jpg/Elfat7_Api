using Elfat7.Domain.Common;

namespace Elfat7.Domain.Entities;

public class GoodsReceipt : BaseEntity
{
    public string ReceiptNumber { get; set; } = string.Empty;
    public int PurchaseOrderId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public int WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    
    public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }

    public ICollection<GoodsReceiptItem> Items { get; set; } = new List<GoodsReceiptItem>();
}

public class GoodsReceiptItem : BaseEntity
{
    public int GoodsReceiptId { get; set; }
    public GoodsReceipt? GoodsReceipt { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal LineTotal { get; set; }
}
