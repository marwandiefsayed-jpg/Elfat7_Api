using Elfat7.Application.Common;
using Elfat7.Application.DTOs.Inventory;
using Elfat7.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Elfat7.Api.Controllers;

[ApiController]
[Route("api/stock-adjustments")]
public class StockAdjustmentsController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public StockAdjustmentsController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAdjustments([FromQuery] PagedFilterRequest filter, CancellationToken ct)
    {
        var result = await _inventoryService.GetStockAdjustmentsAsync(filter, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAdjustment([FromBody] CreateStockAdjustmentDto dto, CancellationToken ct)
    {
        var result = await _inventoryService.CreateStockAdjustmentAsync(dto, ct);
        if (!result.Succeeded) return BadRequest(result);
        return Ok(result);
    }
}
