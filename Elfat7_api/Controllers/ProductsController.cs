using Elfat7.Application.DTOs.Products;
using Elfat7.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Elfat7.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IPdfReportService _pdfService;

    public ProductsController(IProductService productService, IPdfReportService pdfService)
    {
        _productService = productService;
        _pdfService = pdfService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] ProductFilterDto filter, CancellationToken ct)
    {
        var result = await _productService.GetProductsAsync(filter, ct);
        return Ok(result);
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetProduct(string code, CancellationToken ct)
    {
        var product = await _productService.GetProductByCodeAsync(code, ct);
        if (product == null) return NotFound(new { message = "الصنف المطلوب غير موجود" });
        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto, CancellationToken ct)
    {
        var result = await _productService.CreateProductAsync(dto, ct);
        if (!result.Succeeded) return BadRequest(result);
        return CreatedAtAction(nameof(GetProduct), new { code = result.Data!.Code }, result);
    }

    [HttpPut("{code}")]
    public async Task<IActionResult> UpdateProduct(string code, [FromBody] UpdateProductDto dto, CancellationToken ct)
    {
        var result = await _productService.UpdateProductAsync(code, dto, ct);
        if (!result.Succeeded) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{code}")]
    public async Task<IActionResult> DeleteProduct(string code, CancellationToken ct)
    {
        var result = await _productService.DeleteProductAsync(code, ct);
        if (!result.Succeeded) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportPdf([FromQuery] ProductFilterDto filter, CancellationToken ct)
    {
        var products = await _productService.GetAllFilteredProductsAsync(filter, ct);
        var pdfBytes = _pdfService.GenerateProductsPdf(products, filter);
        var fileName = $"Products_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }
}
