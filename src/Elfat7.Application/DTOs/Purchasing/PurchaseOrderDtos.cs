using Elfat7.Application.Common;
using Elfat7.Domain.Enums;

namespace Elfat7.Application.DTOs.Purchasing;

public class PurchaseOrderDto
{
    public string OrderNumber { get; set; } = string.Empty;
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string OrderDateFormatted => ArabicLocalizationHelper.FormatDate(OrderDate);
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string? ExpectedDeliveryDateFormatted => ExpectedDeliveryDate.HasValue ? ArabicLocalizationHelper.FormatDate(ExpectedDeliveryDate.Value) : "-";
    public PurchaseOrderStatus Status { get; set; }
    public string StatusArabic => Status.ToArabic();
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string TotalAmountFormatted => ArabicLocalizationHelper.FormatCurrency(TotalAmount);
    public int ItemsCount { get; set; }
}

public class PurchaseOrderDetailsDto : PurchaseOrderDto
{
    public string? Notes { get; set; }
    public List<PurchaseOrderItemDto> Items { get; set; } = new();
    public List<GoodsReceiptSummaryDto> Receipts { get; set; } = new();
}

public class PurchaseOrderItemDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string UnitPriceFormatted => ArabicLocalizationHelper.FormatCurrency(UnitPrice);
    public decimal LineTotal { get; set; }
    public string LineTotalFormatted => ArabicLocalizationHelper.FormatCurrency(LineTotal);
    public decimal ReceivedQuantity { get; set; }
    public decimal RemainingQuantity { get; set; }
}

public class GoodsReceiptSummaryDto
{
    public string ReceiptNumber { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public decimal TotalAmount { get; set; }
}

public class CreatePurchaseOrderDto
{
    public string SupplierCode { get; set; } = string.Empty;
    public DateTime? OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? Notes { get; set; }
    public List<CreatePurchaseOrderItemDto> Items { get; set; } = new();
}

public class CreatePurchaseOrderItemDto
{
    public string ProductCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class PurchaseOrderFilterDto : PagedFilterRequest
{
    public string? SupplierCode { get; set; }
    public PurchaseOrderStatus? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
