using Elfat7.Domain.Common;

namespace Elfat7.Domain.Entities;

public class Product : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public ProductCategory? Category { get; set; }
    
    // الوحدة (طن / كيلو / شيكارة 50كجم / جوال / كرتونة)
    public string Unit { get; set; } = "طن";
    public decimal DefaultPurchasePrice { get; set; } = 0;
    public decimal DefaultSellingPrice { get; set; } = 0;
    
    // متوسط التكلفة المرجح (Moving Weighted Average Cost)
    public decimal AverageCost { get; set; } = 0;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<WarehouseStock> WarehouseStocks { get; set; } = new List<WarehouseStock>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    public ICollection<ProductSellingUnit> SellingUnits { get; set; } = new List<ProductSellingUnit>();
}
