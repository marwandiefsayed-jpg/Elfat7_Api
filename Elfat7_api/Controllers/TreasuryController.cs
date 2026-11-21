using Elfat7.Application.DTOs.Financial;
using Elfat7.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Elfat7.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TreasuryController : ControllerBase
{
    private readonly IFinancialService _financialService;
    private readonly IPdfReportService _pdfService;

    public TreasuryController(IFinancialService financialService, IPdfReportService pdfService)
    {
        _financialService = financialService;
        _pdfService = pdfService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTreasuries(CancellationToken ct)
    {
        var treasuries = await _financialService.GetTreasuriesAsync(ct);
        return Ok(treasuries);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTreasury([FromBody] CreateTreasuryDto dto, CancellationToken ct)
    {
        var result = await _financialService.CreateTreasuryAsync(dto, ct);
        if (!result.Succeeded) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions([FromQuery] TreasuryFilterDto filter, CancellationToken ct)
    {
        var result = await _financialService.GetTreasuryTransactionsAsync(filter, ct);
        return Ok(result);
    }

    [HttpGet("transactions/export/pdf")]
    public async Task<IActionResult> ExportTransactionsPdf([FromQuery] TreasuryFilterDto filter, CancellationToken ct)
    {
        var transactions = await _financialService.GetAllFilteredTreasuryTransactionsAsync(filter, ct);
        var pdfBytes = _pdfService.GenerateTreasuryTransactionsPdf(transactions, filter);
        var fileName = $"TreasuryTransactions_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }
}
