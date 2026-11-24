using Elfat7.Application.DTOs.Customers;
using Elfat7.Application.DTOs.Products;
using Elfat7.Application.DTOs.Reports;
using Elfat7.Application.DTOs.Suppliers;
using Elfat7.Infrastructure.Pdf;
using FluentAssertions;
using System.Text;
using Xunit;

namespace Elfat7.UnitTests;

public class ArabicPdfTests
{
    private readonly PdfReportService _pdfService = new();

    [Fact]
    public void GenerateSuppliersPdf_ShouldReturnValidPdfBytes_WithArabicRtl()
    {
        // Arrange
        var suppliers = new List<SupplierDto>
        {
            new() { Code = "SUP-0001", Name = "شركة النيل لتوريد المحاصيل", Phone = "01099887766", Address = "المنصورة - الدقهلية", Balance = 45000, IsActive = true },
            new() { Code = "SUP-0002", Name = "مؤسسة الوادي للأسمدة", Phone = "01188776655", Address = "السادات - المنوفية", Balance = 0, IsActive = true }
        };

        // Act
        var pdfBytes = _pdfService.GenerateSuppliersPdf(suppliers);

        // Assert
        pdfBytes.Should().NotBeNull();
        pdfBytes.Length.Should().BeGreaterThan(1000);

        // التحقق من ترويسة ملف PDF القياسية (%PDF-)
        var header = Encoding.ASCII.GetString(pdfBytes.Take(5).ToArray());
        header.Should().Be("%PDF-");
    }

    [Fact]
    public void GenerateProfitLossReportPdf_ShouldReturnValidPdfBytes()
    {
        // Arrange
        var report = new ProfitLossReportDto
        {
            PeriodStart = DateTime.UtcNow.AddMonths(-1),
            PeriodEnd = DateTime.UtcNow,
            TotalRevenue = 1500000m,
            TotalCOGS = 1200000m,
            OperatingExpenses = 85000m
        };

        // Act
        var pdfBytes = _pdfService.GenerateProfitLossReportPdf(report);

        // Assert
        pdfBytes.Should().NotBeNull();
        pdfBytes.Length.Should().BeGreaterThan(1000);
        var header = Encoding.ASCII.GetString(pdfBytes.Take(5).ToArray());
        header.Should().Be("%PDF-");
    }

    [Fact]
    public void GenerateCustomersPdf_ShouldReturnValidPdfBytes()
    {
        // Arrange
        var customers = new List<CustomerDto>
        {
            new() { Code = "CUST-0001", Name = "شركة الدلتا لتجارة الجملة", Phone = "01011223344", Balance = 65000, IsActive = true }
        };

        // Act
        var pdfBytes = _pdfService.GenerateCustomersPdf(customers);

        // Assert
        pdfBytes.Should().NotBeNull();
        pdfBytes.Length.Should().BeGreaterThan(1000);
        var header = Encoding.ASCII.GetString(pdfBytes.Take(5).ToArray());
        header.Should().Be("%PDF-");
    }
}
