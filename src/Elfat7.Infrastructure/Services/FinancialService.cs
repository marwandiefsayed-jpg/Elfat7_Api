using Elfat7.Application.Common;
using Elfat7.Application.DTOs.Financial;
using Elfat7.Application.Interfaces;
using Elfat7.Domain.Entities;
using Elfat7.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Elfat7.Infrastructure.Services;

public class FinancialService : IFinancialService
{
    private readonly IApplicationDbContext _context;

    public FinancialService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TreasuryDto>> GetTreasuriesAsync(CancellationToken ct = default)
    {
        return await _context.Treasuries
            .AsNoTracking()
            .Select(t => new TreasuryDto
            {
                Code = t.Code,
                Name = t.Name,
                Balance = t.Balance,
                IsActive = t.IsActive
            })
            .ToListAsync(ct);
    }

    public async Task<Result<TreasuryDto>> CreateTreasuryAsync(CreateTreasuryDto dto, CancellationToken ct = default)
    {
        var count = await _context.Treasuries.CountAsync(ct);
        var code = $"TRS-{(count + 1):D2}";

        var treasury = new Treasury
        {
            Code = code,
            Name = dto.Name.Trim(),
            Balance = dto.InitialBalance,
            IsActive = true
        };

        await _context.Treasuries.AddAsync(treasury, ct);

        if (dto.InitialBalance > 0)
        {
            var trxCount = await _context.TreasuryTransactions.CountAsync(ct);
            var trx = new TreasuryTransaction
            {
                TransactionNumber = $"TRX-{DateTime.UtcNow.Year}-{(trxCount + 1):D5}",
                Treasury = treasury,
                TransactionType = TreasuryTransactionType.Inflow,
                Category = TreasuryTransactionCategory.CapitalAdjustment,
                Amount = dto.InitialBalance,
                TransactionDate = DateTime.UtcNow,
                ReferenceNumber = code,
                Notes = "رصيد افتتاحي للخزينة"
            };
            await _context.TreasuryTransactions.AddAsync(trx, ct);
        }

        await _context.SaveChangesAsync(ct);

        return Result<TreasuryDto>.Success(new TreasuryDto
        {
            Code = treasury.Code,
            Name = treasury.Name,
            Balance = treasury.Balance,
            IsActive = treasury.IsActive
        }, "تم إضافة الخزينة أو الحساب البنكي بنجاح");
    }

    public async Task<PagedResult<TreasuryTransactionDto>> GetTreasuryTransactionsAsync(TreasuryFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.TreasuryTransactions
            .AsNoTracking()
            .Include(t => t.Treasury)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(t => t.TransactionNumber.Contains(term) || t.ReferenceNumber.Contains(term) ||
                                     (t.Treasury != null && t.Treasury.Name.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(filter.TreasuryCode))
            query = query.Where(t => t.Treasury != null && t.Treasury.Code == filter.TreasuryCode);

        if (filter.TransactionType.HasValue)
            query = query.Where(t => t.TransactionType == filter.TransactionType.Value);

        if (filter.Category.HasValue)
            query = query.Where(t => t.Category == filter.Category.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(t => t.TransactionDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(t => t.TransactionDate <= filter.EndDate.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(t => t.Id)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(t => new TreasuryTransactionDto
            {
                TransactionNumber = t.TransactionNumber,
                TreasuryCode = t.Treasury != null ? t.Treasury.Code : "",
                TreasuryName = t.Treasury != null ? t.Treasury.Name : "",
                TransactionType = t.TransactionType,
                Category = t.Category,
                Amount = t.Amount,
                TransactionDate = t.TransactionDate,
                ReferenceNumber = t.ReferenceNumber,
                Notes = t.Notes
            })
            .ToListAsync(ct);

        return new PagedResult<TreasuryTransactionDto>(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<List<TreasuryTransactionDto>> GetAllFilteredTreasuryTransactionsAsync(TreasuryFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.TreasuryTransactions
            .AsNoTracking()
            .Include(t => t.Treasury)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(t => t.TransactionNumber.Contains(term) || t.ReferenceNumber.Contains(term) ||
                                     (t.Treasury != null && t.Treasury.Name.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(filter.TreasuryCode))
            query = query.Where(t => t.Treasury != null && t.Treasury.Code == filter.TreasuryCode);

        if (filter.TransactionType.HasValue)
            query = query.Where(t => t.TransactionType == filter.TransactionType.Value);

        if (filter.Category.HasValue)
            query = query.Where(t => t.Category == filter.Category.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(t => t.TransactionDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(t => t.TransactionDate <= filter.EndDate.Value);

        return await query
            .OrderByDescending(t => t.Id)
            .Select(t => new TreasuryTransactionDto
            {
                TransactionNumber = t.TransactionNumber,
                TreasuryCode = t.Treasury != null ? t.Treasury.Code : "",
                TreasuryName = t.Treasury != null ? t.Treasury.Name : "",
                TransactionType = t.TransactionType,
                Category = t.Category,
                Amount = t.Amount,
                TransactionDate = t.TransactionDate,
                ReferenceNumber = t.ReferenceNumber,
                Notes = t.Notes
            })
            .ToListAsync(ct);
    }

    public async Task<PagedResult<SupplierPaymentDto>> GetSupplierPaymentsAsync(PaymentFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.SupplierPayments
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.Treasury)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(p => p.PaymentNumber.Contains(term) || (p.Supplier != null && p.Supplier.Name.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(filter.PartyCode))
            query = query.Where(p => p.Supplier != null && p.Supplier.Code == filter.PartyCode);

        if (!string.IsNullOrWhiteSpace(filter.TreasuryCode))
            query = query.Where(p => p.Treasury != null && p.Treasury.Code == filter.TreasuryCode);

        if (filter.PaymentMethod.HasValue)
            query = query.Where(p => p.PaymentMethod == filter.PaymentMethod.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(p => p.PaymentDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(p => p.PaymentDate <= filter.EndDate.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.Id)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(p => new SupplierPaymentDto
            {
                PaymentNumber = p.PaymentNumber,
                SupplierCode = p.Supplier != null ? p.Supplier.Code : "",
                SupplierName = p.Supplier != null ? p.Supplier.Name : "",
                TreasuryCode = p.Treasury != null ? p.Treasury.Code : "",
                TreasuryName = p.Treasury != null ? p.Treasury.Name : "",
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                PaymentMethod = p.PaymentMethod,
                ReferenceNumber = p.ReferenceNumber,
                Notes = p.Notes
            })
            .ToListAsync(ct);

        return new PagedResult<SupplierPaymentDto>(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<List<SupplierPaymentDto>> GetAllFilteredSupplierPaymentsAsync(PaymentFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.SupplierPayments
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.Treasury)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(p => p.PaymentNumber.Contains(term) || (p.Supplier != null && p.Supplier.Name.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(filter.PartyCode))
            query = query.Where(p => p.Supplier != null && p.Supplier.Code == filter.PartyCode);

        if (!string.IsNullOrWhiteSpace(filter.TreasuryCode))
            query = query.Where(p => p.Treasury != null && p.Treasury.Code == filter.TreasuryCode);

        if (filter.PaymentMethod.HasValue)
            query = query.Where(p => p.PaymentMethod == filter.PaymentMethod.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(p => p.PaymentDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(p => p.PaymentDate <= filter.EndDate.Value);

        return await query
            .OrderByDescending(p => p.Id)
            .Select(p => new SupplierPaymentDto
            {
                PaymentNumber = p.PaymentNumber,
                SupplierCode = p.Supplier != null ? p.Supplier.Code : "",
                SupplierName = p.Supplier != null ? p.Supplier.Name : "",
                TreasuryCode = p.Treasury != null ? p.Treasury.Code : "",
                TreasuryName = p.Treasury != null ? p.Treasury.Name : "",
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                PaymentMethod = p.PaymentMethod,
                ReferenceNumber = p.ReferenceNumber,
                Notes = p.Notes
            })
            .ToListAsync(ct);
    }

    public async Task<Result<SupplierPaymentDto>> CreateSupplierPaymentAsync(CreateSupplierPaymentDto dto, CancellationToken ct = default)
    {
        using var transaction = await _context.BeginTransactionAsync(ct);

        var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Code == dto.SupplierCode, ct);
        if (supplier == null) return Result<SupplierPaymentDto>.Failure("المورد المحدد غير موجود");

        var treasury = await _context.Treasuries.FirstOrDefaultAsync(t => t.Code == dto.TreasuryCode, ct);
        if (treasury == null) return Result<SupplierPaymentDto>.Failure("الخزينة المحددة غير موجودة");

        if (treasury.Balance < dto.Amount)
        {
            return Result<SupplierPaymentDto>.Failure($"رصيد الخزينة ({treasury.Balance:N2} ج.م) غير كافٍ لسداد المبلغ المطلوب ({dto.Amount:N2} ج.م)");
        }

        var count = await _context.SupplierPayments.CountAsync(ct);
        var paymentNumber = $"SPAY-{DateTime.UtcNow.Year}-{(count + 1):D4}";

        var payment = new SupplierPayment
        {
            PaymentNumber = paymentNumber,
            SupplierId = supplier.Id,
            TreasuryId = treasury.Id,
            Amount = dto.Amount,
            PaymentDate = dto.PaymentDate ?? DateTime.UtcNow,
            PaymentMethod = dto.PaymentMethod,
            ReferenceNumber = dto.ReferenceNumber?.Trim(),
            Notes = dto.Notes?.Trim()
        };

        // خصم من رصيد الخزينة
        treasury.Balance -= dto.Amount;

        // تخفيض رصيد مديونية المورد
        supplier.Balance -= dto.Amount;

        // تسجيل حركة الخزينة
        var trxCount = await _context.TreasuryTransactions.CountAsync(ct);
        var trx = new TreasuryTransaction
        {
            TransactionNumber = $"TRX-{DateTime.UtcNow.Year}-{(trxCount + 1):D5}",
            TreasuryId = treasury.Id,
            TransactionType = TreasuryTransactionType.Outflow,
            Category = TreasuryTransactionCategory.SupplierPayment,
            Amount = dto.Amount,
            TransactionDate = payment.PaymentDate,
            ReferenceNumber = paymentNumber,
            Notes = $"سداد دفعة للمورد: {supplier.Name}"
        };

        await _context.SupplierPayments.AddAsync(payment, ct);
        await _context.TreasuryTransactions.AddAsync(trx, ct);

        await _context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return Result<SupplierPaymentDto>.Success(new SupplierPaymentDto
        {
            PaymentNumber = payment.PaymentNumber,
            SupplierCode = supplier.Code,
            SupplierName = supplier.Name,
            TreasuryCode = treasury.Code,
            TreasuryName = treasury.Name,
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            PaymentMethod = payment.PaymentMethod,
            ReferenceNumber = payment.ReferenceNumber,
            Notes = payment.Notes
        }, "تم تسجيل سند الصرف وتخفيض رصيد المورد وخصم المبلغ من الخزينة بنجاح");
    }

    public async Task<PagedResult<CustomerPaymentDto>> GetCustomerPaymentsAsync(PaymentFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.CustomerPayments
            .AsNoTracking()
            .Include(p => p.Customer)
            .Include(p => p.Treasury)
            .Include(p => p.SalesInvoice)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(p => p.ReceiptNumber.Contains(term) || (p.Customer != null && p.Customer.Name.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(filter.PartyCode))
            query = query.Where(p => p.Customer != null && p.Customer.Code == filter.PartyCode);

        if (!string.IsNullOrWhiteSpace(filter.TreasuryCode))
            query = query.Where(p => p.Treasury != null && p.Treasury.Code == filter.TreasuryCode);

        if (filter.PaymentMethod.HasValue)
            query = query.Where(p => p.PaymentMethod == filter.PaymentMethod.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(p => p.PaymentDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(p => p.PaymentDate <= filter.EndDate.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.Id)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(p => new CustomerPaymentDto
            {
                ReceiptNumber = p.ReceiptNumber,
                CustomerCode = p.Customer != null ? p.Customer.Code : "",
                CustomerName = p.Customer != null ? p.Customer.Name : "",
                TreasuryCode = p.Treasury != null ? p.Treasury.Code : "",
                TreasuryName = p.Treasury != null ? p.Treasury.Name : "",
                SalesInvoiceNumber = p.SalesInvoice != null ? p.SalesInvoice.InvoiceNumber : null,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                PaymentMethod = p.PaymentMethod,
                ReferenceNumber = p.ReferenceNumber,
                Notes = p.Notes
            })
            .ToListAsync(ct);

        return new PagedResult<CustomerPaymentDto>(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<List<CustomerPaymentDto>> GetAllFilteredCustomerPaymentsAsync(PaymentFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.CustomerPayments
            .AsNoTracking()
            .Include(p => p.Customer)
            .Include(p => p.Treasury)
            .Include(p => p.SalesInvoice)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(p => p.ReceiptNumber.Contains(term) || (p.Customer != null && p.Customer.Name.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(filter.PartyCode))
            query = query.Where(p => p.Customer != null && p.Customer.Code == filter.PartyCode);

        if (!string.IsNullOrWhiteSpace(filter.TreasuryCode))
            query = query.Where(p => p.Treasury != null && p.Treasury.Code == filter.TreasuryCode);

        if (filter.PaymentMethod.HasValue)
            query = query.Where(p => p.PaymentMethod == filter.PaymentMethod.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(p => p.PaymentDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(p => p.PaymentDate <= filter.EndDate.Value);

        return await query
            .OrderByDescending(p => p.Id)
            .Select(p => new CustomerPaymentDto
            {
                ReceiptNumber = p.ReceiptNumber,
                CustomerCode = p.Customer != null ? p.Customer.Code : "",
                CustomerName = p.Customer != null ? p.Customer.Name : "",
                TreasuryCode = p.Treasury != null ? p.Treasury.Code : "",
                TreasuryName = p.Treasury != null ? p.Treasury.Name : "",
                SalesInvoiceNumber = p.SalesInvoice != null ? p.SalesInvoice.InvoiceNumber : null,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                PaymentMethod = p.PaymentMethod,
                ReferenceNumber = p.ReferenceNumber,
                Notes = p.Notes
            })
            .ToListAsync(ct);
    }

    public async Task<Result<CustomerPaymentDto>> CreateCustomerPaymentAsync(CreateCustomerPaymentDto dto, CancellationToken ct = default)
    {
        using var transaction = await _context.BeginTransactionAsync(ct);

        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Code == dto.CustomerCode, ct);
        if (customer == null) return Result<CustomerPaymentDto>.Failure("العميل المحدد غير موجود");

        var treasury = await _context.Treasuries.FirstOrDefaultAsync(t => t.Code == dto.TreasuryCode, ct);
        if (treasury == null) return Result<CustomerPaymentDto>.Failure("الخزينة المحددة غير موجودة");

        SalesInvoice? invoice = null;
        if (!string.IsNullOrWhiteSpace(dto.SalesInvoiceNumber))
        {
            invoice = await _context.SalesInvoices.FirstOrDefaultAsync(i => i.InvoiceNumber == dto.SalesInvoiceNumber, ct);
            if (invoice == null) return Result<CustomerPaymentDto>.Failure("فاتورة المبيعات المحددة غير موجودة");
        }

        var count = await _context.CustomerPayments.CountAsync(ct);
        var receiptNumber = $"CPAY-{DateTime.UtcNow.Year}-{(count + 1):D4}";

        var payment = new CustomerPayment
        {
            ReceiptNumber = receiptNumber,
            CustomerId = customer.Id,
            TreasuryId = treasury.Id,
            SalesInvoiceId = invoice?.Id,
            Amount = dto.Amount,
            PaymentDate = dto.PaymentDate ?? DateTime.UtcNow,
            PaymentMethod = dto.PaymentMethod,
            ReferenceNumber = dto.ReferenceNumber?.Trim(),
            Notes = dto.Notes?.Trim()
        };

        // إضافة المبلغ إلى رصيد الخزينة
        treasury.Balance += dto.Amount;

        // تخفيض مديونية العميل
        customer.Balance -= dto.Amount;

        // إذا كان التحصيل مرتبطاً بفاتورة مبيعات، تحديث المسدد منها
        if (invoice != null)
        {
            invoice.PaidAmount += dto.Amount;
            if (invoice.PaidAmount >= invoice.TotalAmount)
                invoice.PaymentStatus = InvoicePaymentStatus.FullyPaid;
            else if (invoice.PaidAmount > 0)
                invoice.PaymentStatus = InvoicePaymentStatus.PartiallyPaid;
        }

        // تسجيل حركة الخزينة
        var trxCount = await _context.TreasuryTransactions.CountAsync(ct);
        var trx = new TreasuryTransaction
        {
            TransactionNumber = $"TRX-{DateTime.UtcNow.Year}-{(trxCount + 1):D5}",
            TreasuryId = treasury.Id,
            TransactionType = TreasuryTransactionType.Inflow,
            Category = TreasuryTransactionCategory.CustomerPayment,
            Amount = dto.Amount,
            TransactionDate = payment.PaymentDate,
            ReferenceNumber = receiptNumber,
            Notes = $"تحصيل دفعة نقدية من العميل: {customer.Name}"
        };

        await _context.CustomerPayments.AddAsync(payment, ct);
        await _context.TreasuryTransactions.AddAsync(trx, ct);

        await _context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return Result<CustomerPaymentDto>.Success(new CustomerPaymentDto
        {
            ReceiptNumber = payment.ReceiptNumber,
            CustomerCode = customer.Code,
            CustomerName = customer.Name,
            TreasuryCode = treasury.Code,
            TreasuryName = treasury.Name,
            SalesInvoiceNumber = invoice?.InvoiceNumber,
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            PaymentMethod = payment.PaymentMethod,
            ReferenceNumber = payment.ReferenceNumber,
            Notes = payment.Notes
        }, "تم تسجيل سند القبض وإيداع المبلغ بالخزينة وتخفيض مديونية العميل بنجاح");
    }

    public async Task<PagedResult<OperatingExpenseDto>> GetExpensesAsync(ExpenseFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.OperatingExpenses
            .AsNoTracking()
            .Include(e => e.Treasury)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(e => e.ExpenseNumber.Contains(term) || e.Category.Contains(term) ||
                                     e.Description.Contains(term) || (e.PaidTo != null && e.PaidTo.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(filter.Category))
            query = query.Where(e => e.Category == filter.Category);

        if (!string.IsNullOrWhiteSpace(filter.TreasuryCode))
            query = query.Where(e => e.Treasury != null && e.Treasury.Code == filter.TreasuryCode);

        if (filter.StartDate.HasValue)
            query = query.Where(e => e.ExpenseDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(e => e.ExpenseDate <= filter.EndDate.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(e => e.Id)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(e => new OperatingExpenseDto
            {
                ExpenseNumber = e.ExpenseNumber,
                TreasuryCode = e.Treasury != null ? e.Treasury.Code : "",
                TreasuryName = e.Treasury != null ? e.Treasury.Name : "",
                Category = e.Category,
                Amount = e.Amount,
                ExpenseDate = e.ExpenseDate,
                Description = e.Description,
                PaidTo = e.PaidTo,
                Notes = e.Notes
            })
            .ToListAsync(ct);

        return new PagedResult<OperatingExpenseDto>(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<List<OperatingExpenseDto>> GetAllFilteredExpensesAsync(ExpenseFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.OperatingExpenses
            .AsNoTracking()
            .Include(e => e.Treasury)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(e => e.ExpenseNumber.Contains(term) || e.Category.Contains(term) ||
                                     e.Description.Contains(term) || (e.PaidTo != null && e.PaidTo.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(filter.Category))
            query = query.Where(e => e.Category == filter.Category);

        if (!string.IsNullOrWhiteSpace(filter.TreasuryCode))
            query = query.Where(e => e.Treasury != null && e.Treasury.Code == filter.TreasuryCode);

        if (filter.StartDate.HasValue)
            query = query.Where(e => e.ExpenseDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(e => e.ExpenseDate <= filter.EndDate.Value);

        return await query
            .OrderByDescending(e => e.Id)
            .Select(e => new OperatingExpenseDto
            {
                ExpenseNumber = e.ExpenseNumber,
                TreasuryCode = e.Treasury != null ? e.Treasury.Code : "",
                TreasuryName = e.Treasury != null ? e.Treasury.Name : "",
                Category = e.Category,
                Amount = e.Amount,
                ExpenseDate = e.ExpenseDate,
                Description = e.Description,
                PaidTo = e.PaidTo,
                Notes = e.Notes
            })
            .ToListAsync(ct);
    }

    public async Task<Result<OperatingExpenseDto>> CreateExpenseAsync(CreateOperatingExpenseDto dto, CancellationToken ct = default)
    {
        using var transaction = await _context.BeginTransactionAsync(ct);

        var treasury = await _context.Treasuries.FirstOrDefaultAsync(t => t.Code == dto.TreasuryCode, ct);
        if (treasury == null) return Result<OperatingExpenseDto>.Failure("الخزينة المحددة غير موجودة");

        if (treasury.Balance < dto.Amount)
        {
            return Result<OperatingExpenseDto>.Failure($"رصيد الخزينة ({treasury.Balance:N2} ج.م) غير كافٍ لصرف المصروف ({dto.Amount:N2} ج.م)");
        }

        var count = await _context.OperatingExpenses.CountAsync(ct);
        var expenseNumber = $"EXP-{DateTime.UtcNow.Year}-{(count + 1):D4}";

        var expense = new OperatingExpense
        {
            ExpenseNumber = expenseNumber,
            TreasuryId = treasury.Id,
            Category = dto.Category.Trim(),
            Amount = dto.Amount,
            ExpenseDate = dto.ExpenseDate ?? DateTime.UtcNow,
            Description = dto.Description.Trim(),
            PaidTo = dto.PaidTo?.Trim(),
            Notes = dto.Notes?.Trim()
        };

        // خصم المصروف من رصيد الخزينة
        treasury.Balance -= dto.Amount;

        // تسجيل حركة الخزينة
        var trxCount = await _context.TreasuryTransactions.CountAsync(ct);
        var trx = new TreasuryTransaction
        {
            TransactionNumber = $"TRX-{DateTime.UtcNow.Year}-{(trxCount + 1):D5}",
            TreasuryId = treasury.Id,
            TransactionType = TreasuryTransactionType.Outflow,
            Category = TreasuryTransactionCategory.OperatingExpense,
            Amount = dto.Amount,
            TransactionDate = expense.ExpenseDate,
            ReferenceNumber = expenseNumber,
            Notes = $"مصروف تشغيلي: {expense.Category} - {expense.Description}"
        };

        await _context.OperatingExpenses.AddAsync(expense, ct);
        await _context.TreasuryTransactions.AddAsync(trx, ct);

        await _context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return Result<OperatingExpenseDto>.Success(new OperatingExpenseDto
        {
            ExpenseNumber = expense.ExpenseNumber,
            TreasuryCode = treasury.Code,
            TreasuryName = treasury.Name,
            Category = expense.Category,
            Amount = expense.Amount,
            ExpenseDate = expense.ExpenseDate,
            Description = expense.Description,
            PaidTo = expense.PaidTo,
            Notes = expense.Notes
        }, "تم تسجيل المصروف التشغيلي وخصمه من الخزينة بنجاح");
    }
}
