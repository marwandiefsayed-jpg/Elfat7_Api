using Elfat7.Application.Common;
using Elfat7.Application.DTOs.Inventory;
using Elfat7.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Elfat7.Api.Controllers;

[ApiController]
[Route("api/stock-transfers")]
public class StockTransfersController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly IPdfReportService _pdfService;

    public StockTransfersController(IInventoryService inventoryService, IPdfReportService pdfService)
    {
        _inventoryService = inventoryService;
        _pdfService = pdfService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTransfers([FromQuery] PagedFilterRequest filter, CancellationToken ct)
    {
        var result = await _inventoryService.GetStockTransfersAsync(filter, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTransfer([FromBody] CreateStockTransferDto dto, CancellationToken ct)
    {
        var result = await _inventoryService.CreateStockTransferAsync(dto, ct);
        if (!result.Succeeded) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportPdf([FromQuery] PagedFilterRequest filter, CancellationToken ct)
    {
        var transfers = await _inventoryService.GetAllFilteredStockTransfersAsync(filter, ct);
        var pdfBytes = _pdfService.GenerateStockTransfersPdf(transfers);
        var fileName = $"StockTransfers_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }
}
