using Elfat7.Application.Common;

namespace Elfat7.Application.DTOs.Products;

public class ProductCategoryDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }
}

public class CreateProductCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateProductCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ProductDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal DefaultPurchasePrice { get; set; }
    public string DefaultPurchasePriceFormatted => ArabicLocalizationHelper.FormatCurrency(DefaultPurchasePrice);
    public decimal DefaultSellingPrice { get; set; }
    public string DefaultSellingPriceFormatted => ArabicLocalizationHelper.FormatCurrency(DefaultSellingPrice);
    public decimal AverageCost { get; set; }
    public string AverageCostFormatted => ArabicLocalizationHelper.FormatCurrency(AverageCost);
    public decimal TotalStockQuantity { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class ProductDetailsDto : ProductDto
{
    public List<ProductWarehouseStockDto> WarehouseStocks { get; set; } = new();
}

public class ProductWarehouseStockDto
{
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal ReservedQuantity { get; set; }
    public decimal AvailableQuantity { get; set; }
}

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string CategoryCode { get; set; } = string.Empty;
    public string Unit { get; set; } = "طن";
    public decimal DefaultPurchasePrice { get; set; }
    public decimal DefaultSellingPrice { get; set; }
    public string? Description { get; set; }
}

public class UpdateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string CategoryCode { get; set; } = string.Empty;
    public string Unit { get; set; } = "طن";
    public decimal DefaultPurchasePrice { get; set; }
    public decimal DefaultSellingPrice { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ProductFilterDto : PagedFilterRequest
{
    public string? CategoryCode { get; set; }
    public bool? IsActive { get; set; }
}
