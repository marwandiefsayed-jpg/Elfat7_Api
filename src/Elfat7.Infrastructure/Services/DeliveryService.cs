using Elfat7.Application.Common;
using Elfat7.Application.DTOs.Deliveries;
using Elfat7.Application.Interfaces;
using Elfat7.Domain.Entities;
using Elfat7.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Elfat7.Infrastructure.Services;

public class DeliveryService : IDeliveryService
{
    private readonly IApplicationDbContext _context;

    public DeliveryService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<DeliveryDto>> GetDeliveriesAsync(DeliveryFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.Deliveries
            .AsNoTracking()
            .Include(d => d.SalesInvoice!)
            .ThenInclude(i => i.Customer)
            .Include(d => d.Warehouse)
            .Include(d => d.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(d => d.DeliveryNumber.Contains(term) ||
                                     (d.SalesInvoice != null && d.SalesInvoice.InvoiceNumber.Contains(term)) ||
                                     (d.SalesInvoice != null && d.SalesInvoice.Customer != null && d.SalesInvoice.Customer.Name.Contains(term)) ||
                                     (d.DriverName != null && d.DriverName.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(filter.SalesInvoiceNumber))
            query = query.Where(d => d.SalesInvoice != null && d.SalesInvoice.InvoiceNumber == filter.SalesInvoiceNumber);

        if (!string.IsNullOrWhiteSpace(filter.WarehouseCode))
            query = query.Where(d => d.Warehouse != null && d.Warehouse.Code == filter.WarehouseCode);

        if (filter.Status.HasValue)
            query = query.Where(d => d.Status == filter.Status.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(d => d.DeliveryDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(d => d.DeliveryDate <= filter.EndDate.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(d => d.Id)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(d => new DeliveryDto
            {
                DeliveryNumber = d.DeliveryNumber,
                SalesInvoiceNumber = d.SalesInvoice != null ? d.SalesInvoice.InvoiceNumber : "",
                CustomerName = d.SalesInvoice != null && d.SalesInvoice.Customer != null ? d.SalesInvoice.Customer.Name : "",
                WarehouseCode = d.Warehouse != null ? d.Warehouse.Code : "",
                WarehouseName = d.Warehouse != null ? d.Warehouse.Name : "",
                DeliveryDate = d.DeliveryDate,
                Status = d.Status,
                DriverName = d.DriverName,
                VehicleNumber = d.VehicleNumber,
                ItemsCount = d.Items.Count
            })
            .ToListAsync(ct);

        return new PagedResult<DeliveryDto>(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<List<DeliveryDto>> GetAllFilteredDeliveriesAsync(DeliveryFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.Deliveries
            .AsNoTracking()
            .Include(d => d.SalesInvoice!)
            .ThenInclude(i => i.Customer)
            .Include(d => d.Warehouse)
            .Include(d => d.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(d => d.DeliveryNumber.Contains(term) ||
                                     (d.SalesInvoice != null && d.SalesInvoice.InvoiceNumber.Contains(term)) ||
                                     (d.SalesInvoice != null && d.SalesInvoice.Customer != null && d.SalesInvoice.Customer.Name.Contains(term)) ||
                                     (d.DriverName != null && d.DriverName.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(filter.SalesInvoiceNumber))
            query = query.Where(d => d.SalesInvoice != null && d.SalesInvoice.InvoiceNumber == filter.SalesInvoiceNumber);

        if (!string.IsNullOrWhiteSpace(filter.WarehouseCode))
            query = query.Where(d => d.Warehouse != null && d.Warehouse.Code == filter.WarehouseCode);

        if (filter.Status.HasValue)
            query = query.Where(d => d.Status == filter.Status.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(d => d.DeliveryDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(d => d.DeliveryDate <= filter.EndDate.Value);

        return await query
            .OrderByDescending(d => d.Id)
            .Select(d => new DeliveryDto
            {
                DeliveryNumber = d.DeliveryNumber,
                SalesInvoiceNumber = d.SalesInvoice != null ? d.SalesInvoice.InvoiceNumber : "",
                CustomerName = d.SalesInvoice != null && d.SalesInvoice.Customer != null ? d.SalesInvoice.Customer.Name : "",
                WarehouseCode = d.Warehouse != null ? d.Warehouse.Code : "",
                WarehouseName = d.Warehouse != null ? d.Warehouse.Name : "",
                DeliveryDate = d.DeliveryDate,
                Status = d.Status,
                DriverName = d.DriverName,
                VehicleNumber = d.VehicleNumber,
                ItemsCount = d.Items.Count
            })
            .ToListAsync(ct);
    }

    public async Task<DeliveryDetailsDto?> GetDeliveryByNumberAsync(string deliveryNumber, CancellationToken ct = default)
    {
        var del = await _context.Deliveries
            .AsNoTracking()
            .Include(d => d.SalesInvoice!)
            .ThenInclude(i => i.Customer)
            .Include(d => d.Warehouse)
            .Include(d => d.Items)
            .ThenInclude(di => di.Product)
            .FirstOrDefaultAsync(d => d.DeliveryNumber == deliveryNumber, ct);

        if (del == null) return null;

        return new DeliveryDetailsDto
        {
            DeliveryNumber = del.DeliveryNumber,
            SalesInvoiceNumber = del.SalesInvoice?.InvoiceNumber ?? "",
            CustomerName = del.SalesInvoice?.Customer?.Name ?? "",
            WarehouseCode = del.Warehouse?.Code ?? "",
            WarehouseName = del.Warehouse?.Name ?? "",
            DeliveryDate = del.DeliveryDate,
            Status = del.Status,
            DriverName = del.DriverName,
            VehicleNumber = del.VehicleNumber,
            Notes = del.Notes,
            ItemsCount = del.Items.Count,
            Items = del.Items.Select(i => new DeliveryItemDto
            {
                ProductCode = i.Product?.Code ?? "",
                ProductName = i.Product?.Name ?? "",
                Unit = i.Product?.Unit ?? "",
                Quantity = i.Quantity
            }).ToList()
        };
    }

    public async Task<Result<DeliveryDto>> CreateDeliveryAsync(CreateDeliveryDto dto, CancellationToken ct = default)
    {
        using var transaction = await _context.BeginTransactionAsync(ct);

        var invoice = await _context.SalesInvoices
            .Include(i => i.Items)
            .Include(i => i.Customer)
            .FirstOrDefaultAsync(i => i.InvoiceNumber == dto.SalesInvoiceNumber, ct);

        if (invoice == null) return Result<DeliveryDto>.Failure("فاتورة المبيعات المحددة غير موجودة");

        if (invoice.Status != SalesInvoiceStatus.Confirmed && invoice.Status != SalesInvoiceStatus.PartiallyDelivered)
            return Result<DeliveryDto>.Failure($"لا يمكن إنشاء إذن صرف وتسليم لفاتورة بحالة {invoice.Status.ToArabic()} (يجب تأكيد الفاتورة أولاً)");

        var warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.Code == dto.WarehouseCode, ct);
        if (warehouse == null) return Result<DeliveryDto>.Failure("مخزن الصرف والتسليم غير موجود");

        var count = await _context.Deliveries.CountAsync(ct);
        var deliveryNumber = $"DEL-{DateTime.UtcNow.Year}-{(count + 1):D4}";

        var delivery = new Delivery
        {
            DeliveryNumber = deliveryNumber,
            SalesInvoiceId = invoice.Id,
            WarehouseId = warehouse.Id,
            DeliveryDate = dto.DeliveryDate ?? DateTime.UtcNow,
            Status = DeliveryStatus.Delivered,
            DriverName = dto.DriverName?.Trim(),
            VehicleNumber = dto.VehicleNumber?.Trim(),
            Notes = dto.Notes?.Trim()
        };

        var deliveryItems = new List<DeliveryItem>();
        var stockMovements = new List<StockMovement>();

        foreach (var item in dto.Items)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Code == item.ProductCode, ct);
            if (product == null) return Result<DeliveryDto>.Failure($"الصنف {item.ProductCode} غير موجود");

            var invoiceItem = invoice.Items.FirstOrDefault(i => i.ProductId == product.Id);
            if (invoiceItem == null) return Result<DeliveryDto>.Failure($"الصنف {product.Name} غير موجود في فاتورة المبيعات");

            if (invoiceItem.RemainingQuantity < item.Quantity)
            {
                return Result<DeliveryDto>.Failure($"الكمية المصروفة ({item.Quantity}) تتجاوز الكمية المتبقية للتسليم في الفاتورة ({invoiceItem.RemainingQuantity}) للصنف {product.Name}");
            }

            // التحقق من كفاية المخزون الفعلي في المخزن المحدد بالوحدة الأساسية (معامل التحويل)
            var convFactor = invoiceItem.ConversionFactor > 0 ? invoiceItem.ConversionFactor : 1;
            var baseDeductQty = item.Quantity * convFactor;

            var stock = await _context.WarehouseStocks
                .FirstOrDefaultAsync(ws => ws.WarehouseId == warehouse.Id && ws.ProductId == product.Id, ct);

            if (stock == null || stock.AvailableQuantity < baseDeductQty)
            {
                return Result<DeliveryDto>.Failure($"الرصيد المتاح من صنف {product.Name} في مخزن {warehouse.Name} غير كافٍ. المتاح حالياً: {(stock?.AvailableQuantity ?? 0)} {product.Unit} فقط (المطلوب: {baseDeductQty} {product.Unit})");
            }

            // خصم الكمية من المخزن بالوحدة الأساسية
            stock.Quantity -= baseDeductQty;
            stock.LastUpdated = DateTime.UtcNow;

            // تحديث الكمية المسلمة في الفاتورة بوحدة الفاتورة
            invoiceItem.DeliveredQuantity += item.Quantity;

            deliveryItems.Add(new DeliveryItem
            {
                Delivery = delivery,
                ProductId = product.Id,
                Quantity = item.Quantity
            });

            var movCount = await _context.StockMovements.CountAsync(ct);
            stockMovements.Add(new StockMovement
            {
                MovementNumber = $"MOV-{DateTime.UtcNow.Year}-{(movCount + stockMovements.Count + 1):D5}",
                WarehouseId = warehouse.Id,
                ProductId = product.Id,
                MovementType = StockMovementType.SalesDelivery,
                Quantity = -baseDeductQty, // خصم بالوحدة الأساسية للمخزن
                UnitCost = invoiceItem.UnitCost,
                TotalCost = baseDeductQty * invoiceItem.UnitCost,
                ReferenceNumber = deliveryNumber,
                Notes = $"صرف وتسليم بضاعة للعميل بموجب فاتورة {invoice.InvoiceNumber} ({item.Quantity} {invoiceItem.Unit ?? product.Unit})"
            });
        }

        delivery.Items = deliveryItems;

        // تحديث حالة تسليم الفاتورة
        bool allDelivered = invoice.Items.All(i => i.RemainingQuantity == 0);
        invoice.Status = allDelivered ? SalesInvoiceStatus.FullyDelivered : SalesInvoiceStatus.PartiallyDelivered;

        await _context.Deliveries.AddAsync(delivery, ct);
        await _context.StockMovements.AddRangeAsync(stockMovements, ct);

        await _context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return Result<DeliveryDto>.Success(new DeliveryDto
        {
            DeliveryNumber = delivery.DeliveryNumber,
            SalesInvoiceNumber = invoice.InvoiceNumber,
            CustomerName = invoice.Customer?.Name ?? "",
            WarehouseCode = warehouse.Code,
            WarehouseName = warehouse.Name,
            DeliveryDate = delivery.DeliveryDate,
            Status = delivery.Status,
            DriverName = delivery.DriverName,
            VehicleNumber = delivery.VehicleNumber,
            ItemsCount = delivery.Items.Count
        }, "تم صرف البضاعة من المخزن وتسليمها للعميل بنجاح وخصم المخزون");
    }

    public async Task<Result> MarkAsDeliveredAsync(string deliveryNumber, CancellationToken ct = default)
    {
        var delivery = await _context.Deliveries.FirstOrDefaultAsync(d => d.DeliveryNumber == deliveryNumber, ct);
        if (delivery == null) return Result.Failure("إذن التسليم غير موجود");

        delivery.Status = DeliveryStatus.Delivered;
        await _context.SaveChangesAsync(ct);

        return Result.Success("تم تأكيد وصول وتسليم البضاعة للعميل");
    }
}
