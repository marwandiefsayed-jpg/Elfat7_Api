using Elfat7.Application.DTOs.Deliveries;
using Elfat7.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Elfat7.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeliveriesController : ControllerBase
{
    private readonly IDeliveryService _deliveryService;
    private readonly IPdfReportService _pdfService;

    public DeliveriesController(IDeliveryService deliveryService, IPdfReportService pdfService)
    {
        _deliveryService = deliveryService;
        _pdfService = pdfService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDeliveries([FromQuery] DeliveryFilterDto filter, CancellationToken ct)
    {
        var result = await _deliveryService.GetDeliveriesAsync(filter, ct);
        return Ok(result);
    }

    [HttpGet("{deliveryNumber}")]
    public async Task<IActionResult> GetDelivery(string deliveryNumber, CancellationToken ct)
    {
        var del = await _deliveryService.GetDeliveryByNumberAsync(deliveryNumber, ct);
        if (del == null) return NotFound(new { message = "إذن التسليم المطلوب غير موجود" });
        return Ok(del);
    }

    [HttpPost]
    public async Task<IActionResult> CreateDelivery([FromBody] CreateDeliveryDto dto, CancellationToken ct)
    {
        var result = await _deliveryService.CreateDeliveryAsync(dto, ct);
        if (!result.Succeeded) return BadRequest(result);
        return CreatedAtAction(nameof(GetDelivery), new { deliveryNumber = result.Data!.DeliveryNumber }, result);
    }

    [HttpPost("{deliveryNumber}/deliver")]
    public async Task<IActionResult> MarkAsDelivered(string deliveryNumber, CancellationToken ct)
    {
        var result = await _deliveryService.MarkAsDeliveredAsync(deliveryNumber, ct);
        if (!result.Succeeded) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportPdf([FromQuery] DeliveryFilterDto filter, CancellationToken ct)
    {
        var deliveries = await _deliveryService.GetAllFilteredDeliveriesAsync(filter, ct);
        var pdfBytes = _pdfService.GenerateDeliveriesPdf(deliveries, filter);
        var fileName = $"Deliveries_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    [HttpGet("{deliveryNumber}/export/pdf")]
    public async Task<IActionResult> ExportDeliveryNotePdf(string deliveryNumber, CancellationToken ct)
    {
        var del = await _deliveryService.GetDeliveryByNumberAsync(deliveryNumber, ct);
        if (del == null) return NotFound(new { message = "إذن التسليم المطلوب غير موجود" });
        var pdfBytes = _pdfService.GenerateDeliveryDetailsPdf(del);
        var fileName = $"DeliveryNote_{deliveryNumber}_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }
}
