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
}
