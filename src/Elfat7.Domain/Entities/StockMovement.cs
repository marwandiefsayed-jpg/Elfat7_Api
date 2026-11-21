using Elfat7.Domain.Common;
using Elfat7.Domain.Enums;

namespace Elfat7.Domain.Entities;

public class StockMovement : BaseEntity
{
    public string MovementNumber { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    
    public StockMovementType MovementType { get; set; }
    public decimal Quantity { get; set; } // الكمية التي تحركت (+ أو -)
    public decimal UnitCost { get; set; } // تكلفة الوحدة وقت الحركة
    public decimal TotalCost { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty; // رقم إذن الاستلام أو التسليم أو التحويل
    public string? Notes { get; set; }
}
