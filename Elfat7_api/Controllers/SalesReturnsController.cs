using Elfat7.Application.DTOs.SalesReturns;
using Elfat7.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Elfat7.Api.Controllers;

[ApiController]
[Route("api/sales-returns")]
public class SalesReturnsController : ControllerBase
{
    private readonly ISalesReturnService _returnService;
    private readonly IPdfReportService _pdfService;

    public SalesReturnsController(ISalesReturnService returnService, IPdfReportService pdfService)
    {
        _returnService = returnService;
        _pdfService = pdfService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSalesReturns([FromQuery] SalesReturnFilterDto filter, CancellationToken ct)
    {
        var result = await _returnService.GetSalesReturnsAsync(filter, ct);
        return Ok(result);
    }

    [HttpGet("{returnNumber}")]
    public async Task<IActionResult> GetSalesReturn(string returnNumber, CancellationToken ct)
    {
        var ret = await _returnService.GetSalesReturnByNumberAsync(returnNumber, ct);
        if (ret == null) return NotFound(new { message = "مرتجع المبيعات المطلوب غير موجود" });
        return Ok(ret);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSalesReturn([FromBody] CreateSalesReturnDto dto, CancellationToken ct)
    {
        var result = await _returnService.CreateSalesReturnAsync(dto, ct);
        if (!result.Succeeded) return BadRequest(result);
        return CreatedAtAction(nameof(GetSalesReturn), new { returnNumber = result.Data!.ReturnNumber }, result);
    }

    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportPdf([FromQuery] SalesReturnFilterDto filter, CancellationToken ct)
    {
        var returns = await _returnService.GetAllFilteredSalesReturnsAsync(filter, ct);
        var pdfBytes = _pdfService.GenerateSalesReturnsPdf(returns, filter);
        var fileName = $"SalesReturns_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }
}
