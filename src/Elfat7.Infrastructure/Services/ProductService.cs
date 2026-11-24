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
        var query = _context.Products.AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.WarehouseStocks)
            .Include(p => p.SellingUnits)
            .AsQueryable();

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
                IsActive = p.IsActive,
                SellingUnits = p.SellingUnits.Select(u => new ProductSellingUnitDto
                {
                    Id = u.Id,
                    UnitName = u.UnitName,
                    ConversionFactor = u.ConversionFactor,
                    SellingPrice = u.SellingPrice,
                    PurchasePrice = u.PurchasePrice,
                    IsDefault = u.IsDefault
                }).ToList()
            })
            .ToListAsync(ct);

        return new PagedResult<ProductDto>(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<List<ProductDto>> GetAllFilteredProductsAsync(ProductFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.Products.AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.WarehouseStocks)
            .Include(p => p.SellingUnits)
            .AsQueryable();

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
                IsActive = p.IsActive,
                SellingUnits = p.SellingUnits.Select(u => new ProductSellingUnitDto
                {
                    Id = u.Id,
                    UnitName = u.UnitName,
                    ConversionFactor = u.ConversionFactor,
                    SellingPrice = u.SellingPrice,
                    PurchasePrice = u.PurchasePrice,
                    IsDefault = u.IsDefault
                }).ToList()
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
            .Include(p => p.SellingUnits)
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
            SellingUnits = product.SellingUnits.Select(u => new ProductSellingUnitDto
            {
                Id = u.Id,
                UnitName = u.UnitName,
                ConversionFactor = u.ConversionFactor,
                SellingPrice = u.SellingPrice,
                PurchasePrice = u.PurchasePrice,
                IsDefault = u.IsDefault
            }).ToList(),
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
        ProductCategory? category = null;
        if (!string.IsNullOrWhiteSpace(dto.CategoryCode))
        {
            category = await _context.ProductCategories.FirstOrDefaultAsync(c => c.Code == dto.CategoryCode, ct);
        }

        var count = await _context.Products.CountAsync(ct);
        var code = $"PRD-{(count + 1):D4}";

        var defaultPurchasePrice = dto.DefaultPurchasePrice 
            ?? (dto.SellingUnits?.FirstOrDefault(u => u.PurchasePrice > 0)?.PurchasePrice ?? 0);
        var defaultSellingPrice = dto.DefaultSellingPrice 
            ?? (dto.SellingUnits?.FirstOrDefault(u => u.SellingPrice > 0)?.SellingPrice ?? 0);

        var product = new Product
        {
            Code = code,
            Name = dto.Name.Trim(),
            CategoryId = category?.Id,
            Unit = dto.Unit.Trim(),
            DefaultPurchasePrice = defaultPurchasePrice,
            DefaultSellingPrice = defaultSellingPrice,
            AverageCost = defaultPurchasePrice,
            Description = dto.Description?.Trim(),
            IsActive = true
        };

        if (dto.SellingUnits != null && dto.SellingUnits.Any())
        {
            foreach (var u in dto.SellingUnits)
            {
                if (!string.IsNullOrWhiteSpace(u.UnitName))
                {
                    product.SellingUnits.Add(new ProductSellingUnit
                    {
                        UnitName = u.UnitName.Trim(),
                        ConversionFactor = u.ConversionFactor > 0 ? u.ConversionFactor : 1,
                        SellingPrice = u.SellingPrice,
                        PurchasePrice = u.PurchasePrice,
                        IsDefault = u.IsDefault
                    });
                }
            }
        }

        // Always ensure at least the base unit is present as a selling unit
        if (!product.SellingUnits.Any(u => u.UnitName == product.Unit))
        {
            product.SellingUnits.Add(new ProductSellingUnit
            {
                UnitName = product.Unit,
                ConversionFactor = 1,
                SellingPrice = product.DefaultSellingPrice,
                PurchasePrice = product.DefaultPurchasePrice,
                IsDefault = !product.SellingUnits.Any(u => u.IsDefault)
            });
        }

        await _context.Products.AddAsync(product, ct);
        await _context.SaveChangesAsync(ct);

        return Result<ProductDto>.Success(new ProductDto
        {
            Code = product.Code,
            Name = product.Name,
            CategoryCode = category?.Code ?? "",
            CategoryName = category?.Name ?? "",
            Unit = product.Unit,
            DefaultPurchasePrice = product.DefaultPurchasePrice,
            DefaultSellingPrice = product.DefaultSellingPrice,
            AverageCost = product.AverageCost,
            TotalStockQuantity = 0,
            Description = product.Description,
            IsActive = product.IsActive,
            SellingUnits = product.SellingUnits.Select(u => new ProductSellingUnitDto
            {
                Id = u.Id,
                UnitName = u.UnitName,
                ConversionFactor = u.ConversionFactor,
                SellingPrice = u.SellingPrice,
                PurchasePrice = u.PurchasePrice,
                IsDefault = u.IsDefault
            }).ToList()
        }, "تم إضافة الصنف وتكويد وحدات البيع بنجاح");
    }

    public async Task<Result<ProductDto>> UpdateProductAsync(string code, UpdateProductDto dto, CancellationToken ct = default)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.SellingUnits)
            .FirstOrDefaultAsync(p => p.Code == code, ct);

        if (product == null)
            return Result<ProductDto>.Failure("الصنف المطلوب غير موجود");

        ProductCategory? category = null;
        if (!string.IsNullOrWhiteSpace(dto.CategoryCode))
        {
            category = await _context.ProductCategories.FirstOrDefaultAsync(c => c.Code == dto.CategoryCode, ct);
        }

        product.Name = dto.Name.Trim();
        product.CategoryId = category?.Id ?? product.CategoryId;
        product.Unit = dto.Unit.Trim();
        if (dto.DefaultPurchasePrice.HasValue)
            product.DefaultPurchasePrice = dto.DefaultPurchasePrice.Value;
        else if (dto.SellingUnits != null && dto.SellingUnits.Any(u => u.PurchasePrice > 0))
            product.DefaultPurchasePrice = dto.SellingUnits.First(u => u.PurchasePrice > 0).PurchasePrice;

        if (dto.DefaultSellingPrice.HasValue)
            product.DefaultSellingPrice = dto.DefaultSellingPrice.Value;
        else if (dto.SellingUnits != null && dto.SellingUnits.Any(u => u.SellingPrice > 0))
            product.DefaultSellingPrice = dto.SellingUnits.First(u => u.SellingPrice > 0).SellingPrice;

        product.Description = dto.Description?.Trim();
        product.IsActive = dto.IsActive;

        if (dto.SellingUnits != null)
        {
            var incomingIds = dto.SellingUnits
                .Where(u => u.Id.HasValue && u.Id.Value > 0)
                .Select(u => u.Id!.Value)
                .ToHashSet();

            var incomingNames = dto.SellingUnits
                .Where(u => !string.IsNullOrWhiteSpace(u.UnitName))
                .Select(u => u.UnitName.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Units to remove: any existing unit not present in the incoming submission (neither by ID nor by name)
            var unitsToRemove = product.SellingUnits
                .Where(u => !incomingIds.Contains(u.Id) && !incomingNames.Contains(u.UnitName.Trim()))
                .ToList();

            foreach (var u in unitsToRemove)
            {
                _context.ProductSellingUnits.Remove(u);
                product.SellingUnits.Remove(u);
            }

            // Units to update or add
            foreach (var uDto in dto.SellingUnits.Where(u => !string.IsNullOrWhiteSpace(u.UnitName)))
            {
                ProductSellingUnit? existing = null;
                if (uDto.Id.HasValue && uDto.Id.Value > 0)
                {
                    existing = product.SellingUnits.FirstOrDefault(u => u.Id == uDto.Id.Value);
                }
                existing ??= product.SellingUnits.FirstOrDefault(u => u.UnitName.Equals(uDto.UnitName.Trim(), StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
                    existing.UnitName = uDto.UnitName.Trim();
                    existing.ConversionFactor = uDto.ConversionFactor > 0 ? uDto.ConversionFactor : 1;
                    existing.SellingPrice = uDto.SellingPrice;
                    existing.PurchasePrice = uDto.PurchasePrice;
                    existing.IsDefault = uDto.IsDefault;
                }
                else
                {
                    var newUnit = new ProductSellingUnit
                    {
                        ProductId = product.Id,
                        UnitName = uDto.UnitName.Trim(),
                        ConversionFactor = uDto.ConversionFactor > 0 ? uDto.ConversionFactor : 1,
                        SellingPrice = uDto.SellingPrice,
                        PurchasePrice = uDto.PurchasePrice,
                        IsDefault = uDto.IsDefault
                    };
                    product.SellingUnits.Add(newUnit);
                }
            }

            // If no unit is marked as default, ensure the first unit is default
            if (product.SellingUnits.Any() && !product.SellingUnits.Any(u => u.IsDefault))
            {
                product.SellingUnits.First().IsDefault = true;
            }
        }

        await _context.SaveChangesAsync(ct);

        return Result<ProductDto>.Success(new ProductDto
        {
            Code = product.Code,
            Name = product.Name,
            CategoryCode = category?.Code ?? product.Category?.Code ?? "",
            CategoryName = category?.Name ?? product.Category?.Name ?? "",
            Unit = product.Unit,
            DefaultPurchasePrice = product.DefaultPurchasePrice,
            DefaultSellingPrice = product.DefaultSellingPrice,
            AverageCost = product.AverageCost,
            Description = product.Description,
            IsActive = product.IsActive,
            SellingUnits = product.SellingUnits.Select(u => new ProductSellingUnitDto
            {
                Id = u.Id,
                UnitName = u.UnitName,
                ConversionFactor = u.ConversionFactor,
                SellingPrice = u.SellingPrice,
                PurchasePrice = u.PurchasePrice,
                IsDefault = u.IsDefault
            }).ToList()
        }, "تم تحديث بيانات الصنف ووحدات البيع بنجاح");
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
