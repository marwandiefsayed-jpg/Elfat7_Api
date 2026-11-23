using Elfat7.Application.DTOs.GoodsReceiving;
using Elfat7.Application.DTOs.Inventory;
using Elfat7.Application.DTOs.Purchasing;
using Elfat7.Domain.Entities;
using Elfat7.Domain.Enums;
using Elfat7.Infrastructure.Persistence;
using Elfat7.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Elfat7.UnitTests;

public class InventoryAndCostingTests
{
    private Elfat7DbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<Elfat7DbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new Elfat7DbContext(options);
    }

    [Fact]
    public async Task CreatingPurchaseOrder_ShouldNotIncreaseStock()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var supplier = new Supplier { Code = "SUP-0001", Name = "شركة النيل", Phone = "01012345678" };
        var category = new ProductCategory { Code = "CAT-01", Name = "حبوب وغلال" };
        var product = new Product { Code = "PRD-0001", Name = "أرز شعير", Unit = "طن", Category = category };
        var warehouse = new Warehouse { Code = "WH-01", Name = "مخزن طنطا" };

        await context.Suppliers.AddAsync(supplier);
        await context.ProductCategories.AddAsync(category);
        await context.Products.AddAsync(product);
        await context.Warehouses.AddAsync(warehouse);
        await context.SaveChangesAsync();

        var poService = new PurchaseOrderService(context);

        // Act
        var result = await poService.CreatePurchaseOrderAsync(new CreatePurchaseOrderDto
        {
            SupplierCode = supplier.Code,
            Items = new List<CreatePurchaseOrderItemDto>
            {
                new() { ProductCode = product.Code, Quantity = 20, UnitPrice = 16000 }
            }
        });

        // Assert
        result.Succeeded.Should().BeTrue();
        var stock = await context.WarehouseStocks.FirstOrDefaultAsync(ws => ws.ProductId == product.Id);
        stock.Should().BeNull(); // لم يتم إنشاء أي رصيد مخزني
    }

    [Fact]
    public async Task GoodsReceipt_ShouldIncreaseStock_AndCalculateMovingAverageCostCorrectly()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var supplier = new Supplier { Code = "SUP-0001", Name = "شركة النيل", Phone = "01012345678", Balance = 0 };
        var category = new ProductCategory { Code = "CAT-01", Name = "حبوب وغلال" };
        var product = new Product
        {
            Code = "PRD-0001",
            Name = "قمح مصري",
            Unit = "طن",
            Category = category,
            AverageCost = 10000m // متوسط التكلفة القديم
        };
        var warehouse = new Warehouse { Code = "WH-01", Name = "مخزن طنطا" };

        // رصيد سابق: 10 طن بسعر 10000
        var initialStock = new WarehouseStock
        {
            Warehouse = warehouse,
            Product = product,
            Quantity = 10m,
            ReservedQuantity = 0
        };

        var po = new PurchaseOrder
        {
            OrderNumber = "PO-2026-0001",
            Supplier = supplier,
            Status = PurchaseOrderStatus.Confirmed,
            Items = new List<PurchaseOrderItem>
            {
                new() { Product = product, Quantity = 30, UnitPrice = 14000, LineTotal = 420000 }
            }
        };

        await context.Suppliers.AddAsync(supplier);
        await context.ProductCategories.AddAsync(category);
        await context.Products.AddAsync(product);
        await context.Warehouses.AddAsync(warehouse);
        await context.WarehouseStocks.AddAsync(initialStock);
        await context.PurchaseOrders.AddAsync(po);
        await context.SaveChangesAsync();

        var receiptService = new GoodsReceiptService(context);

        // Act: استلام 10 طن جديدة بسعر 14000
        // التكلفة الجديدة المتوقعة = ((10 * 10000) + (10 * 14000)) / (10 + 10) = 240000 / 20 = 12000
        var result = await receiptService.CreateGoodsReceiptAsync(new CreateGoodsReceiptDto
        {
            PurchaseOrderNumber = "PO-2026-0001",
            WarehouseCode = "WH-01",
            Items = new List<CreateGoodsReceiptItemDto>
            {
                new() { ProductCode = "PRD-0001", Quantity = 10, UnitCost = 14000 }
            }
        });

        // Assert
        result.Succeeded.Should().BeTrue();

        var updatedStock = await context.WarehouseStocks.FirstAsync(ws => ws.ProductId == product.Id);
        updatedStock.Quantity.Should().Be(20m); // 10 قديم + 10 مستلم

        var updatedProduct = await context.Products.FirstAsync(p => p.Id == product.Id);
        updatedProduct.AverageCost.Should().Be(12000m); // متوسط التكلفة المرجح الدقيق

        var updatedSupplier = await context.Suppliers.FirstAsync(s => s.Id == supplier.Id);
        updatedSupplier.Balance.Should().Be(140000m); // 10 * 14000

        var movement = await context.StockMovements.FirstOrDefaultAsync(m => m.ProductId == product.Id);
        movement.Should().NotBeNull();
        movement!.MovementType.Should().Be(StockMovementType.PurchaseReceipt);
        movement.Quantity.Should().Be(10m);
    }

    [Fact]
    public async Task TransferStock_WithInsufficientQuantity_ShouldFailAndPreventNegativeStock()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var category = new ProductCategory { Code = "CAT-01", Name = "حبوب وغلال" };
        var product = new Product { Code = "PRD-0001", Name = "أرز شعير", Unit = "طن", Category = category };
        var whSource = new Warehouse { Code = "WH-01", Name = "مخزن طنطا" };
        var whDest = new Warehouse { Code = "WH-02", Name = "مخزن النوبارية" };

        var stock = new WarehouseStock
        {
            Warehouse = whSource,
            Product = product,
            Quantity = 5m, // الرصيد المتاح 5 طن فقط
            ReservedQuantity = 0
        };

        await context.ProductCategories.AddAsync(category);
        await context.Products.AddAsync(product);
        await context.Warehouses.AddRangeAsync(whSource, whDest);
        await context.WarehouseStocks.AddAsync(stock);
        await context.SaveChangesAsync();

        var inventoryService = new InventoryService(context);

        // Act: محاولة تحويل 10 طن (أكبر من الرصيد)
        var result = await inventoryService.CreateStockTransferAsync(new CreateStockTransferDto
        {
            SourceWarehouseCode = "WH-01",
            DestinationWarehouseCode = "WH-02",
            Items = new List<CreateStockTransferItemDto>
            {
                new() { ProductCode = "PRD-0001", Quantity = 10m }
            }
        });

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Message.Should().Contain("غير كافٍ");

        var checkStock = await context.WarehouseStocks.FirstAsync(ws => ws.WarehouseId == whSource.Id);
        checkStock.Quantity.Should().Be(5m); // لم يتغير الرصيد ولم يصبح سالباً
    }

    [Fact]
    public async Task UpdateStockQuantity_WithSellingUnitConversion_ShouldConvertAndSetStockCorrectly()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var category = new ProductCategory { Code = "CAT-01", Name = "حبوب وغلال" };
        var product = new Product
        {
            Code = "PRD-0077",
            Name = "عدس أصفر مجروش",
            Unit = "كجم",
            Category = category,
            DefaultSellingPrice = 50m,
            AverageCost = 40m,
            SellingUnits = new List<ProductSellingUnit>
            {
                new() { UnitName = "شكارة 25كجم", ConversionFactor = 25, SellingPrice = 1200m, IsDefault = true }
            }
        };
        var warehouse = new Warehouse { Code = "WH-01", Name = "مخزن العبور" };

        await context.ProductCategories.AddAsync(category);
        await context.Products.AddAsync(product);
        await context.Warehouses.AddAsync(warehouse);
        await context.SaveChangesAsync();

        var inventoryService = new InventoryService(context);

        // Act: ضبط رصيد المخزن إلى 40 شكارة 25كجم (تعادل 40 * 25 = 1000 كجم)
        var result = await inventoryService.UpdateStockQuantityAsync(new UpdateStockQuantityDto
        {
            WarehouseCode = "WH-01",
            ProductCode = "PRD-0077",
            Unit = "شكارة 25كجم",
            ConversionFactor = 25,
            Quantity = 40,
            Notes = "جرد دوري شهري"
        });

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data!.Quantity.Should().Be(1000m); // الرصيد المخزني الفعلي بالوحدة الأساسية (كجم)

        var stock = await context.WarehouseStocks.FirstAsync(ws => ws.ProductId == product.Id && ws.WarehouseId == warehouse.Id);
        stock.Quantity.Should().Be(1000m);

        var movement = await context.StockMovements.FirstOrDefaultAsync(m => m.ProductId == product.Id);
        movement.Should().NotBeNull();
        movement!.Quantity.Should().Be(1000m);
        movement.MovementType.Should().Be(StockMovementType.AdjustmentIncrease);
    }
}
