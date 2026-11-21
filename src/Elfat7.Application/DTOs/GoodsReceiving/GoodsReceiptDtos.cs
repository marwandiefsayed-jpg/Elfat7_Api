using Elfat7.Application.Common;

namespace Elfat7.Application.DTOs.GoodsReceiving;

public class GoodsReceiptDto
{
    public string ReceiptNumber { get; set; } = string.Empty;
    public string PurchaseOrderNumber { get; set; } = string.Empty;
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public string ReceiptDateFormatted => ArabicLocalizationHelper.FormatDate(ReceiptDate);
    public decimal TotalAmount { get; set; }
    public string TotalAmountFormatted => ArabicLocalizationHelper.FormatCurrency(TotalAmount);
    public int ItemsCount { get; set; }
}

public class GoodsReceiptDetailsDto : GoodsReceiptDto
{
    public string? Notes { get; set; }
    public List<GoodsReceiptItemDto> Items { get; set; } = new();
}

public class GoodsReceiptItemDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public string UnitCostFormatted => ArabicLocalizationHelper.FormatCurrency(UnitCost);
    public decimal LineTotal { get; set; }
    public string LineTotalFormatted => ArabicLocalizationHelper.FormatCurrency(LineTotal);
}

public class CreateGoodsReceiptDto
{
    public string PurchaseOrderNumber { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public DateTime? ReceiptDate { get; set; }
    public string? Notes { get; set; }
    public List<CreateGoodsReceiptItemDto> Items { get; set; } = new();
}

public class CreateGoodsReceiptItemDto
{
    public string ProductCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
}

public class GoodsReceiptFilterDto : PagedFilterRequest
{
    public string? SupplierCode { get; set; }
    public string? WarehouseCode { get; set; }
    public string? PurchaseOrderNumber { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
