using Elfat7.Application.Common;
using Elfat7.Domain.Enums;

namespace Elfat7.Application.DTOs.Sales;

public class SalesOrderDto
{
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string OrderDateFormatted => ArabicLocalizationHelper.FormatDate(OrderDate);
    public SalesOrderStatus Status { get; set; }
    public string StatusArabic => Status.ToArabic();
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string TotalAmountFormatted => ArabicLocalizationHelper.FormatCurrency(TotalAmount);
}

public class SalesOrderDetailsDto : SalesOrderDto
{
    public string? Notes { get; set; }
    public List<SalesOrderItemDto> Items { get; set; } = new();
}

public class SalesOrderItemDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string UnitPriceFormatted => ArabicLocalizationHelper.FormatCurrency(UnitPrice);
    public decimal LineTotal { get; set; }
    public string LineTotalFormatted => ArabicLocalizationHelper.FormatCurrency(LineTotal);
}

public class CreateSalesOrderDto
{
    public string CustomerCode { get; set; } = string.Empty;
    public DateTime? OrderDate { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? Notes { get; set; }
    public List<CreateSalesOrderItemDto> Items { get; set; } = new();
}

public class CreateSalesOrderItemDto
{
    public string ProductCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class SalesOrderFilterDto : PagedFilterRequest
{
    public string? CustomerCode { get; set; }
    public SalesOrderStatus? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class SalesInvoiceDto
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public string? SalesOrderNumber { get; set; }
    public DateTime InvoiceDate { get; set; }
    public string InvoiceDateFormatted => ArabicLocalizationHelper.FormatDate(InvoiceDate);
    public DateTime? DueDate { get; set; }
    public SalesInvoiceStatus Status { get; set; }
    public string StatusArabic => Status.ToArabic();
    public InvoicePaymentStatus PaymentStatus { get; set; }
    public string PaymentStatusArabic => PaymentStatus.ToArabic();
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string TotalAmountFormatted => ArabicLocalizationHelper.FormatCurrency(TotalAmount);
    public decimal PaidAmount { get; set; }
    private decimal? _remainingAmount;
    public decimal RemainingAmount
    {
        get => _remainingAmount ?? Math.Max(0, TotalAmount - PaidAmount);
        set => _remainingAmount = value;
    }
    public string RemainingAmountFormatted => ArabicLocalizationHelper.FormatCurrency(RemainingAmount);
    public int ItemsCount { get; set; }
    public List<SalesInvoiceItemDto> Items { get; set; } = new();
    public string ItemsSummary { get; set; } = string.Empty;
}

public class SalesInvoiceDetailsDto : SalesInvoiceDto
{
    public string? CustomerPhone { get; set; }
    public string? CustomerAddress { get; set; }
    public string? Notes { get; set; }
    public List<DeliverySummaryDto> Deliveries { get; set; } = new();
    public List<SalesReturnSummaryDto> Returns { get; set; } = new();
    public List<SalesInvoicePaymentDto> Payments { get; set; } = new();
}

public class SalesInvoicePaymentDto
{
    public string ReceiptNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string AmountFormatted => ArabicLocalizationHelper.FormatCurrency(Amount);
    public DateTime PaymentDate { get; set; }
    public string PaymentDateFormatted => ArabicLocalizationHelper.FormatDate(PaymentDate);
    public string PaymentMethodArabic { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}

public class SalesInvoiceItemDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string BaseUnit { get; set; } = "كجم";
    public decimal ConversionFactor { get; set; } = 1;
    public decimal Quantity { get; set; }
    public decimal BaseQuantity => Quantity * (ConversionFactor > 0 ? ConversionFactor : 1);
    public decimal UnitPrice { get; set; }
    public string UnitPriceFormatted => ArabicLocalizationHelper.FormatCurrency(UnitPrice);
    public decimal UnitCost { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal { get; set; }
    public string LineTotalFormatted => ArabicLocalizationHelper.FormatCurrency(LineTotal);
    public decimal DeliveredQuantity { get; set; }
    public decimal RemainingQuantity { get; set; }
}

public class DeliverySummaryDto
{
    public string DeliveryNumber { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public DateTime DeliveryDate { get; set; }
    public string StatusArabic { get; set; } = string.Empty;
}

public class SalesReturnSummaryDto
{
    public string ReturnNumber { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class CreateSalesInvoiceDto
{
    public string CustomerCode { get; set; } = string.Empty;
    public string? WarehouseCode { get; set; }
    public string? SalesOrderNumber { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string? Notes { get; set; }
    public bool AutoConfirm { get; set; } = false;
    public List<CreateSalesInvoiceItemDto> Items { get; set; } = new();
}

public class CreateSalesInvoiceItemDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public decimal? ConversionFactor { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
}

public class SalesInvoiceFilterDto : PagedFilterRequest
{
    public string? CustomerCode { get; set; }
    public SalesInvoiceStatus? Status { get; set; }
    public InvoicePaymentStatus? PaymentStatus { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
