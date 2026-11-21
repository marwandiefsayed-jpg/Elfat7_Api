using Elfat7.Application.Common;
using Elfat7.Application.DTOs.Purchasing;
using Elfat7.Application.Interfaces;
using Elfat7.Domain.Entities;
using Elfat7.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Elfat7.Infrastructure.Services;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IApplicationDbContext _context;

    public PurchaseOrderService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<PurchaseOrderDto>> GetPurchaseOrdersAsync(PurchaseOrderFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.PurchaseOrders
            .AsNoTracking()
            .Include(po => po.Supplier)
            .Include(po => po.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(po => po.OrderNumber.Contains(term) || (po.Supplier != null && po.Supplier.Name.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(filter.SupplierCode))
            query = query.Where(po => po.Supplier != null && po.Supplier.Code == filter.SupplierCode);

        if (filter.Status.HasValue)
            query = query.Where(po => po.Status == filter.Status.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(po => po.OrderDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(po => po.OrderDate <= filter.EndDate.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(po => po.Id)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(po => new PurchaseOrderDto
            {
                OrderNumber = po.OrderNumber,
                SupplierCode = po.Supplier != null ? po.Supplier.Code : "",
                SupplierName = po.Supplier != null ? po.Supplier.Name : "",
                OrderDate = po.OrderDate,
                ExpectedDeliveryDate = po.ExpectedDeliveryDate,
                Status = po.Status,
                SubTotal = po.SubTotal,
                DiscountAmount = po.DiscountAmount,
                TotalAmount = po.TotalAmount,
                ItemsCount = po.Items.Count
            })
            .ToListAsync(ct);

        return new PagedResult<PurchaseOrderDto>(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<List<PurchaseOrderDto>> GetAllFilteredPurchaseOrdersAsync(PurchaseOrderFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.PurchaseOrders
            .AsNoTracking()
            .Include(po => po.Supplier)
            .Include(po => po.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(po => po.OrderNumber.Contains(term) || (po.Supplier != null && po.Supplier.Name.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(filter.SupplierCode))
            query = query.Where(po => po.Supplier != null && po.Supplier.Code == filter.SupplierCode);

        if (filter.Status.HasValue)
            query = query.Where(po => po.Status == filter.Status.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(po => po.OrderDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(po => po.OrderDate <= filter.EndDate.Value);

        return await query
            .OrderByDescending(po => po.Id)
            .Select(po => new PurchaseOrderDto
            {
                OrderNumber = po.OrderNumber,
                SupplierCode = po.Supplier != null ? po.Supplier.Code : "",
                SupplierName = po.Supplier != null ? po.Supplier.Name : "",
                OrderDate = po.OrderDate,
                ExpectedDeliveryDate = po.ExpectedDeliveryDate,
                Status = po.Status,
                SubTotal = po.SubTotal,
                DiscountAmount = po.DiscountAmount,
                TotalAmount = po.TotalAmount,
                ItemsCount = po.Items.Count
            })
            .ToListAsync(ct);
    }

    public async Task<PurchaseOrderDetailsDto?> GetPurchaseOrderByNumberAsync(string orderNumber, CancellationToken ct = default)
    {
        var po = await _context.PurchaseOrders
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.Items)
            .ThenInclude(i => i.Product)
            .Include(p => p.GoodsReceipts)
            .ThenInclude(r => r.Warehouse)
            .FirstOrDefaultAsync(p => p.OrderNumber == orderNumber, ct);

        if (po == null) return null;

        return new PurchaseOrderDetailsDto
        {
            OrderNumber = po.OrderNumber,
            SupplierCode = po.Supplier?.Code ?? "",
            SupplierName = po.Supplier?.Name ?? "",
            OrderDate = po.OrderDate,
            ExpectedDeliveryDate = po.ExpectedDeliveryDate,
            Status = po.Status,
            SubTotal = po.SubTotal,
            DiscountAmount = po.DiscountAmount,
            TotalAmount = po.TotalAmount,
            Notes = po.Notes,
            ItemsCount = po.Items.Count,
            Items = po.Items.Select(i => new PurchaseOrderItemDto
            {
                ProductCode = i.Product?.Code ?? "",
                ProductName = i.Product?.Name ?? "",
                Unit = i.Product?.Unit ?? "",
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                LineTotal = i.LineTotal,
                ReceivedQuantity = i.ReceivedQuantity,
                RemainingQuantity = i.RemainingQuantity
            }).ToList(),
            Receipts = po.GoodsReceipts.Select(r => new GoodsReceiptSummaryDto
            {
                ReceiptNumber = r.ReceiptNumber,
                WarehouseName = r.Warehouse?.Name ?? "",
                ReceiptDate = r.ReceiptDate,
                TotalAmount = r.TotalAmount
            }).ToList()
        };
    }

    public async Task<Result<PurchaseOrderDto>> CreatePurchaseOrderAsync(CreatePurchaseOrderDto dto, CancellationToken ct = default)
    {
        var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Code == dto.SupplierCode, ct);
        if (supplier == null) return Result<PurchaseOrderDto>.Failure("المورد المحدد غير موجود");

        var count = await _context.PurchaseOrders.CountAsync(ct);
        var orderNumber = $"PO-{DateTime.UtcNow.Year}-{(count + 1):D4}";

        decimal subTotal = 0;
        var items = new List<PurchaseOrderItem>();

        foreach (var item in dto.Items)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Code == item.ProductCode, ct);
            if (product == null) return Result<PurchaseOrderDto>.Failure($"الصنف {item.ProductCode} غير موجود");

            var lineTotal = item.Quantity * item.UnitPrice;
            subTotal += lineTotal;

            items.Add(new PurchaseOrderItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                LineTotal = lineTotal,
                ReceivedQuantity = 0
            });
        }

        var totalAmount = subTotal - dto.DiscountAmount;

        var po = new PurchaseOrder
        {
            OrderNumber = orderNumber,
            SupplierId = supplier.Id,
            OrderDate = dto.OrderDate ?? DateTime.UtcNow,
            ExpectedDeliveryDate = dto.ExpectedDeliveryDate,
            Status = PurchaseOrderStatus.Draft,
            SubTotal = subTotal,
            DiscountAmount = dto.DiscountAmount,
            TotalAmount = totalAmount > 0 ? totalAmount : 0,
            Notes = dto.Notes?.Trim(),
            Items = items
        };

        await _context.PurchaseOrders.AddAsync(po, ct);
        await _context.SaveChangesAsync(ct);

        return Result<PurchaseOrderDto>.Success(new PurchaseOrderDto
        {
            OrderNumber = po.OrderNumber,
            SupplierCode = supplier.Code,
            SupplierName = supplier.Name,
            OrderDate = po.OrderDate,
            ExpectedDeliveryDate = po.ExpectedDeliveryDate,
            Status = po.Status,
            SubTotal = po.SubTotal,
            DiscountAmount = po.DiscountAmount,
            TotalAmount = po.TotalAmount,
            ItemsCount = po.Items.Count
        }, "تم إنشاء أمر الشراء بنجاح بحالة مسودة (لم يؤثر على المخزون)");
    }

    public async Task<Result> ConfirmPurchaseOrderAsync(string orderNumber, CancellationToken ct = default)
    {
        var po = await _context.PurchaseOrders.FirstOrDefaultAsync(p => p.OrderNumber == orderNumber, ct);
        if (po == null) return Result.Failure("أمر الشراء غير موجود");

        if (po.Status != PurchaseOrderStatus.Draft)
            return Result.Failure("يمكن تأكيد أوامر الشراء التي في حالة مسودة فقط");

        po.Status = PurchaseOrderStatus.Confirmed;
        await _context.SaveChangesAsync(ct);

        return Result.Success("تم تأكيد أمر الشراء وجاهز لاستلام البضاعة بالمخزن");
    }

    public async Task<Result> CancelPurchaseOrderAsync(string orderNumber, CancellationToken ct = default)
    {
        var po = await _context.PurchaseOrders.Include(p => p.GoodsReceipts).FirstOrDefaultAsync(p => p.OrderNumber == orderNumber, ct);
        if (po == null) return Result.Failure("أمر الشراء غير موجود");

        if (po.GoodsReceipts.Any())
            return Result.Failure("لا يمكن إلغاء أمر شراء تم استلام بضائع مرتبطة به بالفعل");

        po.Status = PurchaseOrderStatus.Cancelled;
        await _context.SaveChangesAsync(ct);

        return Result.Success("تم إلغاء أمر الشراء بنجاح");
    }
}
