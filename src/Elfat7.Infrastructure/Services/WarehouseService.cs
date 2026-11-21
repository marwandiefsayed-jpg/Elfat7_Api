using Elfat7.Application.Common;
using Elfat7.Application.DTOs.Warehouses;
using Elfat7.Application.Interfaces;
using Elfat7.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Elfat7.Infrastructure.Services;

public class WarehouseService : IWarehouseService
{
    private readonly IApplicationDbContext _context;

    public WarehouseService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<WarehouseDto>> GetWarehousesAsync(WarehouseFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.Warehouses.AsNoTracking().Include(w => w.Stocks).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(w => w.Name.Contains(term) || w.Code.Contains(term) || (w.KeeperName != null && w.KeeperName.Contains(term)));
        }

        if (filter.IsActive.HasValue)
            query = query.Where(w => w.IsActive == filter.IsActive.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(w => w.Id)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(w => new WarehouseDto
            {
                Code = w.Code,
                Name = w.Name,
                Address = w.Address,
                KeeperName = w.KeeperName,
                KeeperPhone = w.KeeperPhone,
                IsActive = w.IsActive,
                ProductsInStockCount = w.Stocks.Count(s => s.Quantity > 0)
            })
            .ToListAsync(ct);

        return new PagedResult<WarehouseDto>(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<List<WarehouseDto>> GetAllFilteredWarehousesAsync(WarehouseFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.Warehouses.AsNoTracking().Include(w => w.Stocks).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(w => w.Name.Contains(term) || w.Code.Contains(term) || (w.KeeperName != null && w.KeeperName.Contains(term)));
        }

        if (filter.IsActive.HasValue)
            query = query.Where(w => w.IsActive == filter.IsActive.Value);

        return await query
            .OrderBy(w => w.Id)
            .Select(w => new WarehouseDto
            {
                Code = w.Code,
                Name = w.Name,
                Address = w.Address,
                KeeperName = w.KeeperName,
                KeeperPhone = w.KeeperPhone,
                IsActive = w.IsActive,
                ProductsInStockCount = w.Stocks.Count(s => s.Quantity > 0)
            })
            .ToListAsync(ct);
    }

    public async Task<WarehouseDetailsDto?> GetWarehouseByCodeAsync(string code, CancellationToken ct = default)
    {
        var warehouse = await _context.Warehouses
            .AsNoTracking()
            .Include(w => w.Stocks)
            .ThenInclude(s => s.Product)
            .ThenInclude(p => p!.Category)
            .FirstOrDefaultAsync(w => w.Code == code, ct);

        if (warehouse == null) return null;

        return new WarehouseDetailsDto
        {
            Code = warehouse.Code,
            Name = warehouse.Name,
            Address = warehouse.Address,
            KeeperName = warehouse.KeeperName,
            KeeperPhone = warehouse.KeeperPhone,
            IsActive = warehouse.IsActive,
            ProductsInStockCount = warehouse.Stocks.Count(s => s.Quantity > 0),
            StockItems = warehouse.Stocks
                .Where(s => s.Quantity > 0 && s.Product != null)
                .Select(s => new WarehouseStockItemDto
                {
                    ProductCode = s.Product!.Code,
                    ProductName = s.Product.Name,
                    CategoryName = s.Product.Category?.Name ?? "",
                    Unit = s.Product.Unit,
                    Quantity = s.Quantity,
                    ReservedQuantity = s.ReservedQuantity,
                    AvailableQuantity = s.AvailableQuantity,
                    AverageCost = s.Product.AverageCost
                }).ToList()
        };
    }

    public async Task<Result<WarehouseDto>> CreateWarehouseAsync(CreateWarehouseDto dto, CancellationToken ct = default)
    {
        var count = await _context.Warehouses.CountAsync(ct);
        var code = $"WH-{(count + 1):D2}";

        var warehouse = new Warehouse
        {
            Code = code,
            Name = dto.Name.Trim(),
            Address = dto.Address?.Trim(),
            KeeperName = dto.KeeperName?.Trim(),
            KeeperPhone = dto.KeeperPhone?.Trim(),
            IsActive = true
        };

        await _context.Warehouses.AddAsync(warehouse, ct);
        await _context.SaveChangesAsync(ct);

        return Result<WarehouseDto>.Success(new WarehouseDto
        {
            Code = warehouse.Code,
            Name = warehouse.Name,
            Address = warehouse.Address,
            KeeperName = warehouse.KeeperName,
            KeeperPhone = warehouse.KeeperPhone,
            IsActive = warehouse.IsActive,
            ProductsInStockCount = 0
        }, "تم إضافة المخزن بنجاح");
    }

    public async Task<Result<WarehouseDto>> UpdateWarehouseAsync(string code, UpdateWarehouseDto dto, CancellationToken ct = default)
    {
        var warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.Code == code, ct);
        if (warehouse == null)
            return Result<WarehouseDto>.Failure("المخزن المطلوب غير موجود");

        warehouse.Name = dto.Name.Trim();
        warehouse.Address = dto.Address?.Trim();
        warehouse.KeeperName = dto.KeeperName?.Trim();
        warehouse.KeeperPhone = dto.KeeperPhone?.Trim();
        warehouse.IsActive = dto.IsActive;

        await _context.SaveChangesAsync(ct);

        return Result<WarehouseDto>.Success(new WarehouseDto
        {
            Code = warehouse.Code,
            Name = warehouse.Name,
            Address = warehouse.Address,
            KeeperName = warehouse.KeeperName,
            KeeperPhone = warehouse.KeeperPhone,
            IsActive = warehouse.IsActive
        }, "تم تحديث بيانات المخزن بنجاح");
    }
}
