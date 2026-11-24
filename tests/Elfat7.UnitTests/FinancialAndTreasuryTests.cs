using Elfat7.Application.DTOs.Financial;
using Elfat7.Domain.Entities;
using Elfat7.Domain.Enums;
using Elfat7.Infrastructure.Persistence;
using Elfat7.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Elfat7.UnitTests;

public class FinancialAndTreasuryTests
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
    public async Task SupplierPayment_ShouldDecreaseSupplierDebt_AndDecreaseTreasuryBalance()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var supplier = new Supplier { Code = "SUP-0001", Name = "شركة النيل", Phone = "01099887766", Balance = 50000m };
        var treasury = new Treasury { Code = "TRS-01", Name = "الخزينة الرئيسية", Balance = 100000m };

        await context.Suppliers.AddAsync(supplier);
        await context.Treasuries.AddAsync(treasury);
        await context.SaveChangesAsync();

        var finService = new FinancialService(context);

        // Act: سداد 20,000 ج.م للمورد
        var result = await finService.CreateSupplierPaymentAsync(new CreateSupplierPaymentDto
        {
            SupplierCode = "SUP-0001",
            TreasuryCode = "TRS-01",
            Amount = 20000m,
            PaymentMethod = PaymentMethod.Cash
        });

        // Assert
        result.Succeeded.Should().BeTrue();

        var updatedSupplier = await context.Suppliers.FirstAsync(s => s.Id == supplier.Id);
        updatedSupplier.Balance.Should().Be(30000m); // 50000 - 20000

        var updatedTreasury = await context.Treasuries.FirstAsync(t => t.Id == treasury.Id);
        updatedTreasury.Balance.Should().Be(80000m); // 100000 - 20000

        var trx = await context.TreasuryTransactions.FirstOrDefaultAsync(t => t.Category == TreasuryTransactionCategory.SupplierPayment);
        trx.Should().NotBeNull();
        trx!.TransactionType.Should().Be(TreasuryTransactionType.Outflow);
        trx.Amount.Should().Be(20000m);
    }

    [Fact]
    public async Task CustomerPayment_ShouldDecreaseCustomerDebt_AndIncreaseTreasuryBalance()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var customer = new Customer { Code = "CUST-0001", Name = "شركة الدلتا", Phone = "01011223344", Balance = 70000m };
        var treasury = new Treasury { Code = "TRS-01", Name = "الخزينة الرئيسية", Balance = 100000m };
        var invoice = new SalesInvoice
        {
            InvoiceNumber = "INV-2026-0001",
            Customer = customer,
            Status = SalesInvoiceStatus.Confirmed,
            TotalAmount = 50000m,
            PaidAmount = 0m,
            PaymentStatus = InvoicePaymentStatus.Unpaid
        };

        await context.Customers.AddAsync(customer);
        await context.Treasuries.AddAsync(treasury);
        await context.SalesInvoices.AddAsync(invoice);
        await context.SaveChangesAsync();

        var finService = new FinancialService(context);

        // Act: تحصيل 50,000 ج.م من العميل وسداد الفاتورة
        var result = await finService.CreateCustomerPaymentAsync(new CreateCustomerPaymentDto
        {
            CustomerCode = "CUST-0001",
            TreasuryCode = "TRS-01",
            SalesInvoiceNumber = "INV-2026-0001",
            Amount = 50000m,
            PaymentMethod = PaymentMethod.BankTransfer
        });

        // Assert
        result.Succeeded.Should().BeTrue();

        var updatedCustomer = await context.Customers.FirstAsync(c => c.Id == customer.Id);
        updatedCustomer.Balance.Should().Be(20000m); // 70000 - 50000

        var updatedTreasury = await context.Treasuries.FirstAsync(t => t.Id == treasury.Id);
        updatedTreasury.Balance.Should().Be(150000m); // 100000 + 50000

        var updatedInvoice = await context.SalesInvoices.FirstAsync(i => i.Id == invoice.Id);
        updatedInvoice.PaidAmount.Should().Be(50000m);
        updatedInvoice.PaymentStatus.Should().Be(InvoicePaymentStatus.FullyPaid);
    }

    [Fact]
    public async Task GeneralCustomerPayment_ShouldAllocateAcrossUnpaidInvoices_AndReduceBalance()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var customer = new Customer { Code = "CUST-0002", Name = "محمصة الفتح", Phone = "01022334455", Balance = 8000m };
        var treasury = new Treasury { Code = "TRS-01", Name = "الخزينة الرئيسية", Balance = 50000m, IsActive = true };

        var invoice1 = new SalesInvoice
        {
            InvoiceNumber = "INV-2026-0001",
            Customer = customer,
            Status = SalesInvoiceStatus.Confirmed,
            TotalAmount = 3000m,
            PaidAmount = 0m,
            PaymentStatus = InvoicePaymentStatus.Unpaid,
            InvoiceDate = DateTime.UtcNow.AddDays(-2)
        };

        var invoice2 = new SalesInvoice
        {
            InvoiceNumber = "INV-2026-0002",
            Customer = customer,
            Status = SalesInvoiceStatus.Confirmed,
            TotalAmount = 5000m,
            PaidAmount = 0m,
            PaymentStatus = InvoicePaymentStatus.Unpaid,
            InvoiceDate = DateTime.UtcNow.AddDays(-1)
        };

        await context.Customers.AddAsync(customer);
        await context.Treasuries.AddAsync(treasury);
        await context.SalesInvoices.AddRangeAsync(invoice1, invoice2);
        await context.SaveChangesAsync();

        var finService = new FinancialService(context);

        // Act: سداد عام للمديونية بقيمة 4,000 ج.م بدون تحديد فاتورة
        var result = await finService.CreateCustomerPaymentAsync(new CreateCustomerPaymentDto
        {
            CustomerCode = "CUST-0002",
            Amount = 4000m,
            PaymentMethod = PaymentMethod.Cash
        });

        // Assert
        result.Succeeded.Should().BeTrue();

        var updatedInvoice1 = await context.SalesInvoices.FirstAsync(i => i.Id == invoice1.Id);
        updatedInvoice1.PaidAmount.Should().Be(3000m);
        updatedInvoice1.PaymentStatus.Should().Be(InvoicePaymentStatus.FullyPaid);

        var updatedInvoice2 = await context.SalesInvoices.FirstAsync(i => i.Id == invoice2.Id);
        updatedInvoice2.PaidAmount.Should().Be(1000m);
        updatedInvoice2.PaymentStatus.Should().Be(InvoicePaymentStatus.PartiallyPaid);

        var updatedCustomer = await context.Customers.FirstAsync(c => c.Id == customer.Id);
        updatedCustomer.Balance.Should().Be(4000m); // 8000 - 4000

        var updatedTreasury = await context.Treasuries.FirstAsync(t => t.Id == treasury.Id);
        updatedTreasury.Balance.Should().Be(54000m); // 50000 + 4000
    }

    [Fact]
    public async Task CustomerPayment_Validation_ShouldFailIfNegativeOrExceedsDebt()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var customer = new Customer { Code = "CUST-0003", Name = "عميل تجربة", Phone = "01033445566", Balance = 5000m };
        var treasury = new Treasury { Code = "TRS-01", Name = "الخزينة الرئيسية", Balance = 50000m, IsActive = true };

        var invoice = new SalesInvoice
        {
            InvoiceNumber = "INV-2026-0003",
            Customer = customer,
            Status = SalesInvoiceStatus.Confirmed,
            TotalAmount = 5000m,
            PaidAmount = 0m,
            PaymentStatus = InvoicePaymentStatus.Unpaid
        };

        await context.Customers.AddAsync(customer);
        await context.Treasuries.AddAsync(treasury);
        await context.SalesInvoices.AddAsync(invoice);
        await context.SaveChangesAsync();

        var finService = new FinancialService(context);

        // Act & Assert 1: Zero or negative amount
        var zeroResult = await finService.CreateCustomerPaymentAsync(new CreateCustomerPaymentDto
        {
            CustomerCode = "CUST-0003",
            Amount = 0m
        });
        zeroResult.Succeeded.Should().BeFalse();

        // Act & Assert 2: Amount exceeds total debt
        var excessResult = await finService.CreateCustomerPaymentAsync(new CreateCustomerPaymentDto
        {
            CustomerCode = "CUST-0003",
            Amount = 10000m
        });
        excessResult.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task CustomerService_GetCustomerByCode_ShouldReturnFullInvoicesAndPayments()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var product = new Product { Code = "PRD-0001", Name = "لب عباد عالي", Unit = "شكارة 25كجم" };
        var customer = new Customer { Code = "CUST-0004", Name = "محمصة البركة", Phone = "01044556677", Balance = 12000m };
        var treasury = new Treasury { Code = "TRS-01", Name = "الخزينة الرئيسية", Balance = 100000m, IsActive = true };

        var invoice = new SalesInvoice
        {
            InvoiceNumber = "INV-2026-0004",
            Customer = customer,
            Status = SalesInvoiceStatus.Confirmed,
            TotalAmount = 12000m,
            PaidAmount = 2000m,
            PaymentStatus = InvoicePaymentStatus.PartiallyPaid,
            Items = new List<SalesInvoiceItem>
            {
                new() { Product = product, Quantity = 10, Unit = "شكارة 25كجم", UnitPrice = 1200m, LineTotal = 12000m }
            }
        };

        var payment = new CustomerPayment
        {
            ReceiptNumber = "CPAY-2026-0001",
            Customer = customer,
            Treasury = treasury,
            SalesInvoice = invoice,
            Amount = 2000m,
            PaymentDate = DateTime.UtcNow
        };

        await context.Products.AddAsync(product);
        await context.Customers.AddAsync(customer);
        await context.Treasuries.AddAsync(treasury);
        await context.SalesInvoices.AddAsync(invoice);
        await context.CustomerPayments.AddAsync(payment);
        await context.SaveChangesAsync();

        var customerService = new CustomerService(context);

        // Act
        var details = await customerService.GetCustomerByCodeAsync("CUST-0004");

        // Assert
        details.Should().NotBeNull();
        details!.Balance.Should().Be(10000m); // 12000 - 2000
        details.TotalInvoiced.Should().Be(12000m);
        details.TotalPaid.Should().Be(2000m);
        details.RecentInvoices.Should().HaveCount(1);
        details.RecentInvoices[0].RemainingAmount.Should().Be(10000m);
        details.RecentInvoices[0].Items.Should().HaveCount(1);
        details.RecentInvoices[0].Items[0].ProductName.Should().Be("لب عباد عالي");
        details.RecentPayments.Should().HaveCount(1);
        details.RecentPayments[0].Amount.Should().Be(2000m);
        details.RecentPayments[0].SalesInvoiceNumber.Should().Be("INV-2026-0004");
    }

    [Fact]
    public async Task CustomerPayment_WithoutTreasury_ShouldSucceed_AndReduceCustomerDebt()
    {
        // Arrange: لا توجد أي خزينة مسجلة في قاعدة البيانات
        var context = CreateInMemoryDbContext();
        var customer = new Customer { Code = "CUST-0099", Name = "عميل بدون خزينة", Phone = "01000000000", Balance = 15000m };
        var invoice = new SalesInvoice
        {
            InvoiceNumber = "INV-2026-0099",
            Customer = customer,
            Status = SalesInvoiceStatus.Confirmed,
            TotalAmount = 15000m,
            PaidAmount = 0m,
            PaymentStatus = InvoicePaymentStatus.Unpaid
        };

        await context.Customers.AddAsync(customer);
        await context.SalesInvoices.AddAsync(invoice);
        await context.SaveChangesAsync();

        var finService = new FinancialService(context);

        // Act: سداد 5,000 ج.م بدون تحديد أي خزينة
        var result = await finService.CreateCustomerPaymentAsync(new CreateCustomerPaymentDto
        {
            CustomerCode = "CUST-0099",
            SalesInvoiceNumber = "INV-2026-0099",
            Amount = 5000m,
            PaymentMethod = PaymentMethod.Cash
        });

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.ReceiptNumber.Should().StartWith("CPAY-");

        var updatedCustomer = await context.Customers.FirstAsync(c => c.Id == customer.Id);
        updatedCustomer.Balance.Should().Be(10000m); // 15000 - 5000

        var updatedInvoice = await context.SalesInvoices.FirstAsync(i => i.Id == invoice.Id);
        updatedInvoice.PaidAmount.Should().Be(5000m);
        updatedInvoice.PaymentStatus.Should().Be(InvoicePaymentStatus.PartiallyPaid);

        var payment = await context.CustomerPayments.FirstOrDefaultAsync(p => p.CustomerId == customer.Id);
        payment.Should().NotBeNull();
        payment!.TreasuryId.Should().BeNull();
        payment.Amount.Should().Be(5000m);
    }

    [Fact]
    public async Task GeneralCustomerPayment_WithoutTreasury_33000_ShouldSucceed_AndSettleAllInvoices()
    {
        // Arrange: سيناريو المستخدم - سداد عام للمديونية بقيمة 33,000 ج.م بدون تحديد خزينة ولا فاتورة
        var context = CreateInMemoryDbContext();
        var customer = new Customer { Code = "CUST-0004", Name = "marwan", Phone = "01000000000", Balance = 33000m };

        // فاتورة 1: إجمالي 40,000 مسدد منها 10,000 والمتبقي 30,000
        var invoice1 = new SalesInvoice
        {
            InvoiceNumber = "INV-2026-0001",
            Customer = customer,
            Status = SalesInvoiceStatus.Confirmed,
            TotalAmount = 40000m,
            PaidAmount = 10000m,
            PaymentStatus = InvoicePaymentStatus.PartiallyPaid,
            InvoiceDate = DateTime.UtcNow.AddDays(-2)
        };

        // فاتورة 2: إجمالي 3,000 مسدد منها 0 والمتبقي 3,000
        var invoice2 = new SalesInvoice
        {
            InvoiceNumber = "INV-2026-0003",
            Customer = customer,
            Status = SalesInvoiceStatus.Confirmed,
            TotalAmount = 3000m,
            PaidAmount = 0m,
            PaymentStatus = InvoicePaymentStatus.Unpaid,
            InvoiceDate = DateTime.UtcNow.AddDays(-1)
        };

        await context.Customers.AddAsync(customer);
        await context.SalesInvoices.AddRangeAsync(invoice1, invoice2);
        await context.SaveChangesAsync();

        var finService = new FinancialService(context);

        // Act: سداد عام للمديونية بقيمة 33,000 ج.م بدون تحديد خزينة (TreasuryCode = null) وبدون تحديد فاتورة (SalesInvoiceNumber = null)
        var result = await finService.CreateCustomerPaymentAsync(new CreateCustomerPaymentDto
        {
            CustomerCode = "CUST-0004",
            Amount = 33000m,
            PaymentMethod = PaymentMethod.Cash
        });

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Amount.Should().Be(33000m);
        result.Data.ReceiptNumber.Should().StartWith("CPAY-");

        // تحقق من تسوية مديونية العميل بالكامل
        var updatedCustomer = await context.Customers.FirstAsync(c => c.Id == customer.Id);
        updatedCustomer.Balance.Should().Be(0m);

        // تحقق من اكتمال سداد الفاتورة الأولى (40,000 مسدد بالكامل)
        var updatedInv1 = await context.SalesInvoices.FirstAsync(i => i.Id == invoice1.Id);
        updatedInv1.PaidAmount.Should().Be(40000m);
        updatedInv1.PaymentStatus.Should().Be(InvoicePaymentStatus.FullyPaid);

        // تحقق من اكتمال سداد الفاتورة الثانية (3,000 مسدد بالكامل)
        var updatedInv2 = await context.SalesInvoices.FirstAsync(i => i.Id == invoice2.Id);
        updatedInv2.PaidAmount.Should().Be(3000m);
        updatedInv2.PaymentStatus.Should().Be(InvoicePaymentStatus.FullyPaid);

        // تحقق من تسجيل سند القبض بدون خطأ
        var payment = await context.CustomerPayments.FirstOrDefaultAsync(p => p.CustomerId == customer.Id);
        payment.Should().NotBeNull();
        payment!.Amount.Should().Be(33000m);
        payment.TreasuryId.Should().BeNull();
    }
}
