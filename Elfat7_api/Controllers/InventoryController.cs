using Elfat7.Application.DTOs.Inventory;
using Elfat7.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Elfat7.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly IPdfReportService _pdfService;

    public InventoryController(IInventoryService inventoryService, IPdfReportService pdfService)
    {
        _inventoryService = inventoryService;
        _pdfService = pdfService;
    }

    [HttpGet]
    public async Task<IActionResult> GetInventory([FromQuery] InventoryFilterDto filter, CancellationToken ct)
    {
        var result = await _inventoryService.GetInventoryAsync(filter, ct);
        return Ok(result);
    }

    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportInventoryPdf([FromQuery] InventoryFilterDto filter, CancellationToken ct)
    {
        var inventory = await _inventoryService.GetAllFilteredInventoryAsync(filter, ct);
        var pdfBytes = _pdfService.GenerateInventoryPdf(inventory, filter);
        var fileName = $"Inventory_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    [HttpGet("movements")]
    public async Task<IActionResult> GetStockMovements([FromQuery] StockMovementFilterDto filter, CancellationToken ct)
    {
        var result = await _inventoryService.GetStockMovementsAsync(filter, ct);
        return Ok(result);
    }

    [HttpGet("movements/export/pdf")]
    public async Task<IActionResult> ExportStockMovementsPdf([FromQuery] StockMovementFilterDto filter, CancellationToken ct)
    {
        var movements = await _inventoryService.GetAllFilteredStockMovementsAsync(filter, ct);
        var pdfBytes = _pdfService.GenerateStockMovementsPdf(movements, filter);
        var fileName = $"StockMovements_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    [HttpPost("update-stock")]
    public async Task<IActionResult> UpdateStockQuantity([FromBody] UpdateStockQuantityDto dto, CancellationToken ct)
    {
        var result = await _inventoryService.UpdateStockQuantityAsync(dto, ct);
        if (!result.Succeeded) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("add-stock")]
    [HttpPost("incoming-stock")]
    public async Task<IActionResult> AddIncomingStock([FromBody] AddIncomingStockDto dto, CancellationToken ct)
    {
        var result = await _inventoryService.AddIncomingStockAsync(dto, ct);
        if (!result.Succeeded) return BadRequest(result);
        return Ok(result);
    }
}
