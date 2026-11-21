using Elfat7.Application.Common;

namespace Elfat7.Application.DTOs.Suppliers;

public class SupplierDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? CommercialRegistration { get; set; }
    public string? TaxNumber { get; set; }
    public decimal Balance { get; set; }
    public string BalanceFormatted => ArabicLocalizationHelper.FormatCurrency(Balance);
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SupplierDetailsDto : SupplierDto
{
    public List<SupplierRecentOrderDto> RecentOrders { get; set; } = new();
    public List<SupplierRecentPaymentDto> RecentPayments { get; set; } = new();
}

public class SupplierRecentOrderDto
{
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string TotalAmountFormatted => ArabicLocalizationHelper.FormatCurrency(TotalAmount);
}

public class SupplierRecentPaymentDto
{
    public string PaymentNumber { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string AmountFormatted => ArabicLocalizationHelper.FormatCurrency(Amount);
    public string PaymentMethod { get; set; } = string.Empty;
}

public class CreateSupplierDto
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? CommercialRegistration { get; set; }
    public string? TaxNumber { get; set; }
    public string? Notes { get; set; }
}

public class UpdateSupplierDto
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? CommercialRegistration { get; set; }
    public string? TaxNumber { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}

public class SupplierFilterDto : PagedFilterRequest
{
    public decimal? MinBalance { get; set; }
    public decimal? MaxBalance { get; set; }
    public bool? IsActive { get; set; }
}
