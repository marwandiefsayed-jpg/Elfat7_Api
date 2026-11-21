using Elfat7.Application.Common;
using Elfat7.Domain.Enums;

namespace Elfat7.Application.DTOs.Financial;

public class TreasuryDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string BalanceFormatted => ArabicLocalizationHelper.FormatCurrency(Balance);
    public bool IsActive { get; set; }
}

public class CreateTreasuryDto
{
    public string Name { get; set; } = string.Empty;
    public decimal InitialBalance { get; set; }
}

public class TreasuryTransactionDto
{
    public string TransactionNumber { get; set; } = string.Empty;
    public string TreasuryCode { get; set; } = string.Empty;
    public string TreasuryName { get; set; } = string.Empty;
    public TreasuryTransactionType TransactionType { get; set; }
    public string TransactionTypeArabic => TransactionType.ToArabic();
    public TreasuryTransactionCategory Category { get; set; }
    public string CategoryArabic => Category.ToArabic();
    public decimal Amount { get; set; }
    public string AmountFormatted => ArabicLocalizationHelper.FormatCurrency(Amount);
    public DateTime TransactionDate { get; set; }
    public string TransactionDateFormatted => ArabicLocalizationHelper.FormatDate(TransactionDate);
    public string ReferenceNumber { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class TreasuryFilterDto : PagedFilterRequest
{
    public string? TreasuryCode { get; set; }
    public TreasuryTransactionType? TransactionType { get; set; }
    public TreasuryTransactionCategory? Category { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class SupplierPaymentDto
{
    public string PaymentNumber { get; set; } = string.Empty;
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string TreasuryCode { get; set; } = string.Empty;
    public string TreasuryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string AmountFormatted => ArabicLocalizationHelper.FormatCurrency(Amount);
    public DateTime PaymentDate { get; set; }
    public string PaymentDateFormatted => ArabicLocalizationHelper.FormatDate(PaymentDate);
    public PaymentMethod PaymentMethod { get; set; }
    public string PaymentMethodArabic => PaymentMethod.ToArabic();
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}

public class CreateSupplierPaymentDto
{
    public string SupplierCode { get; set; } = string.Empty;
    public string TreasuryCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime? PaymentDate { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}

public class CustomerPaymentDto
{
    public string ReceiptNumber { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string TreasuryCode { get; set; } = string.Empty;
    public string TreasuryName { get; set; } = string.Empty;
    public string? SalesInvoiceNumber { get; set; }
    public decimal Amount { get; set; }
    public string AmountFormatted => ArabicLocalizationHelper.FormatCurrency(Amount);
    public DateTime PaymentDate { get; set; }
    public string PaymentDateFormatted => ArabicLocalizationHelper.FormatDate(PaymentDate);
    public PaymentMethod PaymentMethod { get; set; }
    public string PaymentMethodArabic => PaymentMethod.ToArabic();
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}

public class CreateCustomerPaymentDto
{
    public string CustomerCode { get; set; } = string.Empty;
    public string TreasuryCode { get; set; } = string.Empty;
    public string? SalesInvoiceNumber { get; set; }
    public decimal Amount { get; set; }
    public DateTime? PaymentDate { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}

public class PaymentFilterDto : PagedFilterRequest
{
    public string? PartyCode { get; set; } // SupplierCode or CustomerCode
    public string? TreasuryCode { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class OperatingExpenseDto
{
    public string ExpenseNumber { get; set; } = string.Empty;
    public string TreasuryCode { get; set; } = string.Empty;
    public string TreasuryName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string AmountFormatted => ArabicLocalizationHelper.FormatCurrency(Amount);
    public DateTime ExpenseDate { get; set; }
    public string ExpenseDateFormatted => ArabicLocalizationHelper.FormatDate(ExpenseDate);
    public string Description { get; set; } = string.Empty;
    public string? PaidTo { get; set; }
    public string? Notes { get; set; }
}

public class CreateOperatingExpenseDto
{
    public string TreasuryCode { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime? ExpenseDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? PaidTo { get; set; }
    public string? Notes { get; set; }
}

public class ExpenseFilterDto : PagedFilterRequest
{
    public string? Category { get; set; }
    public string? TreasuryCode { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
