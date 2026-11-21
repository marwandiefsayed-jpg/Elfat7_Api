using Elfat7.Application.DTOs.Sales;
using Elfat7.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Elfat7.Api.Controllers;

[ApiController]
[Route("api/sales-invoices")]
public class SalesInvoicesController : ControllerBase
{
    private readonly ISalesInvoiceService _invoiceService;
    private readonly IPdfReportService _pdfService;

    public SalesInvoicesController(ISalesInvoiceService invoiceService, IPdfReportService pdfService)
    {
        _invoiceService = invoiceService;
        _pdfService = pdfService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSalesInvoices([FromQuery] SalesInvoiceFilterDto filter, CancellationToken ct)
    {
        var result = await _invoiceService.GetSalesInvoicesAsync(filter, ct);
        return Ok(result);
    }

    [HttpGet("{invoiceNumber}")]
    public async Task<IActionResult> GetSalesInvoice(string invoiceNumber, CancellationToken ct)
    {
        var inv = await _invoiceService.GetSalesInvoiceByNumberAsync(invoiceNumber, ct);
        if (inv == null) return NotFound(new { message = "فاتورة المبيعات غير موجودة" });
        return Ok(inv);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSalesInvoice([FromBody] CreateSalesInvoiceDto dto, CancellationToken ct)
    {
        var result = await _invoiceService.CreateSalesInvoiceAsync(dto, ct);
        if (!result.Succeeded) return BadRequest(result);
        return CreatedAtAction(nameof(GetSalesInvoice), new { invoiceNumber = result.Data!.InvoiceNumber }, result);
    }

    [HttpPost("{invoiceNumber}/confirm")]
    public async Task<IActionResult> ConfirmSalesInvoice(string invoiceNumber, CancellationToken ct)
    {
        var result = await _invoiceService.ConfirmSalesInvoiceAsync(invoiceNumber, ct);
        if (!result.Succeeded) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("{invoiceNumber}/cancel")]
    public async Task<IActionResult> CancelSalesInvoice(string invoiceNumber, CancellationToken ct)
    {
        var result = await _invoiceService.CancelSalesInvoiceAsync(invoiceNumber, ct);
        if (!result.Succeeded) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportPdf([FromQuery] SalesInvoiceFilterDto filter, CancellationToken ct)
    {
        var invoices = await _invoiceService.GetAllFilteredSalesInvoicesAsync(filter, ct);
        var pdfBytes = _pdfService.GenerateSalesInvoicesPdf(invoices, filter);
        var fileName = $"SalesInvoices_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    [HttpGet("{invoiceNumber}/export/pdf")]
    public async Task<IActionResult> ExportInvoicePdf(string invoiceNumber, CancellationToken ct)
    {
        var inv = await _invoiceService.GetSalesInvoiceByNumberAsync(invoiceNumber, ct);
        if (inv == null) return NotFound(new { message = "فاتورة المبيعات غير موجودة" });
        var pdfBytes = _pdfService.GenerateSalesInvoiceDetailsPdf(inv);
        var fileName = $"Invoice_{invoiceNumber}_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }
}
