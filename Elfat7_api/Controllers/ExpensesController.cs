using Elfat7.Application.DTOs.Financial;
using Elfat7.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Elfat7.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly IFinancialService _financialService;
    private readonly IPdfReportService _pdfService;

    public ExpensesController(IFinancialService financialService, IPdfReportService pdfService)
    {
        _financialService = financialService;
        _pdfService = pdfService;
    }

    [HttpGet]
    public async Task<IActionResult> GetExpenses([FromQuery] ExpenseFilterDto filter, CancellationToken ct)
    {
        var result = await _financialService.GetExpensesAsync(filter, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateExpense([FromBody] CreateOperatingExpenseDto dto, CancellationToken ct)
    {
        var result = await _financialService.CreateExpenseAsync(dto, ct);
        if (!result.Succeeded) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportPdf([FromQuery] ExpenseFilterDto filter, CancellationToken ct)
    {
        var expenses = await _financialService.GetAllFilteredExpensesAsync(filter, ct);
        var pdfBytes = _pdfService.GenerateExpensesPdf(expenses, filter);
        var fileName = $"OperatingExpenses_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }
}
