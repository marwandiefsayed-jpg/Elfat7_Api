using Elfat7.Domain.Common;

namespace Elfat7.Domain.Entities;

public class WarehouseStock : BaseEntity
{
    public int WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    
    public decimal Quantity { get; set; } = 0; // الكمية الفعلية المتاحة
    public decimal ReservedQuantity { get; set; } = 0; // الكمية المحجوزة
    public decimal AvailableQuantity => Quantity - ReservedQuantity;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
