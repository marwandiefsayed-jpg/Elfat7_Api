using Elfat7.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Elfat7.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Supplier> Suppliers { get; }
    DbSet<ProductCategory> ProductCategories { get; }
    DbSet<Product> Products { get; }
    DbSet<Warehouse> Warehouses { get; }
    DbSet<WarehouseStock> WarehouseStocks { get; }
    DbSet<StockMovement> StockMovements { get; }
    DbSet<StockTransfer> StockTransfers { get; }
    DbSet<StockTransferItem> StockTransferItems { get; }
    DbSet<StockAdjustment> StockAdjustments { get; }
    DbSet<StockAdjustmentItem> StockAdjustmentItems { get; }
    DbSet<Customer> Customers { get; }
    DbSet<PurchaseOrder> PurchaseOrders { get; }
    DbSet<PurchaseOrderItem> PurchaseOrderItems { get; }
    DbSet<GoodsReceipt> GoodsReceipts { get; }
    DbSet<GoodsReceiptItem> GoodsReceiptItems { get; }
    DbSet<SalesOrder> SalesOrders { get; }
    DbSet<SalesOrderItem> SalesOrderItems { get; }
    DbSet<SalesInvoice> SalesInvoices { get; }
    DbSet<SalesInvoiceItem> SalesInvoiceItems { get; }
    DbSet<Delivery> Deliveries { get; }
    DbSet<DeliveryItem> DeliveryItems { get; }
    DbSet<SalesReturn> SalesReturns { get; }
    DbSet<SalesReturnItem> SalesReturnItems { get; }
    DbSet<Treasury> Treasuries { get; }
    DbSet<TreasuryTransaction> TreasuryTransactions { get; }
    DbSet<SupplierPayment> SupplierPayments { get; }
    DbSet<CustomerPayment> CustomerPayments { get; }
    DbSet<OperatingExpense> OperatingExpenses { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
