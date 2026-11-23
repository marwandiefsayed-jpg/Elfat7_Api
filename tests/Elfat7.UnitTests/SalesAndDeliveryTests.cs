using Elfat7.Application.DTOs.Deliveries;
using Elfat7.Application.DTOs.Sales;
using Elfat7.Application.DTOs.SalesReturns;
using Elfat7.Domain.Entities;
using Elfat7.Domain.Enums;
using Elfat7.Infrastructure.Persistence;
using Elfat7.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Elfat7.UnitTests;

public class SalesAndDeliveryTests
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
    public async Task DraftSalesInvoice_ShouldNotAffectInventoryOrCustomerDebt()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var customer = new Customer { Code = "CUST-0001", Name = "شركة الدلتا", Phone = "01011223344", Balance = 0 };
        var category = new ProductCategory { Code = "CAT-01", Name = "حبوب وغلال" };
        var product = new Product { Code = "PRD-0001", Name = "ذرة صفراء", Unit = "طن", Category = category };

        await context.Customers.AddAsync(customer);
        await context.ProductCategories.AddAsync(category);
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();

        var invoiceService = new SalesInvoiceService(context);

        // Act
        var result = await invoiceService.CreateSalesInvoiceAsync(new CreateSalesInvoiceDto
        {
            CustomerCode = "CUST-0001",
            AutoConfirm = false, // مسودة
            Items = new List<CreateSalesInvoiceItemDto>
            {
                new() { ProductCode = "PRD-0001", Quantity = 15, UnitPrice = 12000 }
            }
        });

        // Assert
        result.Succeeded.Should().BeTrue();
        var updatedCustomer = await context.Customers.FirstAsync(c => c.Id == customer.Id);
        updatedCustomer.Balance.Should().Be(0); // لم يتم تسجيل مديونية لأن الفاتورة مسودة

        var stockMovements = await context.StockMovements.CountAsync();
        stockMovements.Should().Be(0); // لا يوجد أي حركة مخزنية
    }

    [Fact]
    public async Task Delivery_WithSufficientStock_ShouldDeductStock_AndAddStockMovement()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var customer = new Customer { Code = "CUST-0001", Name = "شركة الدلتا", Phone = "01011223344" };
        var category = new ProductCategory { Code = "CAT-01", Name = "حبوب وغلال" };
        var product = new Product { Code = "PRD-0001", Name = "ذرة صفراء", Unit = "طن", Category = category, AverageCost = 10000m };
        var warehouse = new Warehouse { Code = "WH-01", Name = "مخزن طنطا" };

        var initialStock = new WarehouseStock
        {
            Warehouse = warehouse,
            Product = product,
            Quantity = 50m,
            ReservedQuantity = 0
        };

        var invoice = new SalesInvoice
        {
            InvoiceNumber = "INV-2026-0001",
            Customer = customer,
            Status = SalesInvoiceStatus.Confirmed,
            TotalAmount = 240000m,
            Items = new List<SalesInvoiceItem>
            {
                new() { Product = product, Quantity = 20m, UnitPrice = 12000m, LineTotal = 240000m, DeliveredQuantity = 0 }
            }
        };

        await context.Customers.AddAsync(customer);
        await context.ProductCategories.AddAsync(category);
        await context.Products.AddAsync(product);
        await context.Warehouses.AddAsync(warehouse);
        await context.WarehouseStocks.AddAsync(initialStock);
        await context.SalesInvoices.AddAsync(invoice);
        await context.SaveChangesAsync();

        var deliveryService = new DeliveryService(context);

        // Act: صرف وتسليم 20 طن
        var result = await deliveryService.CreateDeliveryAsync(new CreateDeliveryDto
        {
            SalesInvoiceNumber = "INV-2026-0001",
            WarehouseCode = "WH-01",
            Items = new List<CreateDeliveryItemDto>
            {
                new() { ProductCode = "PRD-0001", Quantity = 20m }
            }
        });

        // Assert
        result.Succeeded.Should().BeTrue();

        var updatedStock = await context.WarehouseStocks.FirstAsync(ws => ws.ProductId == product.Id);
        updatedStock.Quantity.Should().Be(30m); // 50 - 20 = 30 طن

        var updatedInvoice = await context.SalesInvoices.Include(i => i.Items).FirstAsync(i => i.Id == invoice.Id);
        updatedInvoice.Status.Should().Be(SalesInvoiceStatus.FullyDelivered);
        updatedInvoice.Items.First().DeliveredQuantity.Should().Be(20m);

        var movement = await context.StockMovements.FirstOrDefaultAsync(m => m.ProductId == product.Id);
        movement.Should().NotBeNull();
        movement!.MovementType.Should().Be(StockMovementType.SalesDelivery);
        movement.Quantity.Should().Be(-20m); // حركة منصرفة
    }

    [Fact]
    public async Task SalesReturn_ShouldRestoreStock_AndDecreaseCustomerDebt()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var customer = new Customer { Code = "CUST-0001", Name = "شركة الدلتا", Phone = "01011223344", Balance = 100000m };
        var category = new ProductCategory { Code = "CAT-01", Name = "حبوب وغلال" };
        var product = new Product { Code = "PRD-0001", Name = "أرز شعير", Unit = "طن", Category = category };
        var warehouse = new Warehouse { Code = "WH-01", Name = "مخزن طنطا" };

        var stock = new WarehouseStock
        {
            Warehouse = warehouse,
            Product = product,
            Quantity = 10m,
            ReservedQuantity = 0
        };

        var invoice = new SalesInvoice
        {
            InvoiceNumber = "INV-2026-0001",
            Customer = customer,
            Status = SalesInvoiceStatus.FullyDelivered,
            TotalAmount = 50000m,
            Items = new List<SalesInvoiceItem>
            {
                new() { Product = product, Quantity = 5m, UnitPrice = 10000m, LineTotal = 50000m, DeliveredQuantity = 5m }
            }
        };

        await context.Customers.AddAsync(customer);
        await context.ProductCategories.AddAsync(category);
        await context.Products.AddAsync(product);
        await context.Warehouses.AddAsync(warehouse);
        await context.WarehouseStocks.AddAsync(stock);
        await context.SalesInvoices.AddAsync(invoice);
        await context.SaveChangesAsync();

        var returnService = new SalesReturnService(context);

        // Act: إرجاع 2 طن من الفاتورة بسبب عدم مطابقة المواصفات (قيمة 2 * 10000 = 20000)
        var result = await returnService.CreateSalesReturnAsync(new CreateSalesReturnDto
        {
            SalesInvoiceNumber = "INV-2026-0001",
            WarehouseCode = "WH-01",
            Reason = "حبوب غير مطابقة لنسبة النقاوة المطلوبة",
            Items = new List<CreateSalesReturnItemDto>
            {
                new() { ProductCode = "PRD-0001", Quantity = 2m }
            }
        });

        // Assert
        result.Succeeded.Should().BeTrue();

        var updatedStock = await context.WarehouseStocks.FirstAsync(ws => ws.ProductId == product.Id);
        updatedStock.Quantity.Should().Be(12m); // 10 + 2 طن تم إرجاعها

        var updatedCustomer = await context.Customers.FirstAsync(c => c.Id == customer.Id);
        updatedCustomer.Balance.Should().Be(80000m); // 100000 - 20000

        var movement = await context.StockMovements.FirstOrDefaultAsync(m => m.MovementType == StockMovementType.SalesReturn);
        movement.Should().NotBeNull();
        movement!.Quantity.Should().Be(2m);
    }

    [Fact]
    public async Task SalesInvoice_WithCustomSellingUnit_ShouldSaveUnitAndConversionFactor()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var customer = new Customer { Code = "CUST-0099", Name = "محمود للتجارة", Phone = "01011223344", Balance = 0 };
        var category = new ProductCategory { Code = "CAT-01", Name = "حبوب وغلال" };
        var product = new Product
        {
            Code = "PRD-0099",
            Name = "أرز أبيض",
            Unit = "كجم",
            Category = category,
            DefaultSellingPrice = 30m,
            AverageCost = 25m,
            SellingUnits = new List<ProductSellingUnit>
            {
                new() { UnitName = "شكارة 50كجم", ConversionFactor = 50, SellingPrice = 1450m, IsDefault = true },
                new() { UnitName = "كجم", ConversionFactor = 1, SellingPrice = 30m, IsDefault = false }
            }
        };

        await context.Customers.AddAsync(customer);
        await context.ProductCategories.AddAsync(category);
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();

        var invoiceService = new SalesInvoiceService(context);

        // Act: بيع 10 شكاير بسعر 1450 ج.م للشكارة (إجمالي 14500 ج.م، تعادل 500 كجم)
        var result = await invoiceService.CreateSalesInvoiceAsync(new CreateSalesInvoiceDto
        {
            CustomerCode = "CUST-0099",
            AutoConfirm = true,
            Items = new List<CreateSalesInvoiceItemDto>
            {
                new()
                {
                    ProductCode = "PRD-0099",
                    Unit = "شكارة 50كجم",
                    ConversionFactor = 50,
                    Quantity = 10,
                    UnitPrice = 1450m
                }
            }
        });

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data!.TotalAmount.Should().Be(14500m);

        var savedInv = await invoiceService.GetSalesInvoiceByNumberAsync(result.Data.InvoiceNumber);
        savedInv.Should().NotBeNull();
        savedInv!.Items.Should().HaveCount(1);
        savedInv.Items[0].Unit.Should().Be("شكارة 50كجم");
        savedInv.Items[0].ConversionFactor.Should().Be(50);
        savedInv.Items[0].BaseQuantity.Should().Be(500); // 10 * 50
        savedInv.Items[0].LineTotal.Should().Be(14500m);
    }
}
