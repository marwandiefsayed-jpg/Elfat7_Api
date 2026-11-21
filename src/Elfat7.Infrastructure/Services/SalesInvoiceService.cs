using Elfat7.Application.Common;
using Elfat7.Application.DTOs.Sales;
using Elfat7.Application.Interfaces;
using Elfat7.Domain.Entities;
using Elfat7.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Elfat7.Infrastructure.Services;

public class SalesInvoiceService : ISalesInvoiceService
{
    private readonly IApplicationDbContext _context;

    public SalesInvoiceService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<SalesInvoiceDto>> GetSalesInvoicesAsync(SalesInvoiceFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.SalesInvoices
            .AsNoTracking()
            .Include(si => si.Customer)
            .Include(si => si.SalesOrder)
            .Include(si => si.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(si => si.InvoiceNumber.Contains(term) || (si.Customer != null && si.Customer.Name.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(filter.CustomerCode))
            query = query.Where(si => si.Customer != null && si.Customer.Code == filter.CustomerCode);

        if (filter.Status.HasValue)
            query = query.Where(si => si.Status == filter.Status.Value);

        if (filter.PaymentStatus.HasValue)
            query = query.Where(si => si.PaymentStatus == filter.PaymentStatus.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(si => si.InvoiceDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(si => si.InvoiceDate <= filter.EndDate.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(si => si.Id)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(si => new SalesInvoiceDto
            {
                InvoiceNumber = si.InvoiceNumber,
                CustomerCode = si.Customer != null ? si.Customer.Code : "",
                CustomerName = si.Customer != null ? si.Customer.Name : "",
                SalesOrderNumber = si.SalesOrder != null ? si.SalesOrder.OrderNumber : null,
                InvoiceDate = si.InvoiceDate,
                DueDate = si.DueDate,
                Status = si.Status,
                PaymentStatus = si.PaymentStatus,
                SubTotal = si.SubTotal,
                DiscountAmount = si.DiscountAmount,
                TotalAmount = si.TotalAmount,
                PaidAmount = si.PaidAmount,
                ItemsCount = si.Items.Count
            })
            .ToListAsync(ct);

        return new PagedResult<SalesInvoiceDto>(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<List<SalesInvoiceDto>> GetAllFilteredSalesInvoicesAsync(SalesInvoiceFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.SalesInvoices
            .AsNoTracking()
            .Include(si => si.Customer)
            .Include(si => si.SalesOrder)
            .Include(si => si.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(si => si.InvoiceNumber.Contains(term) || (si.Customer != null && si.Customer.Name.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(filter.CustomerCode))
            query = query.Where(si => si.Customer != null && si.Customer.Code == filter.CustomerCode);

        if (filter.Status.HasValue)
            query = query.Where(si => si.Status == filter.Status.Value);

        if (filter.PaymentStatus.HasValue)
            query = query.Where(si => si.PaymentStatus == filter.PaymentStatus.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(si => si.InvoiceDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(si => si.InvoiceDate <= filter.EndDate.Value);

        return await query
            .OrderByDescending(si => si.Id)
            .Select(si => new SalesInvoiceDto
            {
                InvoiceNumber = si.InvoiceNumber,
                CustomerCode = si.Customer != null ? si.Customer.Code : "",
                CustomerName = si.Customer != null ? si.Customer.Name : "",
                SalesOrderNumber = si.SalesOrder != null ? si.SalesOrder.OrderNumber : null,
                InvoiceDate = si.InvoiceDate,
                DueDate = si.DueDate,
                Status = si.Status,
                PaymentStatus = si.PaymentStatus,
                SubTotal = si.SubTotal,
                DiscountAmount = si.DiscountAmount,
                TotalAmount = si.TotalAmount,
                PaidAmount = si.PaidAmount,
                ItemsCount = si.Items.Count
            })
            .ToListAsync(ct);
    }

    public async Task<SalesInvoiceDetailsDto?> GetSalesInvoiceByNumberAsync(string invoiceNumber, CancellationToken ct = default)
    {
        var inv = await _context.SalesInvoices
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.SalesOrder)
            .Include(s => s.Items)
            .ThenInclude(i => i.Product)
            .Include(s => s.Deliveries)
            .ThenInclude(d => d.Warehouse)
            .Include(s => s.Returns)
            .FirstOrDefaultAsync(s => s.InvoiceNumber == invoiceNumber, ct);

        if (inv == null) return null;

        return new SalesInvoiceDetailsDto
        {
            InvoiceNumber = inv.InvoiceNumber,
            CustomerCode = inv.Customer?.Code ?? "",
            CustomerName = inv.Customer?.Name ?? "",
            SalesOrderNumber = inv.SalesOrder?.OrderNumber,
            InvoiceDate = inv.InvoiceDate,
            DueDate = inv.DueDate,
            Status = inv.Status,
            PaymentStatus = inv.PaymentStatus,
            SubTotal = inv.SubTotal,
            DiscountAmount = inv.DiscountAmount,
            TotalAmount = inv.TotalAmount,
            PaidAmount = inv.PaidAmount,
            Notes = inv.Notes,
            ItemsCount = inv.Items.Count,
            Items = inv.Items.Select(i => new SalesInvoiceItemDto
            {
                ProductCode = i.Product?.Code ?? "",
                ProductName = i.Product?.Name ?? "",
                Unit = i.Product?.Unit ?? "",
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                UnitCost = i.UnitCost,
                DiscountAmount = i.DiscountAmount,
                LineTotal = i.LineTotal,
                DeliveredQuantity = i.DeliveredQuantity
            }).ToList(),
            Deliveries = inv.Deliveries.Select(d => new DeliverySummaryDto
            {
                DeliveryNumber = d.DeliveryNumber,
                WarehouseName = d.Warehouse?.Name ?? "",
                DeliveryDate = d.DeliveryDate,
                StatusArabic = d.Status.ToArabic()
            }).ToList(),
            Returns = inv.Returns.Select(r => new SalesReturnSummaryDto
            {
                ReturnNumber = r.ReturnNumber,
                ReturnDate = r.ReturnDate,
                TotalAmount = r.TotalAmount,
                Reason = r.Reason
            }).ToList()
        };
    }

    public async Task<Result<SalesInvoiceDto>> CreateSalesInvoiceAsync(CreateSalesInvoiceDto dto, CancellationToken ct = default)
    {
        using var transaction = await _context.BeginTransactionAsync(ct);

        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Code == dto.CustomerCode, ct);
        if (customer == null) return Result<SalesInvoiceDto>.Failure("العميل المحدد غير موجود");

        SalesOrder? salesOrder = null;
        if (!string.IsNullOrWhiteSpace(dto.SalesOrderNumber))
        {
            salesOrder = await _context.SalesOrders.FirstOrDefaultAsync(so => so.OrderNumber == dto.SalesOrderNumber, ct);
            if (salesOrder == null) return Result<SalesInvoiceDto>.Failure("أمر البيع المحدد غير موجود");
        }

        var count = await _context.SalesInvoices.CountAsync(ct);
        var invoiceNumber = $"INV-{DateTime.UtcNow.Year}-{(count + 1):D4}";

        decimal subTotal = 0;
        var items = new List<SalesInvoiceItem>();

        foreach (var item in dto.Items)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Code == item.ProductCode, ct);
            if (product == null) return Result<SalesInvoiceDto>.Failure($"الصنف {item.ProductCode} غير موجود");

            var lineTotal = (item.Quantity * item.UnitPrice) - item.DiscountAmount;
            subTotal += (item.Quantity * item.UnitPrice);

            items.Add(new SalesInvoiceItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                UnitCost = product.AverageCost, // تسجيل متوسط التكلفة وقت البيع
                DiscountAmount = item.DiscountAmount,
                LineTotal = lineTotal > 0 ? lineTotal : 0,
                DeliveredQuantity = 0
            });
        }

        var totalAmount = subTotal - dto.DiscountAmount;
        if (totalAmount < 0) totalAmount = 0;

        var status = dto.AutoConfirm ? SalesInvoiceStatus.Confirmed : SalesInvoiceStatus.Draft;

        var invoice = new SalesInvoice
        {
            InvoiceNumber = invoiceNumber,
            CustomerId = customer.Id,
            SalesOrderId = salesOrder?.Id,
            InvoiceDate = dto.InvoiceDate ?? DateTime.UtcNow,
            DueDate = dto.DueDate,
            Status = status,
            PaymentStatus = InvoicePaymentStatus.Unpaid,
            SubTotal = subTotal,
            DiscountAmount = dto.DiscountAmount,
            TotalAmount = totalAmount,
            PaidAmount = 0,
            Notes = dto.Notes?.Trim(),
            Items = items
        };

        // إذا تم تأكيد الفاتورة مباشرة، يتم تسجيل المديونية على رصيد العميل
        if (dto.AutoConfirm)
        {
            customer.Balance += totalAmount;
        }

        await _context.SalesInvoices.AddAsync(invoice, ct);
        await _context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return Result<SalesInvoiceDto>.Success(new SalesInvoiceDto
        {
            InvoiceNumber = invoice.InvoiceNumber,
            CustomerCode = customer.Code,
            CustomerName = customer.Name,
            SalesOrderNumber = salesOrder?.OrderNumber,
            InvoiceDate = invoice.InvoiceDate,
            DueDate = invoice.DueDate,
            Status = invoice.Status,
            PaymentStatus = invoice.PaymentStatus,
            SubTotal = invoice.SubTotal,
            DiscountAmount = invoice.DiscountAmount,
            TotalAmount = invoice.TotalAmount,
            PaidAmount = 0,
            ItemsCount = invoice.Items.Count
        }, dto.AutoConfirm ? "تم إنشاء وتأكيد فاتورة المبيعات وتسجيل المديونية على حساب العميل بنجاح"
                            : "تم إنشاء فاتورة المبيعات كمسودة بنجاح (دون التأثير على المخزون أو رصيد العميل)");
    }

    public async Task<Result> ConfirmSalesInvoiceAsync(string invoiceNumber, CancellationToken ct = default)
    {
        var invoice = await _context.SalesInvoices
            .Include(i => i.Customer)
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber, ct);

        if (invoice == null) return Result.Failure("فاتورة المبيعات غير موجودة");

        if (invoice.Status != SalesInvoiceStatus.Draft)
            return Result.Failure("يمكن تأكيد الفواتير التي في حالة مسودة فقط");

        invoice.Status = SalesInvoiceStatus.Confirmed;

        // تسجيل المديونية على رصيد العميل
        if (invoice.Customer != null)
        {
            invoice.Customer.Balance += invoice.TotalAmount;
        }

        await _context.SaveChangesAsync(ct);

        return Result.Success("تم تأكيد الفاتورة بنجاح وتسجيل المديونية المالية على رصيد العميل");
    }

    public async Task<Result> CancelSalesInvoiceAsync(string invoiceNumber, CancellationToken ct = default)
    {
        using var transaction = await _context.BeginTransactionAsync(ct);

        var invoice = await _context.SalesInvoices
            .Include(i => i.Customer)
            .Include(i => i.Deliveries)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber, ct);

        if (invoice == null) return Result.Failure("فاتورة المبيعات غير موجودة");

        if (invoice.Deliveries.Any(d => d.Status != DeliveryStatus.Cancelled))
            return Result.Failure("لا يمكن إلغاء فاتورة تم صرف أذون تسليم مرتبطة بها للمخزن");

        if (invoice.Payments.Any())
            return Result.Failure("لا يمكن إلغاء فاتورة تم تسجيل دفعات سداد نقدية عليها");

        // إذا كانت الفاتورة مؤكدة وتم تسجيل قيمتها على العميل، نخصمها من رصيد العميل
        if (invoice.Status == SalesInvoiceStatus.Confirmed && invoice.Customer != null)
        {
            invoice.Customer.Balance -= invoice.TotalAmount;
        }

        invoice.Status = SalesInvoiceStatus.Cancelled;

        await _context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return Result.Success("تم إلغاء فاتورة المبيعات بنجاح وتعديل رصيد العميل");
    }
}
