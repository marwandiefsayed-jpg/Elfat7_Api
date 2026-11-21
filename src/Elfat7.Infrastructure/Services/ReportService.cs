using Elfat7.Application.Common;
using Elfat7.Application.DTOs.Reports;
using Elfat7.Application.Interfaces;
using Elfat7.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Elfat7.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly IApplicationDbContext _context;

    public ReportService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PurchasesReportDto> GetPurchasesReportAsync(PurchasesReportFilterDto filter, CancellationToken ct = default)
    {
        var start = filter.StartDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = filter.EndDate ?? DateTime.UtcNow;

        var query = _context.PurchaseOrders
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Where(p => p.OrderDate >= start && p.OrderDate <= end);

        if (!string.IsNullOrWhiteSpace(filter.SupplierCode))
            query = query.Where(p => p.Supplier != null && p.Supplier.Code == filter.SupplierCode);

        var orders = await query.OrderByDescending(p => p.OrderDate).ToListAsync(ct);

        var items = orders.Select(p => new PurchasesReportItemDto
        {
            OrderNumber = p.OrderNumber,
            SupplierName = p.Supplier?.Name ?? "",
            OrderDate = p.OrderDate,
            StatusArabic = p.Status.ToArabic(),
            TotalAmount = p.TotalAmount
        }).ToList();

        var totalPurchases = orders.Sum(p => p.SubTotal);
        var totalDiscounts = orders.Sum(p => p.DiscountAmount);
        var totalNet = orders.Sum(p => p.TotalAmount);

        return new PurchasesReportDto
        {
            PeriodStart = start,
            PeriodEnd = end,
            TotalOrders = orders.Count,
            TotalPurchasesAmount = totalPurchases,
            TotalDiscounts = totalDiscounts,
            TotalNetPurchases = totalNet,
            Items = items
        };
    }

    public async Task<SalesReportDto> GetSalesReportAsync(SalesReportFilterDto filter, CancellationToken ct = default)
    {
        var start = filter.StartDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = filter.EndDate ?? DateTime.UtcNow;

        var query = _context.SalesInvoices
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.Items)
            .Where(s => s.InvoiceDate >= start && s.InvoiceDate <= end && s.Status != SalesInvoiceStatus.Cancelled);

        if (!string.IsNullOrWhiteSpace(filter.CustomerCode))
            query = query.Where(s => s.Customer != null && s.Customer.Code == filter.CustomerCode);

        var invoices = await query.OrderByDescending(s => s.InvoiceDate).ToListAsync(ct);

        var items = invoices.Select(s =>
        {
            var cost = s.Items.Sum(i => i.Quantity * i.UnitCost);
            return new SalesReportItemDto
            {
                InvoiceNumber = s.InvoiceNumber,
                CustomerName = s.Customer?.Name ?? "",
                InvoiceDate = s.InvoiceDate,
                StatusArabic = s.Status.ToArabic(),
                TotalAmount = s.TotalAmount,
                PaidAmount = s.PaidAmount,
                RemainingAmount = s.RemainingAmount,
                GrossProfit = s.TotalAmount - cost
            };
        }).ToList();

        var totalSales = invoices.Sum(i => i.TotalAmount);
        var totalPaid = invoices.Sum(i => i.PaidAmount);
        var totalRemaining = invoices.Sum(i => i.RemainingAmount);
        var totalCost = invoices.Sum(i => i.Items.Sum(item => item.Quantity * item.UnitCost));

        return new SalesReportDto
        {
            PeriodStart = start,
            PeriodEnd = end,
            TotalInvoices = invoices.Count,
            TotalSalesAmount = totalSales,
            TotalPaidAmount = totalPaid,
            TotalRemainingAmount = totalRemaining,
            TotalEstimatedCost = totalCost,
            Items = items
        };
    }

    public async Task<InventoryReportDto> GetInventoryReportAsync(InventoryReportFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.WarehouseStocks)
            .Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(filter.CategoryCode))
            query = query.Where(p => p.Category != null && p.Category.Code == filter.CategoryCode);

        var products = await query.ToListAsync(ct);

        var items = products.Select(p =>
        {
            var qty = p.WarehouseStocks.Sum(ws => ws.Quantity);
            return new InventoryReportItemDto
            {
                ProductCode = p.Code,
                ProductName = p.Name,
                CategoryName = p.Category?.Name ?? "",
                Unit = p.Unit,
                Quantity = qty,
                AverageCost = p.AverageCost,
                TotalValuation = qty * p.AverageCost
            };
        }).ToList();

        if (filter.OnlyLowStock == true)
        {
            items = items.Where(i => i.Quantity < 10).ToList();
        }

        return new InventoryReportDto
        {
            TotalProducts = items.Count,
            TotalStockQuantity = items.Sum(i => i.Quantity),
            TotalStockValuation = items.Sum(i => i.TotalValuation),
            Items = items
        };
    }

    public async Task<WarehouseStockReportDto> GetWarehouseStockReportAsync(WarehouseStockReportFilterDto filter, CancellationToken ct = default)
    {
        var warehouse = await _context.Warehouses
            .AsNoTracking()
            .Include(w => w.Stocks)
            .ThenInclude(s => s.Product)
            .FirstOrDefaultAsync(w => string.IsNullOrWhiteSpace(filter.WarehouseCode) || w.Code == filter.WarehouseCode, ct);

        if (warehouse == null)
        {
            return new WarehouseStockReportDto
            {
                WarehouseCode = "-",
                WarehouseName = "لا يوجد مخزن",
                DistinctProductsCount = 0,
                TotalValuation = 0
            };
        }

        var items = warehouse.Stocks
            .Where(s => s.Quantity > 0 && s.Product != null)
            .Select(s => new WarehouseStockReportItemDto
            {
                ProductCode = s.Product!.Code,
                ProductName = s.Product.Name,
                Unit = s.Product.Unit,
                Quantity = s.Quantity,
                AvailableQuantity = s.AvailableQuantity,
                AverageCost = s.Product.AverageCost,
                TotalValuation = s.Quantity * s.Product.AverageCost
            })
            .ToList();

        return new WarehouseStockReportDto
        {
            WarehouseCode = warehouse.Code,
            WarehouseName = warehouse.Name,
            DistinctProductsCount = items.Count,
            TotalValuation = items.Sum(i => i.TotalValuation),
            Items = items
        };
    }

    public async Task<SupplierBalancesReportDto> GetSupplierBalancesReportAsync(SupplierBalancesReportFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.Suppliers.AsNoTracking().Where(s => s.IsActive);

        if (filter.OnlyWithBalance == true)
            query = query.Where(s => s.Balance > 0);

        if (filter.MinBalance.HasValue)
            query = query.Where(s => s.Balance >= filter.MinBalance.Value);

        var suppliers = await query.OrderByDescending(s => s.Balance).ToListAsync(ct);

        var items = suppliers.Select(s => new SupplierBalancesReportItemDto
        {
            SupplierCode = s.Code,
            SupplierName = s.Name,
            Phone = s.Phone,
            Balance = s.Balance
        }).ToList();

        return new SupplierBalancesReportDto
        {
            TotalSuppliers = suppliers.Count,
            TotalOutstandingDebt = suppliers.Sum(s => s.Balance),
            Items = items
        };
    }

    public async Task<CustomerBalancesReportDto> GetCustomerBalancesReportAsync(CustomerBalancesReportFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.Customers.AsNoTracking().Where(c => c.IsActive);

        if (filter.OnlyWithBalance == true)
            query = query.Where(c => c.Balance > 0);

        if (filter.MinBalance.HasValue)
            query = query.Where(c => c.Balance >= filter.MinBalance.Value);

        var customers = await query.OrderByDescending(c => c.Balance).ToListAsync(ct);

        var items = customers.Select(c => new CustomerBalancesReportItemDto
        {
            CustomerCode = c.Code,
            CustomerName = c.Name,
            Phone = c.Phone,
            CreditLimit = c.CreditLimit,
            Balance = c.Balance
        }).ToList();

        return new CustomerBalancesReportDto
        {
            TotalCustomers = customers.Count,
            TotalOutstandingReceivables = customers.Sum(c => c.Balance),
            Items = items
        };
    }

    public async Task<ExpensesReportDto> GetExpensesReportAsync(ExpensesReportFilterDto filter, CancellationToken ct = default)
    {
        var start = filter.StartDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = filter.EndDate ?? DateTime.UtcNow;

        var query = _context.OperatingExpenses
            .AsNoTracking()
            .Include(e => e.Treasury)
            .Where(e => e.ExpenseDate >= start && e.ExpenseDate <= end);

        if (!string.IsNullOrWhiteSpace(filter.Category))
            query = query.Where(e => e.Category == filter.Category);

        if (!string.IsNullOrWhiteSpace(filter.TreasuryCode))
            query = query.Where(e => e.Treasury != null && e.Treasury.Code == filter.TreasuryCode);

        var expenses = await query.OrderByDescending(e => e.ExpenseDate).ToListAsync(ct);

        var totalAmount = expenses.Sum(e => e.Amount);

        var breakdown = expenses
            .GroupBy(e => e.Category)
            .Select(g => new ExpensesReportCategorySummaryDto
            {
                Category = g.Key,
                TotalAmount = g.Sum(x => x.Amount),
                Percentage = totalAmount > 0 ? Math.Round((g.Sum(x => x.Amount) / totalAmount) * 100, 1) : 0
            })
            .OrderByDescending(b => b.TotalAmount)
            .ToList();

        var items = expenses.Select(e => new ExpensesReportItemDto
        {
            ExpenseNumber = e.ExpenseNumber,
            ExpenseDate = e.ExpenseDate,
            Category = e.Category,
            Amount = e.Amount,
            Description = e.Description,
            PaidTo = e.PaidTo,
            TreasuryName = e.Treasury?.Name ?? ""
        }).ToList();

        return new ExpensesReportDto
        {
            PeriodStart = start,
            PeriodEnd = end,
            TotalExpensesCount = expenses.Count,
            TotalExpensesAmount = totalAmount,
            CategoryBreakdown = breakdown,
            Items = items
        };
    }

    public async Task<ProfitLossReportDto> GetProfitLossReportAsync(ProfitLossReportFilterDto filter, CancellationToken ct = default)
    {
        var start = filter.StartDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = filter.EndDate ?? DateTime.UtcNow;

        // إجمالي المبيعات وتكلفة البضاعة المباعة
        var invoices = await _context.SalesInvoices
            .AsNoTracking()
            .Include(i => i.Items)
            .Where(i => i.InvoiceDate >= start && i.InvoiceDate <= end && i.Status != SalesInvoiceStatus.Cancelled)
            .ToListAsync(ct);

        var totalRevenue = invoices.Sum(i => i.TotalAmount);
        var totalCOGS = invoices.Sum(i => i.Items.Sum(item => item.Quantity * item.UnitCost));

        // إجمالي المصروفات التشغيلية
        var expenses = await _context.OperatingExpenses
            .AsNoTracking()
            .Where(e => e.ExpenseDate >= start && e.ExpenseDate <= end)
            .SumAsync(e => e.Amount, ct);

        return new ProfitLossReportDto
        {
            PeriodStart = start,
            PeriodEnd = end,
            TotalRevenue = totalRevenue,
            TotalCOGS = totalCOGS,
            OperatingExpenses = expenses
        };
    }
}
