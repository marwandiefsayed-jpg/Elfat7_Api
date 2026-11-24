using Elfat7.Application.Common;

namespace Elfat7.Application.DTOs.Reports;

public class PurchasesReportFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SupplierCode { get; set; }
    public string? Status { get; set; }
}

public class PurchasesReportDto
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalPurchasesAmount { get; set; }
    public string TotalPurchasesAmountFormatted => ArabicLocalizationHelper.FormatCurrency(TotalPurchasesAmount);
    public decimal TotalDiscounts { get; set; }
    public decimal TotalNetPurchases { get; set; }
    public string TotalNetPurchasesFormatted => ArabicLocalizationHelper.FormatCurrency(TotalNetPurchases);
    public List<PurchasesReportItemDto> Items { get; set; } = new();
}

public class PurchasesReportItemDto
{
    public string OrderNumber { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string OrderDateFormatted => ArabicLocalizationHelper.FormatDate(OrderDate);
    public string StatusArabic { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string TotalAmountFormatted => ArabicLocalizationHelper.FormatCurrency(TotalAmount);
}

public class SalesReportFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? CustomerCode { get; set; }
    public string? Status { get; set; }
}

public class SalesReportDto
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int TotalInvoices { get; set; }
    public decimal TotalSalesAmount { get; set; }
    public string TotalSalesAmountFormatted => ArabicLocalizationHelper.FormatCurrency(TotalSalesAmount);
    public decimal TotalPaidAmount { get; set; }
    public decimal TotalRemainingAmount { get; set; }
    public decimal TotalEstimatedCost { get; set; }
    public decimal TotalGrossProfit => TotalSalesAmount - TotalEstimatedCost;
    public string TotalGrossProfitFormatted => ArabicLocalizationHelper.FormatCurrency(TotalGrossProfit);
    public List<SalesReportItemDto> Items { get; set; } = new();
}

public class SalesReportItemDto
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public string InvoiceDateFormatted => ArabicLocalizationHelper.FormatDate(InvoiceDate);
    public string StatusArabic { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal GrossProfit { get; set; }
}

public class InventoryReportFilterDto
{
    public string? CategoryCode { get; set; }
    public string? WarehouseCode { get; set; }
    public bool? OnlyLowStock { get; set; }
}

public class InventoryReportDto
{
    public int TotalProducts { get; set; }
    public decimal TotalStockQuantity { get; set; }
    public decimal TotalStockValuation { get; set; }
    public string TotalStockValuationFormatted => ArabicLocalizationHelper.FormatCurrency(TotalStockValuation);
    public List<InventoryReportItemDto> Items { get; set; } = new();
}

public class InventoryReportItemDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal AverageCost { get; set; }
    public string AverageCostFormatted => ArabicLocalizationHelper.FormatCurrency(AverageCost);
    public decimal TotalValuation { get; set; }
    public string TotalValuationFormatted => ArabicLocalizationHelper.FormatCurrency(TotalValuation);
}

public class WarehouseStockReportFilterDto
{
    public string? WarehouseCode { get; set; }
}

public class WarehouseStockReportDto
{
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public int DistinctProductsCount { get; set; }
    public decimal TotalValuation { get; set; }
    public string TotalValuationFormatted => ArabicLocalizationHelper.FormatCurrency(TotalValuation);
    public List<WarehouseStockReportItemDto> Items { get; set; } = new();
}

public class WarehouseStockReportItemDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal AvailableQuantity { get; set; }
    public decimal AverageCost { get; set; }
    public decimal TotalValuation { get; set; }
}

public class SupplierBalancesReportFilterDto
{
    public decimal? MinBalance { get; set; }
    public bool? OnlyWithBalance { get; set; }
}

public class SupplierBalancesReportDto
{
    public int TotalSuppliers { get; set; }
    public decimal TotalOutstandingDebt { get; set; } // إجمالي مستحقات الموردين (دائن)
    public string TotalOutstandingDebtFormatted => ArabicLocalizationHelper.FormatCurrency(TotalOutstandingDebt);
    public List<SupplierBalancesReportItemDto> Items { get; set; } = new();
}

public class SupplierBalancesReportItemDto
{
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string BalanceFormatted => ArabicLocalizationHelper.FormatCurrency(Balance);
}

public class CustomerBalancesReportFilterDto
{
    public decimal? MinBalance { get; set; }
    public bool? OnlyWithBalance { get; set; }
}

public class CustomerBalancesReportDto
{
    public int TotalCustomers { get; set; }
    public decimal TotalOutstandingReceivables { get; set; } // إجمالي مستحقات على العملاء (مدين)
    public string TotalOutstandingReceivablesFormatted => ArabicLocalizationHelper.FormatCurrency(TotalOutstandingReceivables);
    public List<CustomerBalancesReportItemDto> Items { get; set; } = new();
}

public class CustomerBalancesReportItemDto
{
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public decimal? CreditLimit { get; set; }
    public decimal Balance { get; set; }
    public string BalanceFormatted => ArabicLocalizationHelper.FormatCurrency(Balance);
}

public class ExpensesReportFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Category { get; set; }
    public string? TreasuryCode { get; set; }
}

public class ExpensesReportDto
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int TotalExpensesCount { get; set; }
    public decimal TotalExpensesAmount { get; set; }
    public string TotalExpensesAmountFormatted => ArabicLocalizationHelper.FormatCurrency(TotalExpensesAmount);
    public List<ExpensesReportCategorySummaryDto> CategoryBreakdown { get; set; } = new();
    public List<ExpensesReportItemDto> Items { get; set; } = new();
}

public class ExpensesReportCategorySummaryDto
{
    public string Category { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string TotalAmountFormatted => ArabicLocalizationHelper.FormatCurrency(TotalAmount);
    public decimal Percentage { get; set; }
}

public class ExpensesReportItemDto
{
    public string ExpenseNumber { get; set; } = string.Empty;
    public DateTime ExpenseDate { get; set; }
    public string ExpenseDateFormatted => ArabicLocalizationHelper.FormatDate(ExpenseDate);
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string AmountFormatted => ArabicLocalizationHelper.FormatCurrency(Amount);
    public string Description { get; set; } = string.Empty;
    public string? PaidTo { get; set; }
    public string TreasuryName { get; set; } = string.Empty;
}

public class ProfitLossReportFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class ProfitLossReportDto
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal TotalRevenue { get; set; } // إجمالي المبيعات
    public string TotalRevenueFormatted => ArabicLocalizationHelper.FormatCurrency(TotalRevenue);
    public decimal TotalCOGS { get; set; } // تكلفة البضاعة المباعة
    public string TotalCOGSFormatted => ArabicLocalizationHelper.FormatCurrency(TotalCOGS);
    public decimal GrossProfit => TotalRevenue - TotalCOGS; // مجمل الربح
    public string GrossProfitFormatted => ArabicLocalizationHelper.FormatCurrency(GrossProfit);
    public decimal OperatingExpenses { get; set; } // المصروفات التشغيلية
    public string OperatingExpensesFormatted => ArabicLocalizationHelper.FormatCurrency(OperatingExpenses);
    public decimal NetProfit => GrossProfit - OperatingExpenses; // صافي الربح
    public string NetProfitFormatted => ArabicLocalizationHelper.FormatCurrency(NetProfit);
    public decimal ProfitMarginPercentage => TotalRevenue > 0 ? Math.Round((NetProfit / TotalRevenue) * 100, 2) : 0;
}
