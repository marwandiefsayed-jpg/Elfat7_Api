using Elfat7.Application.DTOs.Products;
using Elfat7.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Elfat7.Api.Controllers;

[ApiController]
[Route("api/product-categories")]
public class ProductCategoriesController : ControllerBase
{
    private readonly IProductCategoryService _categoryService;
    private readonly IPdfReportService _pdfService;

    public ProductCategoriesController(IProductCategoryService categoryService, IPdfReportService pdfService)
    {
        _categoryService = categoryService;
        _pdfService = pdfService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories(CancellationToken ct)
    {
        var categories = await _categoryService.GetCategoriesAsync(ct);
        return Ok(categories);
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetCategory(string code, CancellationToken ct)
    {
        var category = await _categoryService.GetCategoryByCodeAsync(code, ct);
        if (category == null) return NotFound(new { message = "تصنيف الأصناف غير موجود" });
        return Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateProductCategoryDto dto, CancellationToken ct)
    {
        var result = await _categoryService.CreateCategoryAsync(dto, ct);
        if (!result.Succeeded) return BadRequest(result);
        return CreatedAtAction(nameof(GetCategory), new { code = result.Data!.Code }, result);
    }

    [HttpPut("{code}")]
    public async Task<IActionResult> UpdateCategory(string code, [FromBody] UpdateProductCategoryDto dto, CancellationToken ct)
    {
        var result = await _categoryService.UpdateCategoryAsync(code, dto, ct);
        if (!result.Succeeded) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportPdf(CancellationToken ct)
    {
        var categories = await _categoryService.GetCategoriesAsync(ct);
        var pdfBytes = _pdfService.GenerateProductCategoriesPdf(categories);
        var fileName = $"Categories_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }
}
