using Elfat7.Application.Common;
using Elfat7.Application.DTOs.GoodsReceiving;
using Elfat7.Application.Interfaces;
using Elfat7.Domain.Entities;
using Elfat7.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Elfat7.Infrastructure.Services;

public class GoodsReceiptService : IGoodsReceiptService
{
    private readonly IApplicationDbContext _context;

    public GoodsReceiptService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<GoodsReceiptDto>> GetGoodsReceiptsAsync(GoodsReceiptFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.GoodsReceipts
            .AsNoTracking()
            .Include(r => r.PurchaseOrder)
            .Include(r => r.Supplier)
            .Include(r => r.Warehouse)
            .Include(r => r.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(r => r.ReceiptNumber.Contains(term) ||
                                     (r.PurchaseOrder != null && r.PurchaseOrder.OrderNumber.Contains(term)) ||
                                     (r.Supplier != null && r.Supplier.Name.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(filter.SupplierCode))
            query = query.Where(r => r.Supplier != null && r.Supplier.Code == filter.SupplierCode);

        if (!string.IsNullOrWhiteSpace(filter.WarehouseCode))
            query = query.Where(r => r.Warehouse != null && r.Warehouse.Code == filter.WarehouseCode);

        if (!string.IsNullOrWhiteSpace(filter.PurchaseOrderNumber))
            query = query.Where(r => r.PurchaseOrder != null && r.PurchaseOrder.OrderNumber == filter.PurchaseOrderNumber);

        if (filter.StartDate.HasValue)
            query = query.Where(r => r.ReceiptDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(r => r.ReceiptDate <= filter.EndDate.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(r => r.Id)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(r => new GoodsReceiptDto
            {
                ReceiptNumber = r.ReceiptNumber,
                PurchaseOrderNumber = r.PurchaseOrder != null ? r.PurchaseOrder.OrderNumber : "",
                SupplierCode = r.Supplier != null ? r.Supplier.Code : "",
                SupplierName = r.Supplier != null ? r.Supplier.Name : "",
                WarehouseCode = r.Warehouse != null ? r.Warehouse.Code : "",
                WarehouseName = r.Warehouse != null ? r.Warehouse.Name : "",
                ReceiptDate = r.ReceiptDate,
                TotalAmount = r.TotalAmount,
                ItemsCount = r.Items.Count
            })
            .ToListAsync(ct);

        return new PagedResult<GoodsReceiptDto>(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<List<GoodsReceiptDto>> GetAllFilteredGoodsReceiptsAsync(GoodsReceiptFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.GoodsReceipts
            .AsNoTracking()
            .Include(r => r.PurchaseOrder)
            .Include(r => r.Supplier)
            .Include(r => r.Warehouse)
            .Include(r => r.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(r => r.ReceiptNumber.Contains(term) ||
                                     (r.PurchaseOrder != null && r.PurchaseOrder.OrderNumber.Contains(term)) ||
                                     (r.Supplier != null && r.Supplier.Name.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(filter.SupplierCode))
            query = query.Where(r => r.Supplier != null && r.Supplier.Code == filter.SupplierCode);

        if (!string.IsNullOrWhiteSpace(filter.WarehouseCode))
            query = query.Where(r => r.Warehouse != null && r.Warehouse.Code == filter.WarehouseCode);

        if (!string.IsNullOrWhiteSpace(filter.PurchaseOrderNumber))
            query = query.Where(r => r.PurchaseOrder != null && r.PurchaseOrder.OrderNumber == filter.PurchaseOrderNumber);

        if (filter.StartDate.HasValue)
            query = query.Where(r => r.ReceiptDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(r => r.ReceiptDate <= filter.EndDate.Value);

        return await query
            .OrderByDescending(r => r.Id)
            .Select(r => new GoodsReceiptDto
            {
                ReceiptNumber = r.ReceiptNumber,
                PurchaseOrderNumber = r.PurchaseOrder != null ? r.PurchaseOrder.OrderNumber : "",
                SupplierCode = r.Supplier != null ? r.Supplier.Code : "",
                SupplierName = r.Supplier != null ? r.Supplier.Name : "",
                WarehouseCode = r.Warehouse != null ? r.Warehouse.Code : "",
                WarehouseName = r.Warehouse != null ? r.Warehouse.Name : "",
                ReceiptDate = r.ReceiptDate,
                TotalAmount = r.TotalAmount,
                ItemsCount = r.Items.Count
            })
            .ToListAsync(ct);
    }

    public async Task<GoodsReceiptDetailsDto?> GetGoodsReceiptByNumberAsync(string receiptNumber, CancellationToken ct = default)
    {
        var r = await _context.GoodsReceipts
            .AsNoTracking()
            .Include(g => g.PurchaseOrder)
            .Include(g => g.Supplier)
            .Include(g => g.Warehouse)
            .Include(g => g.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(g => g.ReceiptNumber == receiptNumber, ct);

        if (r == null) return null;

        return new GoodsReceiptDetailsDto
        {
            ReceiptNumber = r.ReceiptNumber,
            PurchaseOrderNumber = r.PurchaseOrder?.OrderNumber ?? "",
            SupplierCode = r.Supplier?.Code ?? "",
            SupplierName = r.Supplier?.Name ?? "",
            WarehouseCode = r.Warehouse?.Code ?? "",
            WarehouseName = r.Warehouse?.Name ?? "",
            ReceiptDate = r.ReceiptDate,
            TotalAmount = r.TotalAmount,
            Notes = r.Notes,
            ItemsCount = r.Items.Count,
            Items = r.Items.Select(i => new GoodsReceiptItemDto
            {
                ProductCode = i.Product?.Code ?? "",
                ProductName = i.Product?.Name ?? "",
                Unit = i.Product?.Unit ?? "",
                Quantity = i.Quantity,
                UnitCost = i.UnitCost,
                LineTotal = i.LineTotal
            }).ToList()
        };
    }

    public async Task<Result<GoodsReceiptDto>> CreateGoodsReceiptAsync(CreateGoodsReceiptDto dto, CancellationToken ct = default)
    {
        using var transaction = await _context.BeginTransactionAsync(ct);

        var po = await _context.PurchaseOrders
            .Include(p => p.Items)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.OrderNumber == dto.PurchaseOrderNumber, ct);

        if (po == null) return Result<GoodsReceiptDto>.Failure("أمر الشراء المحدد غير موجود");

        if (po.Status == PurchaseOrderStatus.Cancelled || po.Status == PurchaseOrderStatus.FullyReceived)
            return Result<GoodsReceiptDto>.Failure($"لا يمكن الاستلام لأمر شراء بحالة {po.Status.ToArabic()}");

        var warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.Code == dto.WarehouseCode, ct);
        if (warehouse == null) return Result<GoodsReceiptDto>.Failure("مخزن الاستلام غير موجود");

        var receiptCount = await _context.GoodsReceipts.CountAsync(ct);
        var receiptNumber = $"REC-{DateTime.UtcNow.Year}-{(receiptCount + 1):D4}";

        var receipt = new GoodsReceipt
        {
            ReceiptNumber = receiptNumber,
            PurchaseOrderId = po.Id,
            SupplierId = po.SupplierId,
            WarehouseId = warehouse.Id,
            ReceiptDate = dto.ReceiptDate ?? DateTime.UtcNow,
            Notes = dto.Notes?.Trim()
        };

        decimal totalReceiptAmount = 0;
        var receiptItems = new List<GoodsReceiptItem>();
        var stockMovements = new List<StockMovement>();

        foreach (var item in dto.Items)
        {
            var product = await _context.Products.Include(p => p.WarehouseStocks).FirstOrDefaultAsync(p => p.Code == item.ProductCode, ct);
            if (product == null) return Result<GoodsReceiptDto>.Failure($"الصنف {item.ProductCode} غير موجود");

            var poItem = po.Items.FirstOrDefault(i => i.ProductId == product.Id);
            if (poItem == null) return Result<GoodsReceiptDto>.Failure($"الصنف {product.Name} غير موجود في أمر الشراء المحدد");

            if (poItem.RemainingQuantity < item.Quantity)
            {
                return Result<GoodsReceiptDto>.Failure($"الكمية المستلمة ({item.Quantity}) تتجاوز الكمية المتبقية في أمر الشراء ({poItem.RemainingQuantity}) للصنف {product.Name}");
            }

            // تحديث الكمية المستلمة في بند أمر الشراء
            poItem.ReceivedQuantity += item.Quantity;

            var lineTotal = item.Quantity * item.UnitCost;
            totalReceiptAmount += lineTotal;

            receiptItems.Add(new GoodsReceiptItem
            {
                GoodsReceipt = receipt,
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitCost = item.UnitCost,
                LineTotal = lineTotal
            });

            // 1. تحديث أو إنشاء رصيد المخزن المحدد
            var stock = await _context.WarehouseStocks
                .FirstOrDefaultAsync(ws => ws.WarehouseId == warehouse.Id && ws.ProductId == product.Id, ct);

            if (stock == null)
            {
                stock = new WarehouseStock
                {
                    WarehouseId = warehouse.Id,
                    ProductId = product.Id,
                    Quantity = 0,
                    ReservedQuantity = 0,
                    LastUpdated = DateTime.UtcNow
                };
                await _context.WarehouseStocks.AddAsync(stock, ct);
            }

            // 2. تحديث متوسط التكلفة المرجح للصنف (Moving Weighted Average Cost)
            var currentTotalQuantity = product.WarehouseStocks.Sum(ws => ws.Quantity);
            if (currentTotalQuantity + item.Quantity > 0)
            {
                var newAvgCost = ((currentTotalQuantity * product.AverageCost) + (item.Quantity * item.UnitCost))
                                 / (currentTotalQuantity + item.Quantity);
                product.AverageCost = Math.Round(newAvgCost, 2);
            }

            // 3. زيادة كمية المخزن الفعلي
            stock.Quantity += item.Quantity;
            stock.LastUpdated = DateTime.UtcNow;

            // 4. تسجيل حركة المخزون
            var movCount = await _context.StockMovements.CountAsync(ct);
            stockMovements.Add(new StockMovement
            {
                MovementNumber = $"MOV-{DateTime.UtcNow.Year}-{(movCount + stockMovements.Count + 1):D5}",
                WarehouseId = warehouse.Id,
                ProductId = product.Id,
                MovementType = StockMovementType.PurchaseReceipt,
                Quantity = item.Quantity,
                UnitCost = item.UnitCost,
                TotalCost = lineTotal,
                ReferenceNumber = receiptNumber,
                Notes = $"استلام بضاعة بموجب أمر شراء {po.OrderNumber}"
            });
        }

        receipt.TotalAmount = totalReceiptAmount;
        receipt.Items = receiptItems;

        // 5. تحديث رصيد المورد (زيادة مستحقات المورد - دائن)
        if (po.Supplier != null)
        {
            po.Supplier.Balance += totalReceiptAmount;
        }

        // 6. تحديث حالة أمر الشراء
        bool allFullyReceived = po.Items.All(i => i.RemainingQuantity == 0);
        po.Status = allFullyReceived ? PurchaseOrderStatus.FullyReceived : PurchaseOrderStatus.PartiallyReceived;

        await _context.GoodsReceipts.AddAsync(receipt, ct);
        await _context.StockMovements.AddRangeAsync(stockMovements, ct);

        await _context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return Result<GoodsReceiptDto>.Success(new GoodsReceiptDto
        {
            ReceiptNumber = receipt.ReceiptNumber,
            PurchaseOrderNumber = po.OrderNumber,
            SupplierCode = po.Supplier?.Code ?? "",
            SupplierName = po.Supplier?.Name ?? "",
            WarehouseCode = warehouse.Code,
            WarehouseName = warehouse.Name,
            ReceiptDate = receipt.ReceiptDate,
            TotalAmount = receipt.TotalAmount,
            ItemsCount = receipt.Items.Count
        }, "تم استلام البضاعة في المخزن بنجاح وتحديث أرصدة المخزون وحساب المورد");
    }
}
