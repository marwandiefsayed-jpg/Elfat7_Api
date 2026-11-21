using Elfat7.Application.DTOs.Purchasing;
using Elfat7.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Elfat7.Api.Controllers;

[ApiController]
[Route("api/purchase-orders")]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IPurchaseOrderService _poService;
    private readonly IPdfReportService _pdfService;

    public PurchaseOrdersController(IPurchaseOrderService poService, IPdfReportService pdfService)
    {
        _poService = poService;
        _pdfService = pdfService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPurchaseOrders([FromQuery] PurchaseOrderFilterDto filter, CancellationToken ct)
    {
        var result = await _poService.GetPurchaseOrdersAsync(filter, ct);
        return Ok(result);
    }

    [HttpGet("{orderNumber}")]
    public async Task<IActionResult> GetPurchaseOrder(string orderNumber, CancellationToken ct)
    {
        var po = await _poService.GetPurchaseOrderByNumberAsync(orderNumber, ct);
        if (po == null) return NotFound(new { message = "أمر الشراء المطلوب غير موجود" });
        return Ok(po);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePurchaseOrder([FromBody] CreatePurchaseOrderDto dto, CancellationToken ct)
    {
        var result = await _poService.CreatePurchaseOrderAsync(dto, ct);
        if (!result.Succeeded) return BadRequest(result);
        return CreatedAtAction(nameof(GetPurchaseOrder), new { orderNumber = result.Data!.OrderNumber }, result);
    }

    [HttpPost("{orderNumber}/confirm")]
    public async Task<IActionResult> ConfirmPurchaseOrder(string orderNumber, CancellationToken ct)
    {
        var result = await _poService.ConfirmPurchaseOrderAsync(orderNumber, ct);
        if (!result.Succeeded) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("{orderNumber}/cancel")]
    public async Task<IActionResult> CancelPurchaseOrder(string orderNumber, CancellationToken ct)
    {
        var result = await _poService.CancelPurchaseOrderAsync(orderNumber, ct);
        if (!result.Succeeded) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportPdf([FromQuery] PurchaseOrderFilterDto filter, CancellationToken ct)
    {
        var orders = await _poService.GetAllFilteredPurchaseOrdersAsync(filter, ct);
        var pdfBytes = _pdfService.GeneratePurchaseOrdersPdf(orders, filter);
        var fileName = $"PurchaseOrders_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    [HttpGet("{orderNumber}/export/pdf")]
    public async Task<IActionResult> ExportOrderPdf(string orderNumber, CancellationToken ct)
    {
        var po = await _poService.GetPurchaseOrderByNumberAsync(orderNumber, ct);
        if (po == null) return NotFound(new { message = "أمر الشراء المطلوب غير موجود" });
        var pdfBytes = _pdfService.GeneratePurchaseOrderDetailsPdf(po);
        var fileName = $"PO_{orderNumber}_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }
}
