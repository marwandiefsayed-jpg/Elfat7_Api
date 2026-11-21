using Elfat7.Domain.Entities;
using Elfat7.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Elfat7.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(Elfat7DbContext context)
    {
        if (await context.Suppliers.AnyAsync())
            return;

        // 1. Treasuries
        var mainTreasury = new Treasury
        {
            Code = "TRS-01",
            Name = "الخزينة الرئيسية - المقر الإداري",
            Balance = 250000m,
            IsActive = true
        };
        var bankAccount = new Treasury
        {
            Code = "TRS-02",
            Name = "حساب بنك مصر - فرع طنطا",
            Balance = 800000m,
            IsActive = true
        };
        await context.Treasuries.AddRangeAsync(mainTreasury, bankAccount);
        await context.SaveChangesAsync();

        // 2. Categories
        var cat1 = new ProductCategory { Code = "CAT-01", Name = "حبوب وغلال", Description = "أرز، قمح، ذرة، شعير" };
        var cat2 = new ProductCategory { Code = "CAT-02", Name = "بقوليات", Description = "فول بلدي، عدس، حمص، لوبيا" };
        var cat3 = new ProductCategory { Code = "CAT-03", Name = "أسمدة ومخصبات زراعية", Description = "يوريا، نترات نشادر، سوبر فوسفات" };
        var cat4 = new ProductCategory { Code = "CAT-04", Name = "بذور وتقاوي", Description = "تقاوي معتمدة من وزارة الزراعة" };
        await context.ProductCategories.AddRangeAsync(cat1, cat2, cat3, cat4);
        await context.SaveChangesAsync();

        // 3. Products
        var p1 = new Product
        {
            Code = "PRD-0001",
            Name = "أرز شعير عريض الحبة درجة أولى",
            CategoryId = cat1.Id,
            Unit = "طن",
            DefaultPurchasePrice = 16500m,
            DefaultSellingPrice = 18200m,
            AverageCost = 16500m,
            Description = "أرز شعير عريض الحبة جيزة 178 نسبة نقاوة 98%"
        };
        var p2 = new Product
        {
            Code = "PRD-0002",
            Name = "قمح بلدي درجة أولى 23.5 قيراط",
            CategoryId = cat1.Id,
            Unit = "طن",
            DefaultPurchasePrice = 13200m,
            DefaultSellingPrice = 14500m,
            AverageCost = 13200m,
            Description = "قمح مصري نخب أول مناسب للطحن وإنتاج الدقيق الفاخر"
        };
        var p3 = new Product
        {
            Code = "PRD-0003",
            Name = "ذرة صفراء مجففة صب",
            CategoryId = cat1.Id,
            Unit = "طن",
            DefaultPurchasePrice = 11800m,
            DefaultSellingPrice = 13000m,
            AverageCost = 11800m,
            Description = "ذرة صفراء مجففة رطوبة لا تتجاوز 14%"
        };
        var p4 = new Product
        {
            Code = "PRD-0004",
            Name = "فول بلدي حبة عريضة معبأ",
            CategoryId = cat2.Id,
            Unit = "طن",
            DefaultPurchasePrice = 32000m,
            DefaultSellingPrice = 36000m,
            AverageCost = 32000m,
            Description = "فول بلدي محصول جديد سليم ونظيف خالي من السوس"
        };
        var p5 = new Product
        {
            Code = "PRD-0005",
            Name = "سماد يوريا 46% نتروجين (أبو قير)",
            CategoryId = cat3.Id,
            Unit = "شيكارة 50كجم",
            DefaultPurchasePrice = 720m,
            DefaultSellingPrice = 850m,
            AverageCost = 720m,
            Description = "سماد حبيبي سريع الذوبان 46% نتروجين شيكارة 50 كجم"
        };
        await context.Products.AddRangeAsync(p1, p2, p3, p4, p5);
        await context.SaveChangesAsync();

        // 4. Warehouses
        var w1 = new Warehouse
        {
            Code = "WH-01",
            Name = "مخزن الدلتا المركزي - طنطا",
            Address = "طريق طنطا - المحلة الكبرى، محافظة الغربية",
            KeeperName = "الحاج محمود عبد الفتاح",
            KeeperPhone = "01012345671"
        };
        var w2 = new Warehouse
        {
            Code = "WH-02",
            Name = "مخزن النوبارية الزراعي",
            Address = "الكيلو 75 طريق مصر - إسكندرية الصحراوي، محافظة البحيرة",
            KeeperName = "م. أشرف الصاوي",
            KeeperPhone = "01123456782"
        };
        var w3 = new Warehouse
        {
            Code = "WH-03",
            Name = "صوامع الإسماعيلية والقناة",
            Address = "المنطقة الصناعية الأولى، محافظة الإسماعيلية",
            KeeperName = "م. محمد رضوان",
            KeeperPhone = "01234567893"
        };
        await context.Warehouses.AddRangeAsync(w1, w2, w3);
        await context.SaveChangesAsync();

        // 5. Suppliers
        var sup1 = new Supplier
        {
            Code = "SUP-0001",
            Name = "شركة النيل لتوريد المحاصيل الزراعية",
            Phone = "01099887766",
            Address = "المنصورة - محافظة الدقهلية",
            CommercialRegistration = "128475",
            TaxNumber = "340-981-120",
            Balance = 45000m,
            Notes = "مورد رئيسي للأرز والقمح البلدي"
        };
        var sup2 = new Supplier
        {
            Code = "SUP-0002",
            Name = "مؤسسة الوادي للأسمدة والكيماويات",
            Phone = "01188776655",
            Address = "مدينة السادات - محافظة المنوفية",
            CommercialRegistration = "98231",
            TaxNumber = "512-443-889",
            Balance = 0m,
            Notes = "مورد معتمد لجميع الأسمدة والمخصبات"
        };
        var sup3 = new Supplier
        {
            Code = "SUP-0003",
            Name = "شركة طيبة لتجارة الحبوب والغلال",
            Phone = "01277665544",
            Address = "الزقازيق - محافظة الشرقية",
            CommercialRegistration = "67123",
            TaxNumber = "209-118-765",
            Balance = 82000m,
            Notes = "مورد ذرة صفراء وفول بلدي"
        };
        await context.Suppliers.AddRangeAsync(sup1, sup2, sup3);
        await context.SaveChangesAsync();

        // 6. Customers
        var cust1 = new Customer
        {
            Code = "CUST-0001",
            Name = "شركة الدلتا لتجارة الجملة والتوزيع",
            Phone = "01011223344",
            Address = "شارع الجيش، طنطا - محافظة الغربية",
            CommercialRegistration = "55412",
            CreditLimit = 500000m,
            Balance = 65000m,
            Notes = "عميل مميز - دفعات أسبوعية منتظمة"
        };
        var cust2 = new Customer
        {
            Code = "CUST-0002",
            Name = "مطاحن ومضارب القاهرة الكبرى",
            Phone = "01122334455",
            Address = "شبرا الخيمة - محافظة القليوبية",
            CommercialRegistration = "88901",
            CreditLimit = 1500000m,
            Balance = 120000m,
            Notes = "مشتريات قمح وأرز بكميات كبيرة"
        };
        var cust3 = new Customer
        {
            Code = "CUST-0003",
            Name = "منافذ الخير لتعبئة المواد الغذائية",
            Phone = "01233445566",
            Address = "دمنهور - محافظة البحيرة",
            CommercialRegistration = "33214",
            CreditLimit = 300000m,
            Balance = 0m,
            Notes = "سداد فوري عند الاستلام"
        };
        await context.Customers.AddRangeAsync(cust1, cust2, cust3);
        await context.SaveChangesAsync();

        // 7. Initial Warehouse Stocks & Stock Movements
        var stocks = new List<WarehouseStock>
        {
            new() { WarehouseId = w1.Id, ProductId = p1.Id, Quantity = 45m, ReservedQuantity = 0 },
            new() { WarehouseId = w1.Id, ProductId = p2.Id, Quantity = 80m, ReservedQuantity = 0 },
            new() { WarehouseId = w1.Id, ProductId = p3.Id, Quantity = 60m, ReservedQuantity = 0 },
            new() { WarehouseId = w2.Id, ProductId = p4.Id, Quantity = 25m, ReservedQuantity = 0 },
            new() { WarehouseId = w2.Id, ProductId = p5.Id, Quantity = 400m, ReservedQuantity = 0 },
            new() { WarehouseId = w3.Id, ProductId = p1.Id, Quantity = 30m, ReservedQuantity = 0 },
            new() { WarehouseId = w3.Id, ProductId = p2.Id, Quantity = 110m, ReservedQuantity = 0 }
        };
        await context.WarehouseStocks.AddRangeAsync(stocks);

        var movements = new List<StockMovement>
        {
            new() { MovementNumber = "MOV-2026-0001", WarehouseId = w1.Id, ProductId = p1.Id, MovementType = StockMovementType.PurchaseReceipt, Quantity = 45m, UnitCost = 16500m, TotalCost = 742500m, ReferenceNumber = "REC-2026-0001", Notes = "رصيد افتتاحي استلام مخزني" },
            new() { MovementNumber = "MOV-2026-0002", WarehouseId = w1.Id, ProductId = p2.Id, MovementType = StockMovementType.PurchaseReceipt, Quantity = 80m, UnitCost = 13200m, TotalCost = 1056000m, ReferenceNumber = "REC-2026-0001", Notes = "رصيد افتتاحي استلام مخزني" },
            new() { MovementNumber = "MOV-2026-0003", WarehouseId = w1.Id, ProductId = p3.Id, MovementType = StockMovementType.PurchaseReceipt, Quantity = 60m, UnitCost = 11800m, TotalCost = 708000m, ReferenceNumber = "REC-2026-0001", Notes = "رصيد افتتاحي استلام مخزني" },
            new() { MovementNumber = "MOV-2026-0004", WarehouseId = w2.Id, ProductId = p4.Id, MovementType = StockMovementType.PurchaseReceipt, Quantity = 25m, UnitCost = 32000m, TotalCost = 800000m, ReferenceNumber = "REC-2026-0002", Notes = "رصيد افتتاحي استلام مخزني" },
            new() { MovementNumber = "MOV-2026-0005", WarehouseId = w2.Id, ProductId = p5.Id, MovementType = StockMovementType.PurchaseReceipt, Quantity = 400m, UnitCost = 720m, TotalCost = 288000m, ReferenceNumber = "REC-2026-0002", Notes = "رصيد افتتاحي أسمدة" }
        };
        await context.StockMovements.AddRangeAsync(movements);

        // 8. Sample Operating Expenses
        var expenses = new List<OperatingExpense>
        {
            new() { ExpenseNumber = "EXP-2026-0001", TreasuryId = mainTreasury.Id, Category = "نقل وشحن", Amount = 12500m, ExpenseDate = DateTime.UtcNow.AddDays(-5), Description = "نولون نقل 50 طن قمح من الصوامع لمخزن طنطا", PaidTo = "سائقين أسطول النقل البري" },
            new() { ExpenseNumber = "EXP-2026-0002", TreasuryId = mainTreasury.Id, Category = "عمالة وتعتيق", Amount = 4800m, ExpenseDate = DateTime.UtcNow.AddDays(-3), Description = "أجرة تفريغ وتعتيق سيارات الأرز الشعير", PaidTo = "رئيس عمال التفريغ أحمد الشحات" },
            new() { ExpenseNumber = "EXP-2026-0003", TreasuryId = bankAccount.Id, Category = "إيجار مخازن", Amount = 25000m, ExpenseDate = DateTime.UtcNow.AddDays(-10), Description = "إيجار مخزن النوبارية الزراعي عن الشهر الحالي", PaidTo = "شركة الوادي للاستثمار العقاري" }
        };
        await context.OperatingExpenses.AddRangeAsync(expenses);

        await context.SaveChangesAsync();
    }
}
