using Elfat7.Application.DTOs.Suppliers;
using Elfat7.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Elfat7.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;
    private readonly IPdfReportService _pdfService;

    public SuppliersController(ISupplierService supplierService, IPdfReportService pdfService)
    {
        _supplierService = supplierService;
        _pdfService = pdfService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSuppliers([FromQuery] SupplierFilterDto filter, CancellationToken ct)
    {
        var result = await _supplierService.GetSuppliersAsync(filter, ct);
        return Ok(result);
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetSupplier(string code, CancellationToken ct)
    {
        var supplier = await _supplierService.GetSupplierByCodeAsync(code, ct);
        if (supplier == null) return NotFound(new { message = "المورد المطلوب غير موجود" });
        return Ok(supplier);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierDto dto, CancellationToken ct)
    {
        var result = await _supplierService.CreateSupplierAsync(dto, ct);
        if (!result.Succeeded) return BadRequest(result);
        return CreatedAtAction(nameof(GetSupplier), new { code = result.Data!.Code }, result);
    }

    [HttpPut("{code}")]
    public async Task<IActionResult> UpdateSupplier(string code, [FromBody] UpdateSupplierDto dto, CancellationToken ct)
    {
        var result = await _supplierService.UpdateSupplierAsync(code, dto, ct);
        if (!result.Succeeded) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{code}")]
    public async Task<IActionResult> DeleteSupplier(string code, CancellationToken ct)
    {
        var result = await _supplierService.DeleteSupplierAsync(code, ct);
        if (!result.Succeeded) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportPdf([FromQuery] SupplierFilterDto filter, CancellationToken ct)
    {
        var suppliers = await _supplierService.GetAllFilteredSuppliersAsync(filter, ct);
        var pdfBytes = _pdfService.GenerateSuppliersPdf(suppliers, filter);
        var fileName = $"Suppliers_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }
}
