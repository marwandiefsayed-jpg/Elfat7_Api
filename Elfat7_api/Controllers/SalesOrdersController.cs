using Elfat7.Application.DTOs.Sales;
using Elfat7.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Elfat7.Api.Controllers;

[ApiController]
[Route("api/sales-orders")]
public class SalesOrdersController : ControllerBase
{
    private readonly ISalesOrderService _salesOrderService;
    private readonly IPdfReportService _pdfService;

    public SalesOrdersController(ISalesOrderService salesOrderService, IPdfReportService pdfService)
    {
        _salesOrderService = salesOrderService;
        _pdfService = pdfService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSalesOrders([FromQuery] SalesOrderFilterDto filter, CancellationToken ct)
    {
        var result = await _salesOrderService.GetSalesOrdersAsync(filter, ct);
        return Ok(result);
    }

    [HttpGet("{orderNumber}")]
    public async Task<IActionResult> GetSalesOrder(string orderNumber, CancellationToken ct)
    {
        var so = await _salesOrderService.GetSalesOrderByNumberAsync(orderNumber, ct);
        if (so == null) return NotFound(new { message = "طلب البيع المطلوب غير موجود" });
        return Ok(so);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSalesOrder([FromBody] CreateSalesOrderDto dto, CancellationToken ct)
    {
        var result = await _salesOrderService.CreateSalesOrderAsync(dto, ct);
        if (!result.Succeeded) return BadRequest(result);
        return CreatedAtAction(nameof(GetSalesOrder), new { orderNumber = result.Data!.OrderNumber }, result);
    }

    [HttpPost("{orderNumber}/confirm")]
    public async Task<IActionResult> ConfirmSalesOrder(string orderNumber, CancellationToken ct)
    {
        var result = await _salesOrderService.ConfirmSalesOrderAsync(orderNumber, ct);
        if (!result.Succeeded) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("{orderNumber}/cancel")]
    public async Task<IActionResult> CancelSalesOrder(string orderNumber, CancellationToken ct)
    {
        var result = await _salesOrderService.CancelSalesOrderAsync(orderNumber, ct);
        if (!result.Succeeded) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportPdf([FromQuery] SalesOrderFilterDto filter, CancellationToken ct)
    {
        var orders = await _salesOrderService.GetAllFilteredSalesOrdersAsync(filter, ct);
        var pdfBytes = _pdfService.GenerateSalesOrdersPdf(orders, filter);
        var fileName = $"SalesOrders_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }
}
