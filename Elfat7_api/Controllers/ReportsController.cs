using Elfat7.Application.DTOs.Reports;
using Elfat7.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Elfat7.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly IPdfReportService _pdfService;

    public ReportsController(IReportService reportService, IPdfReportService pdfService)
    {
        _reportService = reportService;
        _pdfService = pdfService;
    }

    // 1. Purchases Report
    [HttpGet("purchases")]
    public async Task<IActionResult> GetPurchasesReport([FromQuery] PurchasesReportFilterDto filter, CancellationToken ct)
    {
        var report = await _reportService.GetPurchasesReportAsync(filter, ct);
        return Ok(report);
    }

    [HttpGet("purchases/export/pdf")]
    public async Task<IActionResult> ExportPurchasesReportPdf([FromQuery] PurchasesReportFilterDto filter, CancellationToken ct)
    {
        var report = await _reportService.GetPurchasesReportAsync(filter, ct);
        var pdfBytes = _pdfService.GeneratePurchasesReportPdf(report);
        var fileName = $"PurchasesReport_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    // 2. Sales Report
    [HttpGet("sales")]
    public async Task<IActionResult> GetSalesReport([FromQuery] SalesReportFilterDto filter, CancellationToken ct)
    {
        var report = await _reportService.GetSalesReportAsync(filter, ct);
        return Ok(report);
    }

    [HttpGet("sales/export/pdf")]
    public async Task<IActionResult> ExportSalesReportPdf([FromQuery] SalesReportFilterDto filter, CancellationToken ct)
    {
        var report = await _reportService.GetSalesReportAsync(filter, ct);
        var pdfBytes = _pdfService.GenerateSalesReportPdf(report);
        var fileName = $"SalesReport_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    // 3. Inventory Valuation Report
    [HttpGet("inventory")]
    public async Task<IActionResult> GetInventoryReport([FromQuery] InventoryReportFilterDto filter, CancellationToken ct)
    {
        var report = await _reportService.GetInventoryReportAsync(filter, ct);
        return Ok(report);
    }

    [HttpGet("inventory/export/pdf")]
    public async Task<IActionResult> ExportInventoryReportPdf([FromQuery] InventoryReportFilterDto filter, CancellationToken ct)
    {
        var report = await _reportService.GetInventoryReportAsync(filter, ct);
        var pdfBytes = _pdfService.GenerateInventoryReportPdf(report);
        var fileName = $"InventoryReport_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    // 4. Warehouse Stock Report
    [HttpGet("warehouse-stock")]
    public async Task<IActionResult> GetWarehouseStockReport([FromQuery] WarehouseStockReportFilterDto filter, CancellationToken ct)
    {
        var report = await _reportService.GetWarehouseStockReportAsync(filter, ct);
        return Ok(report);
    }

    [HttpGet("warehouse-stock/export/pdf")]
    public async Task<IActionResult> ExportWarehouseStockReportPdf([FromQuery] WarehouseStockReportFilterDto filter, CancellationToken ct)
    {
        var report = await _reportService.GetWarehouseStockReportAsync(filter, ct);
        var pdfBytes = _pdfService.GenerateWarehouseStockReportPdf(report);
        var fileName = $"WarehouseStockReport_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    // 5. Supplier Balances Report
    [HttpGet("supplier-balances")]
    public async Task<IActionResult> GetSupplierBalancesReport([FromQuery] SupplierBalancesReportFilterDto filter, CancellationToken ct)
    {
        var report = await _reportService.GetSupplierBalancesReportAsync(filter, ct);
        return Ok(report);
    }

    [HttpGet("supplier-balances/export/pdf")]
    public async Task<IActionResult> ExportSupplierBalancesReportPdf([FromQuery] SupplierBalancesReportFilterDto filter, CancellationToken ct)
    {
        var report = await _reportService.GetSupplierBalancesReportAsync(filter, ct);
        var pdfBytes = _pdfService.GenerateSupplierBalancesReportPdf(report);
        var fileName = $"SupplierBalancesReport_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    // 6. Customer Balances Report
    [HttpGet("customer-balances")]
    public async Task<IActionResult> GetCustomerBalancesReport([FromQuery] CustomerBalancesReportFilterDto filter, CancellationToken ct)
    {
        var report = await _reportService.GetCustomerBalancesReportAsync(filter, ct);
        return Ok(report);
    }

    [HttpGet("customer-balances/export/pdf")]
    public async Task<IActionResult> ExportCustomerBalancesReportPdf([FromQuery] CustomerBalancesReportFilterDto filter, CancellationToken ct)
    {
        var report = await _reportService.GetCustomerBalancesReportAsync(filter, ct);
        var pdfBytes = _pdfService.GenerateCustomerBalancesReportPdf(report);
        var fileName = $"CustomerBalancesReport_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    // 7. Expenses Report
    [HttpGet("expenses")]
    public async Task<IActionResult> GetExpensesReport([FromQuery] ExpensesReportFilterDto filter, CancellationToken ct)
    {
        var report = await _reportService.GetExpensesReportAsync(filter, ct);
        return Ok(report);
    }

    [HttpGet("expenses/export/pdf")]
    public async Task<IActionResult> ExportExpensesReportPdf([FromQuery] ExpensesReportFilterDto filter, CancellationToken ct)
    {
        var report = await _reportService.GetExpensesReportAsync(filter, ct);
        var pdfBytes = _pdfService.GenerateExpensesReportPdf(report);
        var fileName = $"ExpensesReport_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    // 8. Profit and Loss Report
    [HttpGet("profit-loss")]
    public async Task<IActionResult> GetProfitLossReport([FromQuery] ProfitLossReportFilterDto filter, CancellationToken ct)
    {
        var report = await _reportService.GetProfitLossReportAsync(filter, ct);
        return Ok(report);
    }

    [HttpGet("profit-loss/export/pdf")]
    public async Task<IActionResult> ExportProfitLossReportPdf([FromQuery] ProfitLossReportFilterDto filter, CancellationToken ct)
    {
        var report = await _reportService.GetProfitLossReportAsync(filter, ct);
        var pdfBytes = _pdfService.GenerateProfitLossReportPdf(report);
        var fileName = $"ProfitLossReport_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }
}
