using Elfat7.Application.DTOs.Financial;
using Elfat7.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Elfat7.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IFinancialService _financialService;
    private readonly IPdfReportService _pdfService;

    public PaymentsController(IFinancialService financialService, IPdfReportService pdfService)
    {
        _financialService = financialService;
        _pdfService = pdfService;
    }

    // Supplier Payments (سندات صرف موردين)
    [HttpGet("suppliers")]
    public async Task<IActionResult> GetSupplierPayments([FromQuery] PaymentFilterDto filter, CancellationToken ct)
    {
        var result = await _financialService.GetSupplierPaymentsAsync(filter, ct);
        return Ok(result);
    }

    [HttpPost("suppliers")]
    public async Task<IActionResult> CreateSupplierPayment([FromBody] CreateSupplierPaymentDto dto, CancellationToken ct)
    {
        var result = await _financialService.CreateSupplierPaymentAsync(dto, ct);
        if (!result.Succeeded) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("suppliers/export/pdf")]
    public async Task<IActionResult> ExportSupplierPaymentsPdf([FromQuery] PaymentFilterDto filter, CancellationToken ct)
    {
        var payments = await _financialService.GetAllFilteredSupplierPaymentsAsync(filter, ct);
        var pdfBytes = _pdfService.GenerateSupplierPaymentsPdf(payments, filter);
        var fileName = $"SupplierPayments_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    // Customer Payments (سندات قبض عملاء)
    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomerPayments([FromQuery] PaymentFilterDto filter, CancellationToken ct)
    {
        var result = await _financialService.GetCustomerPaymentsAsync(filter, ct);
        return Ok(result);
    }

    [HttpPost("customers")]
    public async Task<IActionResult> CreateCustomerPayment([FromBody] CreateCustomerPaymentDto dto, CancellationToken ct)
    {
        var result = await _financialService.CreateCustomerPaymentAsync(dto, ct);
        if (!result.Succeeded) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("customers/export/pdf")]
    public async Task<IActionResult> ExportCustomerPaymentsPdf([FromQuery] PaymentFilterDto filter, CancellationToken ct)
    {
        var payments = await _financialService.GetAllFilteredCustomerPaymentsAsync(filter, ct);
        var pdfBytes = _pdfService.GenerateCustomerPaymentsPdf(payments, filter);
        var fileName = $"CustomerPayments_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }
}
