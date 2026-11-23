using Elfat7.Application.Common;
using Elfat7.Domain.Enums;

namespace Elfat7.Application.DTOs.Inventory;

public class InventoryItemDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal TotalQuantity { get; set; }
    public decimal TotalReservedQuantity { get; set; }
    public decimal TotalAvailableQuantity => TotalQuantity - TotalReservedQuantity;
    public decimal AverageCost { get; set; }
    public decimal TotalValuation => TotalQuantity * AverageCost;
    public string TotalValuationFormatted => ArabicLocalizationHelper.FormatCurrency(TotalValuation);
}

public class InventoryFilterDto : PagedFilterRequest
{
    public string? CategoryCode { get; set; }
    public string? WarehouseCode { get; set; }
    public bool? OnlyLowStock { get; set; }
}

public class StockMovementDto
{
    public string MovementNumber { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public StockMovementType MovementType { get; set; }
    public string MovementTypeArabic => MovementType.ToArabic();
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal UnitCost { get; set; }
    public string UnitCostFormatted => ArabicLocalizationHelper.FormatCurrency(UnitCost);
    public decimal TotalCost { get; set; }
    public string TotalCostFormatted => ArabicLocalizationHelper.FormatCurrency(TotalCost);
    public string ReferenceNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedAtFormatted => ArabicLocalizationHelper.FormatDateTime(CreatedAt);
    public string? Notes { get; set; }
}

public class StockMovementFilterDto : PagedFilterRequest
{
    public string? WarehouseCode { get; set; }
    public string? ProductCode { get; set; }
    public StockMovementType? MovementType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class StockTransferDto
{
    public string TransferNumber { get; set; } = string.Empty;
    public string SourceWarehouseCode { get; set; } = string.Empty;
    public string SourceWarehouseName { get; set; } = string.Empty;
    public string DestinationWarehouseCode { get; set; } = string.Empty;
    public string DestinationWarehouseName { get; set; } = string.Empty;
    public DateTime TransferDate { get; set; }
    public string TransferDateFormatted => ArabicLocalizationHelper.FormatDate(TransferDate);
    public TransferStatus Status { get; set; }
    public string StatusArabic => Status.ToArabic();
    public string? Notes { get; set; }
    public List<StockTransferItemDto> Items { get; set; } = new();
}

public class StockTransferItemDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
}

public class CreateStockTransferDto
{
    public string SourceWarehouseCode { get; set; } = string.Empty;
    public string DestinationWarehouseCode { get; set; } = string.Empty;
    public DateTime? TransferDate { get; set; }
    public string? Notes { get; set; }
    public List<CreateStockTransferItemDto> Items { get; set; } = new();
}

public class CreateStockTransferItemDto
{
    public string ProductCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}

public class StockAdjustmentDto
{
    public string AdjustmentNumber { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public DateTime AdjustmentDate { get; set; }
    public string AdjustmentDateFormatted => ArabicLocalizationHelper.FormatDate(AdjustmentDate);
    public AdjustmentType Type { get; set; }
    public string TypeArabic => Type.ToArabic();
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<StockAdjustmentItemDto> Items { get; set; } = new();
}

public class StockAdjustmentItemDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public string? Reason { get; set; }
}

public class CreateStockAdjustmentDto
{
    public string WarehouseCode { get; set; } = string.Empty;
    public AdjustmentType Type { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime? AdjustmentDate { get; set; }
    public string? Notes { get; set; }
    public List<CreateStockAdjustmentItemDto> Items { get; set; } = new();
}

public class CreateStockAdjustmentItemDto
{
    public string ProductCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? Reason { get; set; }
}

public class UpdateStockQuantityDto
{
    public string WarehouseCode { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public decimal? ConversionFactor { get; set; }
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }
}

public class AddIncomingStockDto
{
    public string WarehouseCode { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public decimal? ConversionFactor { get; set; }
    public decimal Quantity { get; set; }
    public decimal? UnitCost { get; set; }
    public string? Notes { get; set; }
}

public class WarehouseStockDto
{
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal ReservedQuantity { get; set; }
    public decimal AvailableQuantity { get; set; }
    public decimal AverageCost { get; set; }
}
