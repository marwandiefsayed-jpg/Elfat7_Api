using Elfat7.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elfat7.Infrastructure.Persistence.Configurations;

public class EntityConfigurations :
    IEntityTypeConfiguration<Supplier>,
    IEntityTypeConfiguration<ProductCategory>,
    IEntityTypeConfiguration<Product>,
    IEntityTypeConfiguration<ProductSellingUnit>,
    IEntityTypeConfiguration<Warehouse>,
    IEntityTypeConfiguration<WarehouseStock>,
    IEntityTypeConfiguration<StockMovement>,
    IEntityTypeConfiguration<StockTransfer>,
    IEntityTypeConfiguration<StockAdjustment>,
    IEntityTypeConfiguration<Customer>,
    IEntityTypeConfiguration<PurchaseOrder>,
    IEntityTypeConfiguration<GoodsReceipt>,
    IEntityTypeConfiguration<SalesOrder>,
    IEntityTypeConfiguration<SalesInvoice>,
    IEntityTypeConfiguration<Delivery>,
    IEntityTypeConfiguration<SalesReturn>,
    IEntityTypeConfiguration<Treasury>,
    IEntityTypeConfiguration<TreasuryTransaction>,
    IEntityTypeConfiguration<SupplierPayment>,
    IEntityTypeConfiguration<CustomerPayment>,
    IEntityTypeConfiguration<OperatingExpense>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Code).IsUnique();
        builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Phone).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Balance).HasPrecision(18, 2);
    }

    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Code).IsUnique();
        builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
    }

    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Code).IsUnique();
        builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Unit).HasMaxLength(50).IsRequired();
        builder.Property(x => x.DefaultPurchasePrice).HasPrecision(18, 2);
        builder.Property(x => x.DefaultSellingPrice).HasPrecision(18, 2);
        builder.Property(x => x.AverageCost).HasPrecision(18, 2);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Code).IsUnique();
        builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
    }

    public void Configure(EntityTypeBuilder<WarehouseStock> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.WarehouseId, x.ProductId }).IsUnique();
        builder.Property(x => x.Quantity).HasPrecision(18, 3);
        builder.Property(x => x.ReservedQuantity).HasPrecision(18, 3);

        builder.HasOne(x => x.Warehouse)
            .WithMany(x => x.Stocks)
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Product)
            .WithMany(x => x.WarehouseStocks)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.MovementNumber).IsUnique();
        builder.Property(x => x.MovementNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Quantity).HasPrecision(18, 3);
        builder.Property(x => x.UnitCost).HasPrecision(18, 2);
        builder.Property(x => x.TotalCost).HasPrecision(18, 2);
        builder.Property(x => x.ReferenceNumber).HasMaxLength(50);

        builder.HasOne(x => x.Warehouse)
            .WithMany(x => x.Movements)
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Product)
            .WithMany(x => x.StockMovements)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<StockTransfer> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.TransferNumber).IsUnique();
        builder.Property(x => x.TransferNumber).HasMaxLength(50).IsRequired();

        builder.HasOne(x => x.SourceWarehouse)
            .WithMany()
            .HasForeignKey(x => x.SourceWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DestinationWarehouse)
            .WithMany()
            .HasForeignKey(x => x.DestinationWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.StockTransfer)
            .HasForeignKey(x => x.StockTransferId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<StockAdjustment> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.AdjustmentNumber).IsUnique();
        builder.Property(x => x.AdjustmentNumber).HasMaxLength(50).IsRequired();

        builder.HasOne(x => x.Warehouse)
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.StockAdjustment)
            .HasForeignKey(x => x.StockAdjustmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Code).IsUnique();
        builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Phone).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CreditLimit).HasPrecision(18, 2);
        builder.Property(x => x.Balance).HasPrecision(18, 2);
    }

    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.OrderNumber).IsUnique();
        builder.Property(x => x.OrderNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.SubTotal).HasPrecision(18, 2);
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);

        builder.HasOne(x => x.Supplier)
            .WithMany(x => x.PurchaseOrders)
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.PurchaseOrder)
            .HasForeignKey(x => x.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<GoodsReceipt> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.ReceiptNumber).IsUnique();
        builder.Property(x => x.ReceiptNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);

        builder.HasOne(x => x.PurchaseOrder)
            .WithMany(x => x.GoodsReceipts)
            .HasForeignKey(x => x.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Supplier)
            .WithMany(x => x.GoodsReceipts)
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Warehouse)
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.GoodsReceipt)
            .HasForeignKey(x => x.GoodsReceiptId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.OrderNumber).IsUnique();
        builder.Property(x => x.OrderNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.SubTotal).HasPrecision(18, 2);
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);

        builder.HasOne(x => x.Customer)
            .WithMany(x => x.SalesOrders)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.SalesOrder)
            .HasForeignKey(x => x.SalesOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<SalesInvoice> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.InvoiceNumber).IsUnique();
        builder.Property(x => x.InvoiceNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.SubTotal).HasPrecision(18, 2);
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.PaidAmount).HasPrecision(18, 2);

        builder.HasOne(x => x.Customer)
            .WithMany(x => x.SalesInvoices)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SalesOrder)
            .WithMany(x => x.SalesInvoices)
            .HasForeignKey(x => x.SalesOrderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.SalesInvoice)
            .HasForeignKey(x => x.SalesInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<Delivery> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.DeliveryNumber).IsUnique();
        builder.Property(x => x.DeliveryNumber).HasMaxLength(50).IsRequired();

        builder.HasOne(x => x.SalesInvoice)
            .WithMany(x => x.Deliveries)
            .HasForeignKey(x => x.SalesInvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Warehouse)
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.Delivery)
            .HasForeignKey(x => x.DeliveryId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<SalesReturn> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.ReturnNumber).IsUnique();
        builder.Property(x => x.ReturnNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);

        builder.HasOne(x => x.SalesInvoice)
            .WithMany(x => x.Returns)
            .HasForeignKey(x => x.SalesInvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Customer)
            .WithMany(x => x.SalesReturns)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Warehouse)
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.SalesReturn)
            .HasForeignKey(x => x.SalesReturnId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<Treasury> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Code).IsUnique();
        builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Balance).HasPrecision(18, 2);
    }

    public void Configure(EntityTypeBuilder<TreasuryTransaction> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.TransactionNumber).IsUnique();
        builder.Property(x => x.TransactionNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 2);

        builder.HasOne(x => x.Treasury)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.TreasuryId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<SupplierPayment> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.PaymentNumber).IsUnique();
        builder.Property(x => x.PaymentNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 2);

        builder.HasOne(x => x.Supplier)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Treasury)
            .WithMany()
            .HasForeignKey(x => x.TreasuryId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<CustomerPayment> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.ReceiptNumber).IsUnique();
        builder.Property(x => x.ReceiptNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 2);

        builder.HasOne(x => x.Customer)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Treasury)
            .WithMany()
            .HasForeignKey(x => x.TreasuryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SalesInvoice)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.SalesInvoiceId)
            .OnDelete(DeleteBehavior.SetNull);
    }

    public void Configure(EntityTypeBuilder<OperatingExpense> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.ExpenseNumber).IsUnique();
        builder.Property(x => x.ExpenseNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.Category).HasMaxLength(100).IsRequired();

        builder.HasOne(x => x.Treasury)
            .WithMany()
            .HasForeignKey(x => x.TreasuryId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<ProductSellingUnit> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UnitName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ConversionFactor).HasPrecision(18, 4);
        builder.Property(x => x.SellingPrice).HasPrecision(18, 2);
        builder.Property(x => x.PurchasePrice).HasPrecision(18, 2);

        builder.HasOne(x => x.Product)
            .WithMany(p => p.SellingUnits)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<SalesInvoiceItem> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Unit).HasMaxLength(50);
        builder.Property(x => x.ConversionFactor).HasPrecision(18, 4);
        builder.Property(x => x.Quantity).HasPrecision(18, 3);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Property(x => x.UnitCost).HasPrecision(18, 2);
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        builder.Property(x => x.LineTotal).HasPrecision(18, 2);
        builder.Property(x => x.DeliveredQuantity).HasPrecision(18, 3);

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
