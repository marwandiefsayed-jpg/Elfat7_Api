using Elfat7.Application.Common;
using Elfat7.Application.DTOs.Inventory;
using Elfat7.Application.Interfaces;
using Elfat7.Domain.Entities;
using Elfat7.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Elfat7.Infrastructure.Services;

public class InventoryService : IInventoryService
{
    private readonly IApplicationDbContext _context;

    public InventoryService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<InventoryItemDto>> GetInventoryAsync(InventoryFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.Products.AsNoTracking().Include(p => p.Category).Include(p => p.WarehouseStocks).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(p => p.Name.Contains(term) || p.Code.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(filter.CategoryCode))
            query = query.Where(p => p.Category != null && p.Category.Code == filter.CategoryCode);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(p => p.Id)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(p => new InventoryItemDto
            {
                ProductCode = p.Code,
                ProductName = p.Name,
                CategoryName = p.Category != null ? p.Category.Name : "",
                Unit = p.Unit,
                TotalQuantity = p.WarehouseStocks.Sum(ws => ws.Quantity),
                TotalReservedQuantity = p.WarehouseStocks.Sum(ws => ws.ReservedQuantity),
                AverageCost = p.AverageCost
            })
            .ToListAsync(ct);

        return new PagedResult<InventoryItemDto>(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<List<InventoryItemDto>> GetAllFilteredInventoryAsync(InventoryFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.Products.AsNoTracking().Include(p => p.Category).Include(p => p.WarehouseStocks).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(p => p.Name.Contains(term) || p.Code.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(filter.CategoryCode))
            query = query.Where(p => p.Category != null && p.Category.Code == filter.CategoryCode);

        return await query
            .OrderBy(p => p.Id)
            .Select(p => new InventoryItemDto
            {
                ProductCode = p.Code,
                ProductName = p.Name,
                CategoryName = p.Category != null ? p.Category.Name : "",
                Unit = p.Unit,
                TotalQuantity = p.WarehouseStocks.Sum(ws => ws.Quantity),
                TotalReservedQuantity = p.WarehouseStocks.Sum(ws => ws.ReservedQuantity),
                AverageCost = p.AverageCost
            })
            .ToListAsync(ct);
    }

    public async Task<PagedResult<StockMovementDto>> GetStockMovementsAsync(StockMovementFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.StockMovements
            .AsNoTracking()
            .Include(m => m.Warehouse)
            .Include(m => m.Product)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(m => m.MovementNumber.Contains(term) || m.ReferenceNumber.Contains(term) ||
                                     (m.Product != null && m.Product.Name.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(filter.WarehouseCode))
            query = query.Where(m => m.Warehouse != null && m.Warehouse.Code == filter.WarehouseCode);

        if (!string.IsNullOrWhiteSpace(filter.ProductCode))
            query = query.Where(m => m.Product != null && m.Product.Code == filter.ProductCode);

        if (filter.MovementType.HasValue)
            query = query.Where(m => m.MovementType == filter.MovementType.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(m => m.CreatedAt >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(m => m.CreatedAt <= filter.EndDate.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(m => m.Id)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(m => new StockMovementDto
            {
                MovementNumber = m.MovementNumber,
                WarehouseCode = m.Warehouse != null ? m.Warehouse.Code : "",
                WarehouseName = m.Warehouse != null ? m.Warehouse.Name : "",
                ProductCode = m.Product != null ? m.Product.Code : "",
                ProductName = m.Product != null ? m.Product.Name : "",
                MovementType = m.MovementType,
                Quantity = m.Quantity,
                Unit = m.Product != null ? m.Product.Unit : "",
                UnitCost = m.UnitCost,
                TotalCost = m.TotalCost,
                ReferenceNumber = m.ReferenceNumber,
                CreatedAt = m.CreatedAt,
                Notes = m.Notes
            })
            .ToListAsync(ct);

        return new PagedResult<StockMovementDto>(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<List<StockMovementDto>> GetAllFilteredStockMovementsAsync(StockMovementFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.StockMovements
            .AsNoTracking()
            .Include(m => m.Warehouse)
            .Include(m => m.Product)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(m => m.MovementNumber.Contains(term) || m.ReferenceNumber.Contains(term) ||
                                     (m.Product != null && m.Product.Name.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(filter.WarehouseCode))
            query = query.Where(m => m.Warehouse != null && m.Warehouse.Code == filter.WarehouseCode);

        if (!string.IsNullOrWhiteSpace(filter.ProductCode))
            query = query.Where(m => m.Product != null && m.Product.Code == filter.ProductCode);

        if (filter.MovementType.HasValue)
            query = query.Where(m => m.MovementType == filter.MovementType.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(m => m.CreatedAt >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(m => m.CreatedAt <= filter.EndDate.Value);

        return await query
            .OrderByDescending(m => m.Id)
            .Select(m => new StockMovementDto
            {
                MovementNumber = m.MovementNumber,
                WarehouseCode = m.Warehouse != null ? m.Warehouse.Code : "",
                WarehouseName = m.Warehouse != null ? m.Warehouse.Name : "",
                ProductCode = m.Product != null ? m.Product.Code : "",
                ProductName = m.Product != null ? m.Product.Name : "",
                MovementType = m.MovementType,
                Quantity = m.Quantity,
                Unit = m.Product != null ? m.Product.Unit : "",
                UnitCost = m.UnitCost,
                TotalCost = m.TotalCost,
                ReferenceNumber = m.ReferenceNumber,
                CreatedAt = m.CreatedAt,
                Notes = m.Notes
            })
            .ToListAsync(ct);
    }

    public async Task<PagedResult<StockTransferDto>> GetStockTransfersAsync(PagedFilterRequest filter, CancellationToken ct = default)
    {
        var query = _context.StockTransfers
            .AsNoTracking()
            .Include(t => t.SourceWarehouse)
            .Include(t => t.DestinationWarehouse)
            .Include(t => t.Items)
            .ThenInclude(i => i.Product)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(t => t.TransferNumber.Contains(term) ||
                                     (t.SourceWarehouse != null && t.SourceWarehouse.Name.Contains(term)) ||
                                     (t.DestinationWarehouse != null && t.DestinationWarehouse.Name.Contains(term)));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(t => t.Id)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(t => new StockTransferDto
            {
                TransferNumber = t.TransferNumber,
                SourceWarehouseCode = t.SourceWarehouse != null ? t.SourceWarehouse.Code : "",
                SourceWarehouseName = t.SourceWarehouse != null ? t.SourceWarehouse.Name : "",
                DestinationWarehouseCode = t.DestinationWarehouse != null ? t.DestinationWarehouse.Code : "",
                DestinationWarehouseName = t.DestinationWarehouse != null ? t.DestinationWarehouse.Name : "",
                TransferDate = t.TransferDate,
                Status = t.Status,
                Notes = t.Notes,
                Items = t.Items.Select(i => new StockTransferItemDto
                {
                    ProductCode = i.Product != null ? i.Product.Code : "",
                    ProductName = i.Product != null ? i.Product.Name : "",
                    Unit = i.Product != null ? i.Product.Unit : "",
                    Quantity = i.Quantity,
                    UnitCost = i.UnitCost
                }).ToList()
            })
            .ToListAsync(ct);

        return new PagedResult<StockTransferDto>(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<List<StockTransferDto>> GetAllFilteredStockTransfersAsync(PagedFilterRequest filter, CancellationToken ct = default)
    {
        var query = _context.StockTransfers
            .AsNoTracking()
            .Include(t => t.SourceWarehouse)
            .Include(t => t.DestinationWarehouse)
            .Include(t => t.Items)
            .ThenInclude(i => i.Product)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(t => t.TransferNumber.Contains(term) ||
                                     (t.SourceWarehouse != null && t.SourceWarehouse.Name.Contains(term)) ||
                                     (t.DestinationWarehouse != null && t.DestinationWarehouse.Name.Contains(term)));
        }

        return await query
            .OrderByDescending(t => t.Id)
            .Select(t => new StockTransferDto
            {
                TransferNumber = t.TransferNumber,
                SourceWarehouseCode = t.SourceWarehouse != null ? t.SourceWarehouse.Code : "",
                SourceWarehouseName = t.SourceWarehouse != null ? t.SourceWarehouse.Name : "",
                DestinationWarehouseCode = t.DestinationWarehouse != null ? t.DestinationWarehouse.Code : "",
                DestinationWarehouseName = t.DestinationWarehouse != null ? t.DestinationWarehouse.Name : "",
                TransferDate = t.TransferDate,
                Status = t.Status,
                Notes = t.Notes,
                Items = t.Items.Select(i => new StockTransferItemDto
                {
                    ProductCode = i.Product != null ? i.Product.Code : "",
                    ProductName = i.Product != null ? i.Product.Name : "",
                    Unit = i.Product != null ? i.Product.Unit : "",
                    Quantity = i.Quantity,
                    UnitCost = i.UnitCost
                }).ToList()
            })
            .ToListAsync(ct);
    }

    public async Task<Result<StockTransferDto>> CreateStockTransferAsync(CreateStockTransferDto dto, CancellationToken ct = default)
    {
        using var transaction = await _context.BeginTransactionAsync(ct);

        var srcWh = await _context.Warehouses.FirstOrDefaultAsync(w => w.Code == dto.SourceWarehouseCode, ct);
        if (srcWh == null) return Result<StockTransferDto>.Failure("مخزن المصدر غير موجود");

        var dstWh = await _context.Warehouses.FirstOrDefaultAsync(w => w.Code == dto.DestinationWarehouseCode, ct);
        if (dstWh == null) return Result<StockTransferDto>.Failure("مخزن الوجهة غير موجود");

        var transferCount = await _context.StockTransfers.CountAsync(ct);
        var transferNumber = $"TR-{DateTime.UtcNow.Year}-{(transferCount + 1):D4}";

        var transfer = new StockTransfer
        {
            TransferNumber = transferNumber,
            SourceWarehouseId = srcWh.Id,
            DestinationWarehouseId = dstWh.Id,
            TransferDate = dto.TransferDate ?? DateTime.UtcNow,
            Status = TransferStatus.Completed,
            Notes = dto.Notes?.Trim()
        };

        var transferItems = new List<StockTransferItem>();
        var movements = new List<StockMovement>();

        foreach (var item in dto.Items)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Code == item.ProductCode, ct);
            if (product == null) return Result<StockTransferDto>.Failure($"الصنف {item.ProductCode} غير موجود");

            var srcStock = await _context.WarehouseStocks
                .FirstOrDefaultAsync(ws => ws.WarehouseId == srcWh.Id && ws.ProductId == product.Id, ct);

            if (srcStock == null || srcStock.AvailableQuantity < item.Quantity)
            {
                return Result<StockTransferDto>.Failure($"الرصيد المتاح من صنف {product.Name} في مخزن {srcWh.Name} غير كافٍ لإتمام التحويل. الرصيد المتاح: {(srcStock?.AvailableQuantity ?? 0)} {product.Unit}");
            }

            var dstStock = await _context.WarehouseStocks
                .FirstOrDefaultAsync(ws => ws.WarehouseId == dstWh.Id && ws.ProductId == product.Id, ct);

            if (dstStock == null)
            {
                dstStock = new WarehouseStock
                {
                    WarehouseId = dstWh.Id,
                    ProductId = product.Id,
                    Quantity = 0,
                    ReservedQuantity = 0,
                    LastUpdated = DateTime.UtcNow
                };
                await _context.WarehouseStocks.AddAsync(dstStock, ct);
            }

            srcStock.Quantity -= item.Quantity;
            srcStock.LastUpdated = DateTime.UtcNow;

            dstStock.Quantity += item.Quantity;
            dstStock.LastUpdated = DateTime.UtcNow;

            transferItems.Add(new StockTransferItem
            {
                StockTransfer = transfer,
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitCost = product.AverageCost
            });

            var movCount = await _context.StockMovements.CountAsync(ct);

            // الحركة الصادرة من مخزن المصدر
            movements.Add(new StockMovement
            {
                MovementNumber = $"MOV-{DateTime.UtcNow.Year}-{(movCount + movements.Count + 1):D5}",
                WarehouseId = srcWh.Id,
                ProductId = product.Id,
                MovementType = StockMovementType.TransferOut,
                Quantity = -item.Quantity,
                UnitCost = product.AverageCost,
                TotalCost = item.Quantity * product.AverageCost,
                ReferenceNumber = transferNumber,
                Notes = $"تحويل صادر إلى {dstWh.Name}"
            });

            // الحركة الواردة إلى مخزن الوجهة
            movements.Add(new StockMovement
            {
                MovementNumber = $"MOV-{DateTime.UtcNow.Year}-{(movCount + movements.Count + 2):D5}",
                WarehouseId = dstWh.Id,
                ProductId = product.Id,
                MovementType = StockMovementType.TransferIn,
                Quantity = item.Quantity,
                UnitCost = product.AverageCost,
                TotalCost = item.Quantity * product.AverageCost,
                ReferenceNumber = transferNumber,
                Notes = $"تحويل وارد من {srcWh.Name}"
            });
        }

        transfer.Items = transferItems;
        await _context.StockTransfers.AddAsync(transfer, ct);
        await _context.StockMovements.AddRangeAsync(movements, ct);

        await _context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return Result<StockTransferDto>.Success(new StockTransferDto
        {
            TransferNumber = transfer.TransferNumber,
            SourceWarehouseCode = srcWh.Code,
            SourceWarehouseName = srcWh.Name,
            DestinationWarehouseCode = dstWh.Code,
            DestinationWarehouseName = dstWh.Name,
            TransferDate = transfer.TransferDate,
            Status = transfer.Status,
            Notes = transfer.Notes
        }, "تم إتمام التحويل المخزني بنجاح وتحديث الأرصدة");
    }

    public async Task<PagedResult<StockAdjustmentDto>> GetStockAdjustmentsAsync(PagedFilterRequest filter, CancellationToken ct = default)
    {
        var query = _context.StockAdjustments
            .AsNoTracking()
            .Include(a => a.Warehouse)
            .Include(a => a.Items)
            .ThenInclude(i => i.Product)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(a => a.AdjustmentNumber.Contains(term) || a.Reason.Contains(term) ||
                                     (a.Warehouse != null && a.Warehouse.Name.Contains(term)));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(a => a.Id)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(a => new StockAdjustmentDto
            {
                AdjustmentNumber = a.AdjustmentNumber,
                WarehouseCode = a.Warehouse != null ? a.Warehouse.Code : "",
                WarehouseName = a.Warehouse != null ? a.Warehouse.Name : "",
                AdjustmentDate = a.AdjustmentDate,
                Type = a.Type,
                Reason = a.Reason,
                Notes = a.Notes,
                Items = a.Items.Select(i => new StockAdjustmentItemDto
                {
                    ProductCode = i.Product != null ? i.Product.Code : "",
                    ProductName = i.Product != null ? i.Product.Name : "",
                    Unit = i.Product != null ? i.Product.Unit : "",
                    Quantity = i.Quantity,
                    UnitCost = i.UnitCost,
                    Reason = i.Reason
                }).ToList()
            })
            .ToListAsync(ct);

        return new PagedResult<StockAdjustmentDto>(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<Result<StockAdjustmentDto>> CreateStockAdjustmentAsync(CreateStockAdjustmentDto dto, CancellationToken ct = default)
    {
        using var transaction = await _context.BeginTransactionAsync(ct);

        var warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.Code == dto.WarehouseCode, ct);
        if (warehouse == null) return Result<StockAdjustmentDto>.Failure("المخزن المحدد غير موجود");

        var adjCount = await _context.StockAdjustments.CountAsync(ct);
        var adjNumber = $"ADJ-{DateTime.UtcNow.Year}-{(adjCount + 1):D4}";

        var adjustment = new StockAdjustment
        {
            AdjustmentNumber = adjNumber,
            WarehouseId = warehouse.Id,
            AdjustmentDate = dto.AdjustmentDate ?? DateTime.UtcNow,
            Type = dto.Type,
            Reason = dto.Reason.Trim(),
            Notes = dto.Notes?.Trim()
        };

        var adjItems = new List<StockAdjustmentItem>();
        var movements = new List<StockMovement>();

        foreach (var item in dto.Items)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Code == item.ProductCode, ct);
            if (product == null) return Result<StockAdjustmentDto>.Failure($"الصنف {item.ProductCode} غير موجود");

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

            if (dto.Type == AdjustmentType.Decrease)
            {
                if (stock.AvailableQuantity < item.Quantity)
                {
                    return Result<StockAdjustmentDto>.Failure($"لا يمكن خصم كمية {item.Quantity} {product.Unit} من صنف {product.Name}. الرصيد المتاح {stock.AvailableQuantity} فقط");
                }
                stock.Quantity -= item.Quantity;
            }
            else
            {
                stock.Quantity += item.Quantity;
            }

            stock.LastUpdated = DateTime.UtcNow;

            adjItems.Add(new StockAdjustmentItem
            {
                StockAdjustment = adjustment,
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitCost = product.AverageCost,
                Reason = item.Reason
            });

            var movCount = await _context.StockMovements.CountAsync(ct);

            movements.Add(new StockMovement
            {
                MovementNumber = $"MOV-{DateTime.UtcNow.Year}-{(movCount + movements.Count + 1):D5}",
                WarehouseId = warehouse.Id,
                ProductId = product.Id,
                MovementType = dto.Type == AdjustmentType.Increase ? StockMovementType.AdjustmentIncrease : StockMovementType.AdjustmentDecrease,
                Quantity = dto.Type == AdjustmentType.Increase ? item.Quantity : -item.Quantity,
                UnitCost = product.AverageCost,
                TotalCost = item.Quantity * product.AverageCost,
                ReferenceNumber = adjNumber,
                Notes = $"تسوية جردية: {dto.Reason}"
            });
        }

        adjustment.Items = adjItems;
        await _context.StockAdjustments.AddAsync(adjustment, ct);
        await _context.StockMovements.AddRangeAsync(movements, ct);

        await _context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return Result<StockAdjustmentDto>.Success(new StockAdjustmentDto
        {
            AdjustmentNumber = adjustment.AdjustmentNumber,
            WarehouseCode = warehouse.Code,
            WarehouseName = warehouse.Name,
            AdjustmentDate = adjustment.AdjustmentDate,
            Type = adjustment.Type,
            Reason = adjustment.Reason,
            Notes = adjustment.Notes
        }, "تم تسجيل التسوية الجردية وتحديث أرصدة المخزون بنجاح");
    }
}
