using Elfat7.Domain.Common;

namespace Elfat7.Domain.Entities;

public class Warehouse : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? KeeperName { get; set; }
    public string? KeeperPhone { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<WarehouseStock> Stocks { get; set; } = new List<WarehouseStock>();
    public ICollection<StockMovement> Movements { get; set; } = new List<StockMovement>();
}
