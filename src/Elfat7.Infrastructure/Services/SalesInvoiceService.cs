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
            .Include(si => si.Warehouse)
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
                WarehouseCode = si.Warehouse != null ? si.Warehouse.Code : "",
                WarehouseName = si.Warehouse != null ? si.Warehouse.Name : "",
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
            .Include(si => si.Warehouse)
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
                WarehouseCode = si.Warehouse != null ? si.Warehouse.Code : "",
                WarehouseName = si.Warehouse != null ? si.Warehouse.Name : "",
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
            .Include(s => s.Warehouse)
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
            WarehouseCode = inv.Warehouse?.Code ?? "",
            WarehouseName = inv.Warehouse?.Name ?? "",
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
                Unit = !string.IsNullOrWhiteSpace(i.Unit) ? i.Unit : (i.Product?.Unit ?? ""),
                ConversionFactor = i.ConversionFactor > 0 ? i.ConversionFactor : 1,
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

        Warehouse? warehouse = null;
        if (!string.IsNullOrWhiteSpace(dto.WarehouseCode))
        {
            warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.Code == dto.WarehouseCode, ct);
            if (warehouse == null) return Result<SalesInvoiceDto>.Failure("مستودع الصرف المحدد غير موجود");
        }
        else
        {
            warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.IsActive, ct);
        }

        var shouldDeductStock = dto.AutoConfirm && warehouse != null;

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
        var stockMovements = new List<StockMovement>();

        var movCount = await _context.StockMovements.CountAsync(ct);

        foreach (var item in dto.Items)
        {
            var product = await _context.Products
                .Include(p => p.WarehouseStocks)
                .FirstOrDefaultAsync(p => p.Code == item.ProductCode, ct);
            if (product == null) return Result<SalesInvoiceDto>.Failure($"الصنف {item.ProductCode} غير موجود");

            var lineTotal = (item.Quantity * item.UnitPrice) - item.DiscountAmount;
            subTotal += (item.Quantity * item.UnitPrice);

            var itemUnit = !string.IsNullOrWhiteSpace(item.Unit) ? item.Unit.Trim() : product.Unit;
            var convFactor = item.ConversionFactor.HasValue && item.ConversionFactor.Value > 0 ? item.ConversionFactor.Value : 1;
            var baseDeductQty = item.Quantity * convFactor;

            if (shouldDeductStock && warehouse != null)
            {
                // التحقق من توافر الرصيد الفعلي في المستودع المحدد
                var stock = await _context.WarehouseStocks
                    .FirstOrDefaultAsync(ws => ws.WarehouseId == warehouse.Id && ws.ProductId == product.Id, ct);

                if (stock == null || stock.AvailableQuantity < baseDeductQty)
                {
                    var available = stock?.AvailableQuantity ?? 0;
                    return Result<SalesInvoiceDto>.Failure(
                        $"الرصيد المتاح من صنف ({product.Name}) في مستودع ({warehouse.Name}) غير كافٍ. " +
                        $"المتاح حالياً: {available} {product.Unit} فقط، والمطلوب صرفه: {item.Quantity} {itemUnit} (= {baseDeductQty} {product.Unit})");
                }

                // خصم الكمية من رصيد المستودع بالوحدة الأساسية
                stock.Quantity -= baseDeductQty;
                stock.LastUpdated = DateTime.UtcNow;

                // تسجيل حركة المخزن بالسالب (صرف مبيعات)
                stockMovements.Add(new StockMovement
                {
                    MovementNumber = $"MOV-{DateTime.UtcNow.Year}-{(movCount + stockMovements.Count + 1):D5}",
                    WarehouseId = warehouse.Id,
                    ProductId = product.Id,
                    MovementType = StockMovementType.SalesDelivery,
                    Quantity = -baseDeductQty,
                    UnitCost = product.AverageCost,
                    TotalCost = baseDeductQty * product.AverageCost,
                    ReferenceNumber = invoiceNumber,
                    Notes = $"صرف مبيعات بموجب فاتورة رقم {invoiceNumber} للعميل {customer.Name} ({item.Quantity} {itemUnit})"
                });
            }

            items.Add(new SalesInvoiceItem
            {
                ProductId = product.Id,
                Unit = itemUnit,
                ConversionFactor = convFactor,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                UnitCost = product.AverageCost * convFactor,
                DiscountAmount = item.DiscountAmount,
                LineTotal = lineTotal > 0 ? lineTotal : 0,
                DeliveredQuantity = shouldDeductStock ? item.Quantity : 0
            });
        }

        var totalAmount = subTotal - dto.DiscountAmount;
        if (totalAmount < 0) totalAmount = 0;

        var paidAmount = dto.PaidAmount > 0 ? dto.PaidAmount : 0;
        var remaining = totalAmount - paidAmount;
        var paymentStatus = remaining <= 0
            ? InvoicePaymentStatus.FullyPaid
            : paidAmount > 0
                ? InvoicePaymentStatus.PartiallyPaid
                : InvoicePaymentStatus.Unpaid;

        var status = dto.AutoConfirm ? SalesInvoiceStatus.Confirmed : SalesInvoiceStatus.Draft;

        var invoice = new SalesInvoice
        {
            InvoiceNumber = invoiceNumber,
            CustomerId = customer.Id,
            WarehouseId = warehouse?.Id,
            SalesOrderId = salesOrder?.Id,
            InvoiceDate = dto.InvoiceDate ?? DateTime.UtcNow,
            DueDate = dto.DueDate,
            Status = status,
            PaymentStatus = dto.AutoConfirm ? paymentStatus : InvoicePaymentStatus.Unpaid,
            SubTotal = subTotal,
            DiscountAmount = dto.DiscountAmount,
            TotalAmount = totalAmount,
            PaidAmount = dto.AutoConfirm ? paidAmount : 0,
            Notes = dto.Notes?.Trim(),
            Items = items
        };

        // تسجيل المديونية الصافية المتبقية على رصيد العميل إذا كانت الفاتورة مؤكدة
        if (dto.AutoConfirm && remaining > 0)
        {
            customer.Balance += remaining;
        }

        await _context.SalesInvoices.AddAsync(invoice, ct);
        if (stockMovements.Count > 0)
        {
            await _context.StockMovements.AddRangeAsync(stockMovements, ct);
        }

        await _context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return Result<SalesInvoiceDto>.Success(new SalesInvoiceDto
        {
            InvoiceNumber = invoice.InvoiceNumber,
            CustomerCode = customer.Code,
            CustomerName = customer.Name,
            WarehouseCode = warehouse?.Code ?? "",
            WarehouseName = warehouse?.Name ?? "",
            SalesOrderNumber = salesOrder?.OrderNumber,
            InvoiceDate = invoice.InvoiceDate,
            DueDate = invoice.DueDate,
            Status = invoice.Status,
            PaymentStatus = invoice.PaymentStatus,
            SubTotal = invoice.SubTotal,
            DiscountAmount = invoice.DiscountAmount,
            TotalAmount = invoice.TotalAmount,
            PaidAmount = invoice.PaidAmount,
            ItemsCount = invoice.Items.Count
        }, "تم إنشاء وتأكيد فاتورة المبيعات وخصم الكميات المباعة من رصيد المستودع بنجاح");
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

        if (invoice.Customer != null)
        {
            var remaining = invoice.TotalAmount - invoice.PaidAmount;
            if (remaining > 0) invoice.Customer.Balance += remaining;
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
            .Include(i => i.Items)
            .ThenInclude(it => it.Product)
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber, ct);

        if (invoice == null) return Result.Failure("فاتورة المبيعات غير موجودة");

        if (invoice.Deliveries.Any(d => d.Status != DeliveryStatus.Cancelled))
            return Result.Failure("لا يمكن إلغاء فاتورة تم صرف أذون تسليم منفصلة مرتبطة بها للمخزن");

        if (invoice.Payments.Any())
            return Result.Failure("لا يمكن إلغاء فاتورة تم تسجيل دفعات سداد نقدية عليها");

        // استرجاع المخزون المخصوم إذا كانت الفاتورة مؤكدة
        if (invoice.Status == SalesInvoiceStatus.Confirmed && invoice.WarehouseId.HasValue)
        {
            var movCount = await _context.StockMovements.CountAsync(ct);
            var restockMovements = new List<StockMovement>();

            foreach (var item in invoice.Items)
            {
                var convFactor = item.ConversionFactor > 0 ? item.ConversionFactor : 1;
                var baseQty = item.Quantity * convFactor;

                var stock = await _context.WarehouseStocks
                    .FirstOrDefaultAsync(ws => ws.WarehouseId == invoice.WarehouseId.Value && ws.ProductId == item.ProductId, ct);
                if (stock != null)
                {
                    stock.Quantity += baseQty;
                    stock.LastUpdated = DateTime.UtcNow;
                }

                restockMovements.Add(new StockMovement
                {
                    MovementNumber = $"MOV-{DateTime.UtcNow.Year}-{(movCount + restockMovements.Count + 1):D5}",
                    WarehouseId = invoice.WarehouseId.Value,
                    ProductId = item.ProductId,
                    MovementType = StockMovementType.SalesReturn,
                    Quantity = baseQty,
                    UnitCost = item.UnitCost,
                    TotalCost = baseQty * item.UnitCost,
                    ReferenceNumber = invoice.InvoiceNumber,
                    Notes = $"إلغاء فاتورة مبيعات رقم {invoice.InvoiceNumber} وإعادة بضاعة للمستودع"
                });
            }

            if (restockMovements.Count > 0)
            {
                await _context.StockMovements.AddRangeAsync(restockMovements, ct);
            }
        }

        // إذا كانت الفاتورة مؤكدة وتم تسجيل قيمتها على العميل، نخصمها من رصيد العميل
        if (invoice.Status == SalesInvoiceStatus.Confirmed && invoice.Customer != null)
        {
            var remaining = invoice.TotalAmount - invoice.PaidAmount;
            if (remaining > 0)
            {
                invoice.Customer.Balance -= remaining;
            }
        }

        invoice.Status = SalesInvoiceStatus.Cancelled;

        await _context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return Result.Success("تم إلغاء فاتورة المبيعات بنجاح واسترجاع البضاعة لرصيد المستودع وتعديل رصيد العميل");
    }
}
