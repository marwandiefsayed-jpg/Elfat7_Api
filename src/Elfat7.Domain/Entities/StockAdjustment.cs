using Elfat7.Domain.Common;
using Elfat7.Domain.Enums;

namespace Elfat7.Domain.Entities;

public class StockAdjustment : BaseEntity
{
    public string AdjustmentNumber { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    
    public DateTime AdjustmentDate { get; set; } = DateTime.UtcNow;
    public AdjustmentType Type { get; set; }
    public string Reason { get; set; } = string.Empty; // هالك، عجز جرد، زيادة جرد، رطوبة، تلف
    public string? Notes { get; set; }

    public ICollection<StockAdjustmentItem> Items { get; set; } = new List<StockAdjustmentItem>();
}

public class StockAdjustmentItem : BaseEntity
{
    public int StockAdjustmentId { get; set; }
    public StockAdjustment? StockAdjustment { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public string? Reason { get; set; }
}
