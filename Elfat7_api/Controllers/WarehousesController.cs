using Elfat7.Application.DTOs.Warehouses;
using Elfat7.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Elfat7.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehousesController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;
    private readonly IPdfReportService _pdfService;

    public WarehousesController(IWarehouseService warehouseService, IPdfReportService pdfService)
    {
        _warehouseService = warehouseService;
        _pdfService = pdfService;
    }

    [HttpGet]
    public async Task<IActionResult> GetWarehouses([FromQuery] WarehouseFilterDto filter, CancellationToken ct)
    {
        var result = await _warehouseService.GetWarehousesAsync(filter, ct);
        return Ok(result);
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetWarehouse(string code, CancellationToken ct)
    {
        var warehouse = await _warehouseService.GetWarehouseByCodeAsync(code, ct);
        if (warehouse == null) return NotFound(new { message = "المخزن المطلوب غير موجود" });
        return Ok(warehouse);
    }

    [HttpPost]
    public async Task<IActionResult> CreateWarehouse([FromBody] CreateWarehouseDto dto, CancellationToken ct)
    {
        var result = await _warehouseService.CreateWarehouseAsync(dto, ct);
        if (!result.Succeeded) return BadRequest(result);
        return CreatedAtAction(nameof(GetWarehouse), new { code = result.Data!.Code }, result);
    }

    [HttpPut("{code}")]
    public async Task<IActionResult> UpdateWarehouse(string code, [FromBody] UpdateWarehouseDto dto, CancellationToken ct)
    {
        var result = await _warehouseService.UpdateWarehouseAsync(code, dto, ct);
        if (!result.Succeeded) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportPdf([FromQuery] WarehouseFilterDto filter, CancellationToken ct)
    {
        var warehouses = await _warehouseService.GetAllFilteredWarehousesAsync(filter, ct);
        var pdfBytes = _pdfService.GenerateWarehousesPdf(warehouses, filter);
        var fileName = $"Warehouses_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }
}
