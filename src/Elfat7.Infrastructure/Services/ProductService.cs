using Elfat7.Application.Common;
using Elfat7.Application.DTOs.Products;
using Elfat7.Application.Interfaces;
using Elfat7.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Elfat7.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly IApplicationDbContext _context;

    public ProductService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ProductDto>> GetProductsAsync(ProductFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.Products.AsNoTracking().Include(p => p.Category).Include(p => p.WarehouseStocks).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(p => p.Name.Contains(term) || p.Code.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(filter.CategoryCode))
            query = query.Where(p => p.Category != null && p.Category.Code == filter.CategoryCode);

        if (filter.IsActive.HasValue)
            query = query.Where(p => p.IsActive == filter.IsActive.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.Id)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(p => new ProductDto
            {
                Code = p.Code,
                Name = p.Name,
                CategoryCode = p.Category != null ? p.Category.Code : "",
                CategoryName = p.Category != null ? p.Category.Name : "",
                Unit = p.Unit,
                DefaultPurchasePrice = p.DefaultPurchasePrice,
                DefaultSellingPrice = p.DefaultSellingPrice,
                AverageCost = p.AverageCost,
                TotalStockQuantity = p.WarehouseStocks.Sum(ws => ws.Quantity),
                Description = p.Description,
                IsActive = p.IsActive
            })
            .ToListAsync(ct);

        return new PagedResult<ProductDto>(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<List<ProductDto>> GetAllFilteredProductsAsync(ProductFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.Products.AsNoTracking().Include(p => p.Category).Include(p => p.WarehouseStocks).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(p => p.Name.Contains(term) || p.Code.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(filter.CategoryCode))
            query = query.Where(p => p.Category != null && p.Category.Code == filter.CategoryCode);

        if (filter.IsActive.HasValue)
            query = query.Where(p => p.IsActive == filter.IsActive.Value);

        return await query
            .OrderByDescending(p => p.Id)
            .Select(p => new ProductDto
            {
                Code = p.Code,
                Name = p.Name,
                CategoryCode = p.Category != null ? p.Category.Code : "",
                CategoryName = p.Category != null ? p.Category.Name : "",
                Unit = p.Unit,
                DefaultPurchasePrice = p.DefaultPurchasePrice,
                DefaultSellingPrice = p.DefaultSellingPrice,
                AverageCost = p.AverageCost,
                TotalStockQuantity = p.WarehouseStocks.Sum(ws => ws.Quantity),
                Description = p.Description,
                IsActive = p.IsActive
            })
            .ToListAsync(ct);
    }

    public async Task<ProductDetailsDto?> GetProductByCodeAsync(string code, CancellationToken ct = default)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.WarehouseStocks)
            .ThenInclude(ws => ws.Warehouse)
            .FirstOrDefaultAsync(p => p.Code == code, ct);

        if (product == null) return null;

        return new ProductDetailsDto
        {
            Code = product.Code,
            Name = product.Name,
            CategoryCode = product.Category?.Code ?? "",
            CategoryName = product.Category?.Name ?? "",
            Unit = product.Unit,
            DefaultPurchasePrice = product.DefaultPurchasePrice,
            DefaultSellingPrice = product.DefaultSellingPrice,
            AverageCost = product.AverageCost,
            TotalStockQuantity = product.WarehouseStocks.Sum(ws => ws.Quantity),
            Description = product.Description,
            IsActive = product.IsActive,
            WarehouseStocks = product.WarehouseStocks.Select(ws => new ProductWarehouseStockDto
            {
                WarehouseCode = ws.Warehouse?.Code ?? "",
                WarehouseName = ws.Warehouse?.Name ?? "",
                Quantity = ws.Quantity,
                ReservedQuantity = ws.ReservedQuantity,
                AvailableQuantity = ws.AvailableQuantity
            }).ToList()
        };
    }

    public async Task<Result<ProductDto>> CreateProductAsync(CreateProductDto dto, CancellationToken ct = default)
    {
        var category = await _context.ProductCategories.FirstOrDefaultAsync(c => c.Code == dto.CategoryCode, ct);
        if (category == null)
            return Result<ProductDto>.Failure("تصنيف الأصناف المحدد غير موجود");

        var count = await _context.Products.CountAsync(ct);
        var code = $"PRD-{(count + 1):D4}";

        var product = new Product
        {
            Code = code,
            Name = dto.Name.Trim(),
            CategoryId = category.Id,
            Unit = dto.Unit.Trim(),
            DefaultPurchasePrice = dto.DefaultPurchasePrice,
            DefaultSellingPrice = dto.DefaultSellingPrice,
            AverageCost = dto.DefaultPurchasePrice,
            Description = dto.Description?.Trim(),
            IsActive = true
        };

        await _context.Products.AddAsync(product, ct);
        await _context.SaveChangesAsync(ct);

        return Result<ProductDto>.Success(new ProductDto
        {
            Code = product.Code,
            Name = product.Name,
            CategoryCode = category.Code,
            CategoryName = category.Name,
            Unit = product.Unit,
            DefaultPurchasePrice = product.DefaultPurchasePrice,
            DefaultSellingPrice = product.DefaultSellingPrice,
            AverageCost = product.AverageCost,
            TotalStockQuantity = 0,
            Description = product.Description,
            IsActive = product.IsActive
        }, "تم إضافة الصنف بنجاح");
    }

    public async Task<Result<ProductDto>> UpdateProductAsync(string code, UpdateProductDto dto, CancellationToken ct = default)
    {
        var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Code == code, ct);
        if (product == null)
            return Result<ProductDto>.Failure("الصنف المطلوب غير موجود");

        var category = await _context.ProductCategories.FirstOrDefaultAsync(c => c.Code == dto.CategoryCode, ct);
        if (category == null)
            return Result<ProductDto>.Failure("تصنيف الأصناف المحدد غير موجود");

        product.Name = dto.Name.Trim();
        product.CategoryId = category.Id;
        product.Unit = dto.Unit.Trim();
        product.DefaultPurchasePrice = dto.DefaultPurchasePrice;
        product.DefaultSellingPrice = dto.DefaultSellingPrice;
        product.Description = dto.Description?.Trim();
        product.IsActive = dto.IsActive;

        await _context.SaveChangesAsync(ct);

        return Result<ProductDto>.Success(new ProductDto
        {
            Code = product.Code,
            Name = product.Name,
            CategoryCode = category.Code,
            CategoryName = category.Name,
            Unit = product.Unit,
            DefaultPurchasePrice = product.DefaultPurchasePrice,
            DefaultSellingPrice = product.DefaultSellingPrice,
            AverageCost = product.AverageCost,
            Description = product.Description,
            IsActive = product.IsActive
        }, "تم تحديث بيانات الصنف بنجاح");
    }

    public async Task<Result> DeleteProductAsync(string code, CancellationToken ct = default)
    {
        var product = await _context.Products
            .Include(p => p.WarehouseStocks)
            .FirstOrDefaultAsync(p => p.Code == code, ct);

        if (product == null)
            return Result.Failure("الصنف المطلوب غير موجود");

        if (product.WarehouseStocks.Any(ws => ws.Quantity > 0))
            return Result.Failure("لا يمكن حذف صنف له رصيد حالي في المخازن");

        product.IsActive = false;
        await _context.SaveChangesAsync(ct);
        return Result.Success("تم إلغاء تنشيط الصنف بنجاح");
    }
}
