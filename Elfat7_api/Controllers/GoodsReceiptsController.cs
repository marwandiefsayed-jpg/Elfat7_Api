using Elfat7.Application.DTOs.GoodsReceiving;
using Elfat7.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Elfat7.Api.Controllers;

[ApiController]
[Route("api/goods-receipts")]
public class GoodsReceiptsController : ControllerBase
{
    private readonly IGoodsReceiptService _receiptService;
    private readonly IPdfReportService _pdfService;

    public GoodsReceiptsController(IGoodsReceiptService receiptService, IPdfReportService pdfService)
    {
        _receiptService = receiptService;
        _pdfService = pdfService;
    }

    [HttpGet]
    public async Task<IActionResult> GetGoodsReceipts([FromQuery] GoodsReceiptFilterDto filter, CancellationToken ct)
    {
        var result = await _receiptService.GetGoodsReceiptsAsync(filter, ct);
        return Ok(result);
    }

    [HttpGet("{receiptNumber}")]
    public async Task<IActionResult> GetGoodsReceipt(string receiptNumber, CancellationToken ct)
    {
        var receipt = await _receiptService.GetGoodsReceiptByNumberAsync(receiptNumber, ct);
        if (receipt == null) return NotFound(new { message = "إذن الاستلام غير موجود" });
        return Ok(receipt);
    }

    [HttpPost]
    public async Task<IActionResult> CreateGoodsReceipt([FromBody] CreateGoodsReceiptDto dto, CancellationToken ct)
    {
        var result = await _receiptService.CreateGoodsReceiptAsync(dto, ct);
        if (!result.Succeeded) return BadRequest(result);
        return CreatedAtAction(nameof(GetGoodsReceipt), new { receiptNumber = result.Data!.ReceiptNumber }, result);
    }

    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportPdf([FromQuery] GoodsReceiptFilterDto filter, CancellationToken ct)
    {
        var receipts = await _receiptService.GetAllFilteredGoodsReceiptsAsync(filter, ct);
        var pdfBytes = _pdfService.GenerateGoodsReceiptsPdf(receipts, filter);
        var fileName = $"GoodsReceipts_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    [HttpGet("{receiptNumber}/export/pdf")]
    public async Task<IActionResult> ExportReceiptPdf(string receiptNumber, CancellationToken ct)
    {
        var receipt = await _receiptService.GetGoodsReceiptByNumberAsync(receiptNumber, ct);
        if (receipt == null) return NotFound(new { message = "إذن الاستلام غير موجود" });
        var pdfBytes = _pdfService.GenerateGoodsReceiptDetailsPdf(receipt);
        var fileName = $"Receipt_{receiptNumber}_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }
}
