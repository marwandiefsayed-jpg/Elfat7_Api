using Elfat7.Application.Common;
using Elfat7.Application.DTOs.Products;
using Elfat7.Application.Interfaces;
using Elfat7.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Elfat7.Infrastructure.Services;

public class ProductCategoryService : IProductCategoryService
{
    private readonly IApplicationDbContext _context;

    public ProductCategoryService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductCategoryDto>> GetCategoriesAsync(CancellationToken ct = default)
    {
        return await _context.ProductCategories
            .AsNoTracking()
            .Select(c => new ProductCategoryDto
            {
                Code = c.Code,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                ProductCount = c.Products.Count
            })
            .ToListAsync(ct);
    }

    public async Task<ProductCategoryDto?> GetCategoryByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _context.ProductCategories
            .AsNoTracking()
            .Where(c => c.Code == code)
            .Select(c => new ProductCategoryDto
            {
                Code = c.Code,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                ProductCount = c.Products.Count
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Result<ProductCategoryDto>> CreateCategoryAsync(CreateProductCategoryDto dto, CancellationToken ct = default)
    {
        var count = await _context.ProductCategories.CountAsync(ct);
        var code = $"CAT-{(count + 1):D2}";

        var category = new ProductCategory
        {
            Code = code,
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            IsActive = true
        };

        await _context.ProductCategories.AddAsync(category, ct);
        await _context.SaveChangesAsync(ct);

        return Result<ProductCategoryDto>.Success(new ProductCategoryDto
        {
            Code = category.Code,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            ProductCount = 0
        }, "تم إضافة تصنيف الأصناف بنجاح");
    }

    public async Task<Result<ProductCategoryDto>> UpdateCategoryAsync(string code, UpdateProductCategoryDto dto, CancellationToken ct = default)
    {
        var cat = await _context.ProductCategories.FirstOrDefaultAsync(c => c.Code == code, ct);
        if (cat == null)
            return Result<ProductCategoryDto>.Failure("تصنيف الأصناف غير موجود");

        cat.Name = dto.Name.Trim();
        cat.Description = dto.Description?.Trim();
        cat.IsActive = dto.IsActive;

        await _context.SaveChangesAsync(ct);

        return Result<ProductCategoryDto>.Success(new ProductCategoryDto
        {
            Code = cat.Code,
            Name = cat.Name,
            Description = cat.Description,
            IsActive = cat.IsActive,
            ProductCount = await _context.Products.CountAsync(p => p.CategoryId == cat.Id, ct)
        }, "تم تحديث التصنيف بنجاح");
    }
}
