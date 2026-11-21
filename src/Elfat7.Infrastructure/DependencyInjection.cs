using Elfat7.Application.Interfaces;
using Elfat7.Infrastructure.Pdf;
using Elfat7.Infrastructure.Persistence;
using Elfat7.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Elfat7.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=(localdb)\\mssqllocaldb;Database=Elfat7AgriDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        services.AddDbContext<Elfat7DbContext>(options =>
            options.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(Elfat7DbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<Elfat7DbContext>());

        // Register Business Services
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IProductCategoryService, ProductCategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
        services.AddScoped<IGoodsReceiptService, GoodsReceiptService>();
        services.AddScoped<ISalesOrderService, SalesOrderService>();
        services.AddScoped<ISalesInvoiceService, SalesInvoiceService>();
        services.AddScoped<IDeliveryService, DeliveryService>();
        services.AddScoped<ISalesReturnService, SalesReturnService>();
        services.AddScoped<IFinancialService, FinancialService>();
        services.AddScoped<IReportService, ReportService>();

        // Register PDF Service
        services.AddSingleton<IPdfReportService, PdfReportService>();

        return services;
    }
}
