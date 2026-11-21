using Elfat7.Application.Common;

namespace Elfat7.Application.DTOs.SalesReturns;

public class SalesReturnDto
{
    public string ReturnNumber { get; set; } = string.Empty;
    public string SalesInvoiceNumber { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; }
    public string ReturnDateFormatted => ArabicLocalizationHelper.FormatDate(ReturnDate);
    public decimal TotalAmount { get; set; }
    public string TotalAmountFormatted => ArabicLocalizationHelper.FormatCurrency(TotalAmount);
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<SalesReturnItemDto> Items { get; set; } = new();
}

public class SalesReturnItemDto
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

public class CreateSalesReturnDto
{
    public string SalesInvoiceNumber { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public DateTime? ReturnDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<CreateSalesReturnItemDto> Items { get; set; } = new();
}

public class CreateSalesReturnItemDto
{
    public string ProductCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}

public class SalesReturnFilterDto : PagedFilterRequest
{
    public string? CustomerCode { get; set; }
    public string? SalesInvoiceNumber { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
