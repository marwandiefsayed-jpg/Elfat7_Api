using Elfat7.Application.Common;

namespace Elfat7.Application.DTOs.Customers;

public class CustomerDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? CommercialRegistration { get; set; }
    public decimal? CreditLimit { get; set; }
    public string? CreditLimitFormatted => CreditLimit.HasValue ? ArabicLocalizationHelper.FormatCurrency(CreditLimit.Value) : null;
    public decimal Balance { get; set; }
    public string BalanceFormatted => ArabicLocalizationHelper.FormatCurrency(Balance);
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CustomerDetailsDto : CustomerDto
{
    public decimal TotalInvoiced { get; set; }
    public string TotalInvoicedFormatted => ArabicLocalizationHelper.FormatCurrency(TotalInvoiced);
    public decimal TotalPaid { get; set; }
    public string TotalPaidFormatted => ArabicLocalizationHelper.FormatCurrency(TotalPaid);
    public List<CustomerRecentInvoiceDto> RecentInvoices { get; set; } = new();
    public List<CustomerRecentPaymentDto> RecentPayments { get; set; } = new();
}

public class CustomerRecentInvoiceDto
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string TotalAmountFormatted => ArabicLocalizationHelper.FormatCurrency(TotalAmount);
    public decimal PaidAmount { get; set; }
    public string PaidAmountFormatted => ArabicLocalizationHelper.FormatCurrency(PaidAmount);
    public decimal RemainingAmount { get; set; }
    public string RemainingAmountFormatted => ArabicLocalizationHelper.FormatCurrency(RemainingAmount);
    public List<CustomerInvoiceItemSummaryDto> Items { get; set; } = new();
    public string ItemsSummary { get; set; } = string.Empty;
}

public class CustomerInvoiceItemSummaryDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class CustomerRecentPaymentDto
{
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string AmountFormatted => ArabicLocalizationHelper.FormatCurrency(Amount);
    public string PaymentMethod { get; set; } = string.Empty;
    public string? SalesInvoiceNumber { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public string? TreasuryName { get; set; }
}

public class CreateCustomerDto
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? CommercialRegistration { get; set; }
    public decimal? CreditLimit { get; set; }
    public string? Notes { get; set; }
}

public class UpdateCustomerDto
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? CommercialRegistration { get; set; }
    public decimal? CreditLimit { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CustomerFilterDto : PagedFilterRequest
{
    public decimal? MinBalance { get; set; }
    public decimal? MaxBalance { get; set; }
    public bool? IsActive { get; set; }
}
