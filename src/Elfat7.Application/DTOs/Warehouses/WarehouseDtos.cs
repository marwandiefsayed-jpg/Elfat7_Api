using Elfat7.Application.Common;

namespace Elfat7.Application.DTOs.Warehouses;

public class WarehouseDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? KeeperName { get; set; }
    public string? KeeperPhone { get; set; }
    public bool IsActive { get; set; }
    public int ProductsInStockCount { get; set; }
}

public class WarehouseDetailsDto : WarehouseDto
{
    public List<WarehouseStockItemDto> StockItems { get; set; } = new();
}

public class WarehouseStockItemDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal ReservedQuantity { get; set; }
    public decimal AvailableQuantity { get; set; }
    public decimal AverageCost { get; set; }
    public decimal TotalValuation => Quantity * AverageCost;
    public string TotalValuationFormatted => ArabicLocalizationHelper.FormatCurrency(TotalValuation);
}

public class CreateWarehouseDto
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? KeeperName { get; set; }
    public string? KeeperPhone { get; set; }
}

public class UpdateWarehouseDto
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? KeeperName { get; set; }
    public string? KeeperPhone { get; set; }
    public bool IsActive { get; set; } = true;
}

public class WarehouseFilterDto : PagedFilterRequest
{
    public bool? IsActive { get; set; }
}
