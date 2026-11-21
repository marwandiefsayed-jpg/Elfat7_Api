using Elfat7.Application.Common;
using Elfat7.Application.DTOs.Customers;
using Elfat7.Application.DTOs.Deliveries;
using Elfat7.Application.DTOs.Financial;
using Elfat7.Application.DTOs.GoodsReceiving;
using Elfat7.Application.DTOs.Inventory;
using Elfat7.Application.DTOs.Products;
using Elfat7.Application.DTOs.Purchasing;
using Elfat7.Application.DTOs.Reports;
using Elfat7.Application.DTOs.Sales;
using Elfat7.Application.DTOs.SalesReturns;
using Elfat7.Application.DTOs.Suppliers;
using Elfat7.Application.DTOs.Warehouses;

namespace Elfat7.Application.Interfaces;

public interface ISupplierService
{
    Task<PagedResult<SupplierDto>> GetSuppliersAsync(SupplierFilterDto filter, CancellationToken ct = default);
    Task<List<SupplierDto>> GetAllFilteredSuppliersAsync(SupplierFilterDto filter, CancellationToken ct = default);
    Task<SupplierDetailsDto?> GetSupplierByCodeAsync(string code, CancellationToken ct = default);
    Task<Result<SupplierDto>> CreateSupplierAsync(CreateSupplierDto dto, CancellationToken ct = default);
    Task<Result<SupplierDto>> UpdateSupplierAsync(string code, UpdateSupplierDto dto, CancellationToken ct = default);
    Task<Result> DeleteSupplierAsync(string code, CancellationToken ct = default);
}

public interface IProductCategoryService
{
    Task<List<ProductCategoryDto>> GetCategoriesAsync(CancellationToken ct = default);
    Task<ProductCategoryDto?> GetCategoryByCodeAsync(string code, CancellationToken ct = default);
    Task<Result<ProductCategoryDto>> CreateCategoryAsync(CreateProductCategoryDto dto, CancellationToken ct = default);
    Task<Result<ProductCategoryDto>> UpdateCategoryAsync(string code, UpdateProductCategoryDto dto, CancellationToken ct = default);
}

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetProductsAsync(ProductFilterDto filter, CancellationToken ct = default);
    Task<List<ProductDto>> GetAllFilteredProductsAsync(ProductFilterDto filter, CancellationToken ct = default);
    Task<ProductDetailsDto?> GetProductByCodeAsync(string code, CancellationToken ct = default);
    Task<Result<ProductDto>> CreateProductAsync(CreateProductDto dto, CancellationToken ct = default);
    Task<Result<ProductDto>> UpdateProductAsync(string code, UpdateProductDto dto, CancellationToken ct = default);
    Task<Result> DeleteProductAsync(string code, CancellationToken ct = default);
}

public interface IWarehouseService
{
    Task<PagedResult<WarehouseDto>> GetWarehousesAsync(WarehouseFilterDto filter, CancellationToken ct = default);
    Task<List<WarehouseDto>> GetAllFilteredWarehousesAsync(WarehouseFilterDto filter, CancellationToken ct = default);
    Task<WarehouseDetailsDto?> GetWarehouseByCodeAsync(string code, CancellationToken ct = default);
    Task<Result<WarehouseDto>> CreateWarehouseAsync(CreateWarehouseDto dto, CancellationToken ct = default);
    Task<Result<WarehouseDto>> UpdateWarehouseAsync(string code, UpdateWarehouseDto dto, CancellationToken ct = default);
}

public interface IInventoryService
{
    Task<PagedResult<InventoryItemDto>> GetInventoryAsync(InventoryFilterDto filter, CancellationToken ct = default);
    Task<List<InventoryItemDto>> GetAllFilteredInventoryAsync(InventoryFilterDto filter, CancellationToken ct = default);
    Task<PagedResult<StockMovementDto>> GetStockMovementsAsync(StockMovementFilterDto filter, CancellationToken ct = default);
    Task<List<StockMovementDto>> GetAllFilteredStockMovementsAsync(StockMovementFilterDto filter, CancellationToken ct = default);
    Task<PagedResult<StockTransferDto>> GetStockTransfersAsync(PagedFilterRequest filter, CancellationToken ct = default);
    Task<List<StockTransferDto>> GetAllFilteredStockTransfersAsync(PagedFilterRequest filter, CancellationToken ct = default);
    Task<Result<StockTransferDto>> CreateStockTransferAsync(CreateStockTransferDto dto, CancellationToken ct = default);
    Task<PagedResult<StockAdjustmentDto>> GetStockAdjustmentsAsync(PagedFilterRequest filter, CancellationToken ct = default);
    Task<Result<StockAdjustmentDto>> CreateStockAdjustmentAsync(CreateStockAdjustmentDto dto, CancellationToken ct = default);
}

public interface ICustomerService
{
    Task<PagedResult<CustomerDto>> GetCustomersAsync(CustomerFilterDto filter, CancellationToken ct = default);
    Task<List<CustomerDto>> GetAllFilteredCustomersAsync(CustomerFilterDto filter, CancellationToken ct = default);
    Task<CustomerDetailsDto?> GetCustomerByCodeAsync(string code, CancellationToken ct = default);
    Task<Result<CustomerDto>> CreateCustomerAsync(CreateCustomerDto dto, CancellationToken ct = default);
    Task<Result<CustomerDto>> UpdateCustomerAsync(string code, UpdateCustomerDto dto, CancellationToken ct = default);
    Task<Result> DeleteCustomerAsync(string code, CancellationToken ct = default);
}

public interface IPurchaseOrderService
{
    Task<PagedResult<PurchaseOrderDto>> GetPurchaseOrdersAsync(PurchaseOrderFilterDto filter, CancellationToken ct = default);
    Task<List<PurchaseOrderDto>> GetAllFilteredPurchaseOrdersAsync(PurchaseOrderFilterDto filter, CancellationToken ct = default);
    Task<PurchaseOrderDetailsDto?> GetPurchaseOrderByNumberAsync(string orderNumber, CancellationToken ct = default);
    Task<Result<PurchaseOrderDto>> CreatePurchaseOrderAsync(CreatePurchaseOrderDto dto, CancellationToken ct = default);
    Task<Result> ConfirmPurchaseOrderAsync(string orderNumber, CancellationToken ct = default);
    Task<Result> CancelPurchaseOrderAsync(string orderNumber, CancellationToken ct = default);
}

public interface IGoodsReceiptService
{
    Task<PagedResult<GoodsReceiptDto>> GetGoodsReceiptsAsync(GoodsReceiptFilterDto filter, CancellationToken ct = default);
    Task<List<GoodsReceiptDto>> GetAllFilteredGoodsReceiptsAsync(GoodsReceiptFilterDto filter, CancellationToken ct = default);
    Task<GoodsReceiptDetailsDto?> GetGoodsReceiptByNumberAsync(string receiptNumber, CancellationToken ct = default);
    Task<Result<GoodsReceiptDto>> CreateGoodsReceiptAsync(CreateGoodsReceiptDto dto, CancellationToken ct = default);
}

public interface ISalesOrderService
{
    Task<PagedResult<SalesOrderDto>> GetSalesOrdersAsync(SalesOrderFilterDto filter, CancellationToken ct = default);
    Task<List<SalesOrderDto>> GetAllFilteredSalesOrdersAsync(SalesOrderFilterDto filter, CancellationToken ct = default);
    Task<SalesOrderDetailsDto?> GetSalesOrderByNumberAsync(string orderNumber, CancellationToken ct = default);
    Task<Result<SalesOrderDto>> CreateSalesOrderAsync(CreateSalesOrderDto dto, CancellationToken ct = default);
    Task<Result> ConfirmSalesOrderAsync(string orderNumber, CancellationToken ct = default);
    Task<Result> CancelSalesOrderAsync(string orderNumber, CancellationToken ct = default);
}

public interface ISalesInvoiceService
{
    Task<PagedResult<SalesInvoiceDto>> GetSalesInvoicesAsync(SalesInvoiceFilterDto filter, CancellationToken ct = default);
    Task<List<SalesInvoiceDto>> GetAllFilteredSalesInvoicesAsync(SalesInvoiceFilterDto filter, CancellationToken ct = default);
    Task<SalesInvoiceDetailsDto?> GetSalesInvoiceByNumberAsync(string invoiceNumber, CancellationToken ct = default);
    Task<Result<SalesInvoiceDto>> CreateSalesInvoiceAsync(CreateSalesInvoiceDto dto, CancellationToken ct = default);
    Task<Result> ConfirmSalesInvoiceAsync(string invoiceNumber, CancellationToken ct = default);
    Task<Result> CancelSalesInvoiceAsync(string invoiceNumber, CancellationToken ct = default);
}

public interface IDeliveryService
{
    Task<PagedResult<DeliveryDto>> GetDeliveriesAsync(DeliveryFilterDto filter, CancellationToken ct = default);
    Task<List<DeliveryDto>> GetAllFilteredDeliveriesAsync(DeliveryFilterDto filter, CancellationToken ct = default);
    Task<DeliveryDetailsDto?> GetDeliveryByNumberAsync(string deliveryNumber, CancellationToken ct = default);
    Task<Result<DeliveryDto>> CreateDeliveryAsync(CreateDeliveryDto dto, CancellationToken ct = default);
    Task<Result> MarkAsDeliveredAsync(string deliveryNumber, CancellationToken ct = default);
}

public interface ISalesReturnService
{
    Task<PagedResult<SalesReturnDto>> GetSalesReturnsAsync(SalesReturnFilterDto filter, CancellationToken ct = default);
    Task<List<SalesReturnDto>> GetAllFilteredSalesReturnsAsync(SalesReturnFilterDto filter, CancellationToken ct = default);
    Task<SalesReturnDto?> GetSalesReturnByNumberAsync(string returnNumber, CancellationToken ct = default);
    Task<Result<SalesReturnDto>> CreateSalesReturnAsync(CreateSalesReturnDto dto, CancellationToken ct = default);
}

public interface IFinancialService
{
    // Treasury
    Task<List<TreasuryDto>> GetTreasuriesAsync(CancellationToken ct = default);
    Task<Result<TreasuryDto>> CreateTreasuryAsync(CreateTreasuryDto dto, CancellationToken ct = default);
    Task<PagedResult<TreasuryTransactionDto>> GetTreasuryTransactionsAsync(TreasuryFilterDto filter, CancellationToken ct = default);
    Task<List<TreasuryTransactionDto>> GetAllFilteredTreasuryTransactionsAsync(TreasuryFilterDto filter, CancellationToken ct = default);
    
    // Supplier Payments
    Task<PagedResult<SupplierPaymentDto>> GetSupplierPaymentsAsync(PaymentFilterDto filter, CancellationToken ct = default);
    Task<List<SupplierPaymentDto>> GetAllFilteredSupplierPaymentsAsync(PaymentFilterDto filter, CancellationToken ct = default);
    Task<Result<SupplierPaymentDto>> CreateSupplierPaymentAsync(CreateSupplierPaymentDto dto, CancellationToken ct = default);
    
    // Customer Payments
    Task<PagedResult<CustomerPaymentDto>> GetCustomerPaymentsAsync(PaymentFilterDto filter, CancellationToken ct = default);
    Task<List<CustomerPaymentDto>> GetAllFilteredCustomerPaymentsAsync(PaymentFilterDto filter, CancellationToken ct = default);
    Task<Result<CustomerPaymentDto>> CreateCustomerPaymentAsync(CreateCustomerPaymentDto dto, CancellationToken ct = default);
    
    // Operating Expenses
    Task<PagedResult<OperatingExpenseDto>> GetExpensesAsync(ExpenseFilterDto filter, CancellationToken ct = default);
    Task<List<OperatingExpenseDto>> GetAllFilteredExpensesAsync(ExpenseFilterDto filter, CancellationToken ct = default);
    Task<Result<OperatingExpenseDto>> CreateExpenseAsync(CreateOperatingExpenseDto dto, CancellationToken ct = default);
}

public interface IReportService
{
    Task<PurchasesReportDto> GetPurchasesReportAsync(PurchasesReportFilterDto filter, CancellationToken ct = default);
    Task<SalesReportDto> GetSalesReportAsync(SalesReportFilterDto filter, CancellationToken ct = default);
    Task<InventoryReportDto> GetInventoryReportAsync(InventoryReportFilterDto filter, CancellationToken ct = default);
    Task<WarehouseStockReportDto> GetWarehouseStockReportAsync(WarehouseStockReportFilterDto filter, CancellationToken ct = default);
    Task<SupplierBalancesReportDto> GetSupplierBalancesReportAsync(SupplierBalancesReportFilterDto filter, CancellationToken ct = default);
    Task<CustomerBalancesReportDto> GetCustomerBalancesReportAsync(CustomerBalancesReportFilterDto filter, CancellationToken ct = default);
    Task<ExpensesReportDto> GetExpensesReportAsync(ExpensesReportFilterDto filter, CancellationToken ct = default);
    Task<ProfitLossReportDto> GetProfitLossReportAsync(ProfitLossReportFilterDto filter, CancellationToken ct = default);
}

public interface IPdfReportService
{
    byte[] GenerateSuppliersPdf(List<SupplierDto> suppliers, SupplierFilterDto? filter = null);
    byte[] GenerateProductsPdf(List<ProductDto> products, ProductFilterDto? filter = null);
    byte[] GenerateProductCategoriesPdf(List<ProductCategoryDto> categories);
    byte[] GenerateWarehousesPdf(List<WarehouseDto> warehouses, WarehouseFilterDto? filter = null);
    byte[] GenerateInventoryPdf(List<InventoryItemDto> inventory, InventoryFilterDto? filter = null);
    byte[] GenerateStockMovementsPdf(List<StockMovementDto> movements, StockMovementFilterDto? filter = null);
    byte[] GenerateStockTransfersPdf(List<StockTransferDto> transfers);
    byte[] GenerateCustomersPdf(List<CustomerDto> customers, CustomerFilterDto? filter = null);
    byte[] GeneratePurchaseOrdersPdf(List<PurchaseOrderDto> orders, PurchaseOrderFilterDto? filter = null);
    byte[] GeneratePurchaseOrderDetailsPdf(PurchaseOrderDetailsDto order);
    byte[] GenerateGoodsReceiptsPdf(List<GoodsReceiptDto> receipts, GoodsReceiptFilterDto? filter = null);
    byte[] GenerateGoodsReceiptDetailsPdf(GoodsReceiptDetailsDto receipt);
    byte[] GenerateSalesOrdersPdf(List<SalesOrderDto> orders, SalesOrderFilterDto? filter = null);
    byte[] GenerateSalesInvoicesPdf(List<SalesInvoiceDto> invoices, SalesInvoiceFilterDto? filter = null);
    byte[] GenerateSalesInvoiceDetailsPdf(SalesInvoiceDetailsDto invoice);
    byte[] GenerateDeliveriesPdf(List<DeliveryDto> deliveries, DeliveryFilterDto? filter = null);
    byte[] GenerateDeliveryDetailsPdf(DeliveryDetailsDto delivery);
    byte[] GenerateSalesReturnsPdf(List<SalesReturnDto> returns, SalesReturnFilterDto? filter = null);
    byte[] GenerateSupplierPaymentsPdf(List<SupplierPaymentDto> payments, PaymentFilterDto? filter = null);
    byte[] GenerateCustomerPaymentsPdf(List<CustomerPaymentDto> payments, PaymentFilterDto? filter = null);
    byte[] GenerateTreasuryTransactionsPdf(List<TreasuryTransactionDto> transactions, TreasuryFilterDto? filter = null);
    byte[] GenerateExpensesPdf(List<OperatingExpenseDto> expenses, ExpenseFilterDto? filter = null);
    byte[] GeneratePurchasesReportPdf(PurchasesReportDto report);
    byte[] GenerateSalesReportPdf(SalesReportDto report);
    byte[] GenerateInventoryReportPdf(InventoryReportDto report);
    byte[] GenerateWarehouseStockReportPdf(WarehouseStockReportDto report);
    byte[] GenerateSupplierBalancesReportPdf(SupplierBalancesReportDto report);
    byte[] GenerateCustomerBalancesReportPdf(CustomerBalancesReportDto report);
    byte[] GenerateExpensesReportPdf(ExpensesReportDto report);
    byte[] GenerateProfitLossReportPdf(ProfitLossReportDto report);
}
