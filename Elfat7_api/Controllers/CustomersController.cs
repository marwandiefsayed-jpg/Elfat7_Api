using Elfat7.Application.DTOs.Customers;
using Elfat7.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Elfat7.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly IPdfReportService _pdfService;

    public CustomersController(ICustomerService customerService, IPdfReportService pdfService)
    {
        _customerService = customerService;
        _pdfService = pdfService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomers([FromQuery] CustomerFilterDto filter, CancellationToken ct)
    {
        var result = await _customerService.GetCustomersAsync(filter, ct);
        return Ok(result);
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetCustomer(string code, CancellationToken ct)
    {
        var customer = await _customerService.GetCustomerByCodeAsync(code, ct);
        if (customer == null) return NotFound(new { message = "العميل المطلوب غير موجود" });
        return Ok(customer);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerDto dto, CancellationToken ct)
    {
        var result = await _customerService.CreateCustomerAsync(dto, ct);
        if (!result.Succeeded) return BadRequest(result);
        return CreatedAtAction(nameof(GetCustomer), new { code = result.Data!.Code }, result);
    }

    [HttpPut("{code}")]
    public async Task<IActionResult> UpdateCustomer(string code, [FromBody] UpdateCustomerDto dto, CancellationToken ct)
    {
        var result = await _customerService.UpdateCustomerAsync(code, dto, ct);
        if (!result.Succeeded) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{code}")]
    public async Task<IActionResult> DeleteCustomer(string code, CancellationToken ct)
    {
        var result = await _customerService.DeleteCustomerAsync(code, ct);
        if (!result.Succeeded) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportPdf([FromQuery] CustomerFilterDto filter, CancellationToken ct)
    {
        var customers = await _customerService.GetAllFilteredCustomersAsync(filter, ct);
        var pdfBytes = _pdfService.GenerateCustomersPdf(customers, filter);
        var fileName = $"Customers_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }
}
