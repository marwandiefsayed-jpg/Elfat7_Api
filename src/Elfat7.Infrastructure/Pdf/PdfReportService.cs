using Elfat7.Application.Common;
using Elfat7.Application.DTOs.Customers;
using Elfat7.Application.DTOs.Deliveries;
using Elfat7.Application.DTOs.Financial;
using Elfat7.Application.DTOs.GoodsReceiving;
using Elfat7.Application.DTOs.Inventory;
using Elfat7.Application.DTOs.Products;
using Elfat7.Application.DTOs.Purchasing;
using Elfat7.Application.DTOs.Reports;
using Elfat7.Application.DTOs.Sales;
using Elfat7.Application.DTOs.SalesReturns;
using Elfat7.Application.DTOs.Suppliers;
using Elfat7.Application.DTOs.Warehouses;
using Elfat7.Application.Interfaces;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Reflection;

namespace Elfat7.Infrastructure.Pdf;

public class PdfReportService : IPdfReportService
{
    private static bool _fontsRegistered = false;
    private static readonly object _fontLock = new();

    public PdfReportService()
    {
        EnsureFontsRegistered();
    }

    private static void EnsureFontsRegistered()
    {
        if (_fontsRegistered) return;
        lock (_fontLock)
        {
            if (_fontsRegistered) return;
            QuestPDF.Settings.License = LicenseType.Community;

            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var tahomaResource = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("tahoma.ttf", StringComparison.OrdinalIgnoreCase));
                if (tahomaResource != null)
                {
                    using var stream = assembly.GetManifestResourceStream(tahomaResource);
                    if (stream != null) FontManager.RegisterFontFromStream(stream);
                }
                else if (File.Exists(@"C:\Windows\Fonts\tahoma.ttf"))
                {
                    using var stream = File.OpenRead(@"C:\Windows\Fonts\tahoma.ttf");
                    FontManager.RegisterFontFromStream(stream);
                }

                var tahomaBdResource = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("tahomabd.ttf", StringComparison.OrdinalIgnoreCase));
                if (tahomaBdResource != null)
                {
                    using var stream = assembly.GetManifestResourceStream(tahomaBdResource);
                    if (stream != null) FontManager.RegisterFontFromStream(stream);
                }
                else if (File.Exists(@"C:\Windows\Fonts\tahomabd.ttf"))
                {
                    using var stream = File.OpenRead(@"C:\Windows\Fonts\tahomabd.ttf");
                    FontManager.RegisterFontFromStream(stream);
                }
            }
            catch
            {
                // Fallback to system fonts
            }

            _fontsRegistered = true;
        }
    }

    private static void ApplyStandardPageLayout(PageDescriptor page, string title, string? subTitle = null, bool isLandscape = false)
    {
        page.Size(isLandscape ? PageSizes.A4.Landscape() : PageSizes.A4);
        page.Margin(20);
        page.ContentFromRightToLeft();
        page.DefaultTextStyle(x => x.FontFamily("Tahoma").FontSize(9).FontColor(Colors.Grey.Darken4));

        page.Header().Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("شركة الفتح لتجارة وتوزيع الحاصلات الزراعية").Bold().FontSize(13).FontColor(Colors.Green.Darken3);
                    c.Item().Text("تجارة وتوزيع المحاصيل الزراعية والحبوب والأسمدة").FontSize(8).FontColor(Colors.Grey.Darken1);
                    c.Item().Text("س.ت: 128475 - ب.ض: 340-981-120 - المركز الرئيسي: طنطا").FontSize(8).FontColor(Colors.Grey.Darken1);
                });

                row.RelativeItem().AlignLeft().Column(c =>
                {
                    c.Item().Text(title).Bold().FontSize(14).FontColor(Colors.Blue.Darken3);
                    if (!string.IsNullOrWhiteSpace(subTitle))
                        c.Item().Text(subTitle).FontSize(9).FontColor(Colors.Grey.Darken2);
                    c.Item().Text($"تاريخ الاستخراج: {ArabicLocalizationHelper.FormatDate(DateTime.UtcNow)}").FontSize(8).FontColor(Colors.Grey.Darken1);
                });
            });

            col.Item().PaddingVertical(4).LineHorizontal(1).LineColor(Colors.Green.Darken2);
        });

        page.Footer().Column(col =>
        {
            col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
            col.Item().PaddingTop(3).Row(row =>
            {
                row.RelativeItem().Text("شركة الفتح لتجارة وتوزيع الحاصلات الزراعية - جميع الحقوق محفوظة").FontSize(7).FontColor(Colors.Grey.Darken1);
                row.RelativeItem().AlignLeft().Text(x =>
                {
                    x.Span("صفحة ");
                    x.CurrentPageNumber();
                    x.Span(" من ");
                    x.TotalPages();
                });
            });
        });
    }

    // 1. Suppliers PDF
    public byte[] GenerateSuppliersPdf(List<SupplierDto> suppliers, SupplierFilterDto? filter = null)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ApplyStandardPageLayout(page, "تقرير الموردين والأرصدة المستحقة");
                page.Content().PaddingTop(6).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(75); // كود المورد
                            columns.RelativeColumn(3);  // اسم المورد
                            columns.RelativeColumn(2);  // الهاتف
                            columns.RelativeColumn(3);  // العنوان
                            columns.RelativeColumn(2);  // الرصيد الدائن
                            columns.ConstantColumn(50); // الحالة
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("كود المورد").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("اسم المورد").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("رقم الهاتف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("العنوان").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الرصيد المستحق").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الحالة").Bold();
                        });

                        foreach (var s in suppliers)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(s.Code);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(s.Name).Bold();
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(s.Phone);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(s.Address ?? "-");
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(s.BalanceFormatted).FontColor(s.Balance > 0 ? Colors.Red.Darken2 : Colors.Grey.Darken3);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(s.IsActive ? "نشط" : "غير نشط");
                        }
                    });

                    col.Item().PaddingTop(8).AlignLeft().Text($"إجمالي عدد الموردين: {suppliers.Count} | إجمالي الأرصدة المستحقة: {ArabicLocalizationHelper.FormatCurrency(suppliers.Sum(s => s.Balance))}").Bold();
                });
            });
        });

        return doc.GeneratePdf();
    }

    // 2. Products PDF
    public byte[] GenerateProductsPdf(List<ProductDto> products, ProductFilterDto? filter = null)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ApplyStandardPageLayout(page, "دليل الأصناف والأسعار الزراعية");
                page.Content().PaddingTop(6).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(75); // كود الصنف
                            columns.RelativeColumn(3);  // اسم الصنف
                            columns.RelativeColumn(2);  // التصنيف
                            columns.ConstantColumn(50); // الوحدة
                            columns.RelativeColumn(2);  // سعر الشراء
                            columns.RelativeColumn(2);  // سعر البيع
                            columns.RelativeColumn(2);  // متوسط التكلفة
                            columns.RelativeColumn(2);  // الرصيد المخزني
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("كود الصنف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("اسم الصنف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("التصنيف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الوحدة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("سعر الشراء").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("سعر البيع").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("متوسط التكلفة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الرصيد الحالي").Bold();
                        });

                        foreach (var p in products)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(p.Code);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(p.Name).Bold();
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(p.CategoryName);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(p.Unit);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(p.DefaultPurchasePriceFormatted);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(p.DefaultSellingPriceFormatted);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(p.AverageCostFormatted);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(ArabicLocalizationHelper.FormatQuantity(p.TotalStockQuantity, p.Unit)).Bold();
                        }
                    });

                    col.Item().PaddingTop(8).AlignLeft().Text($"إجمالي عدد الأصناف: {products.Count}").Bold();
                });
            });
        });

        return doc.GeneratePdf();
    }

    // 3. Product Categories PDF
    public byte[] GenerateProductCategoriesPdf(List<ProductCategoryDto> categories)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ApplyStandardPageLayout(page, "تقرير تصنيفات الأصناف الزراعية");
                page.Content().PaddingTop(6).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(80);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(4);
                            columns.ConstantColumn(90);
                            columns.ConstantColumn(60);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("كود التصنيف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("اسم التصنيف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الوصف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("عدد الأصناف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الحالة").Bold();
                        });

                        foreach (var c in categories)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(c.Code);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(c.Name).Bold();
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(c.Description ?? "-");
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(c.ProductCount.ToString());
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(c.IsActive ? "نشط" : "غير نشط");
                        }
                    });
                });
            });
        });

        return doc.GeneratePdf();
    }

    // 4. Warehouses PDF
    public byte[] GenerateWarehousesPdf(List<WarehouseDto> warehouses, WarehouseFilterDto? filter = null)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ApplyStandardPageLayout(page, "دليل مخازن الشركة وصوامع التخزين");
                page.Content().PaddingTop(6).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(75);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.ConstantColumn(80);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("كود المخزن").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("اسم المخزن").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الموقع والعنوان").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("أمين المخزن").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("هاتف التواصل").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الأصناف المتوفرة").Bold();
                        });

                        foreach (var w in warehouses)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(w.Code);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(w.Name).Bold();
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(w.Address ?? "-");
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(w.KeeperName ?? "-");
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(w.KeeperPhone ?? "-");
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(w.ProductsInStockCount.ToString());
                        }
                    });
                });
            });
        });

        return doc.GeneratePdf();
    }

    // 5. Inventory Stock PDF
    public byte[] GenerateInventoryPdf(List<InventoryItemDto> inventory, InventoryFilterDto? filter = null)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ApplyStandardPageLayout(page, "تقرير أرصدة المخزون والتقييم المالي");
                page.Content().PaddingTop(6).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(75);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.ConstantColumn(50);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("كود الصنف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("اسم الصنف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("التصنيف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الوحدة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الرصيد الفعلي").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("المتاح للصرف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("قيمة المخزون").Bold();
                        });

                        foreach (var item in inventory)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.ProductCode);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.ProductName).Bold();
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.CategoryName);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.Unit);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(ArabicLocalizationHelper.FormatQuantity(item.TotalQuantity));
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(ArabicLocalizationHelper.FormatQuantity(item.TotalAvailableQuantity));
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.TotalValuationFormatted).Bold();
                        }
                    });

                    col.Item().PaddingTop(8).AlignLeft().Text($"إجمالي تقييم المخزون المالي: {ArabicLocalizationHelper.FormatCurrency(inventory.Sum(i => i.TotalValuation))}").Bold();
                });
            });
        });

        return doc.GeneratePdf();
    }

    // 6. Stock Movements PDF
    public byte[] GenerateStockMovementsPdf(List<StockMovementDto> movements, StockMovementFilterDto? filter = null)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ApplyStandardPageLayout(page, "سجل حركات المخزون والمستندات المخزنية", null, true);
                page.Content().PaddingTop(6).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(90);  // رقم الحركة
                            columns.RelativeColumn(2);   // التاريخ
                            columns.RelativeColumn(3);   // المخزن
                            columns.RelativeColumn(3);   // الصنف
                            columns.RelativeColumn(2);   // نوع الحركة
                            columns.RelativeColumn(2);   // الكمية
                            columns.RelativeColumn(2);   // تكلفة الوحدة
                            columns.RelativeColumn(2);   // إجمالي التكلفة
                            columns.RelativeColumn(2);   // رقم المرجع
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("رقم الحركة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("التاريخ والوقت").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("المخزن").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الصنف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("نوع الحركة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الكمية").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("تكلفة الوحدة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("إجمالي القيمة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("رقم المرجع").Bold();
                        });

                        foreach (var m in movements)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(m.MovementNumber);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(m.CreatedAtFormatted);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(m.WarehouseName);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(m.ProductName).Bold();
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(m.MovementTypeArabic);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(ArabicLocalizationHelper.FormatQuantity(m.Quantity, m.Unit)).FontColor(m.Quantity > 0 ? Colors.Green.Darken2 : Colors.Red.Darken2);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(m.UnitCostFormatted);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(m.TotalCostFormatted);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(m.ReferenceNumber);
                        }
                    });

                    col.Item().PaddingTop(8).AlignLeft().Text($"إجمالي عدد الحركات المخزنية: {movements.Count}").Bold();
                });
            });
        });

        return doc.GeneratePdf();
    }

    // 7. Stock Transfers PDF
    public byte[] GenerateStockTransfersPdf(List<StockTransferDto> transfers)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ApplyStandardPageLayout(page, "سجل التحويلات بين المخازن");
                page.Content().PaddingTop(6).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(85);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("رقم التحويل").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("تاريخ التحويل").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("مخزن المصدر").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("مخزن الوجهة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الحالة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("ملاحظات").Bold();
                        });

                        foreach (var t in transfers)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(t.TransferNumber);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(t.TransferDateFormatted);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(t.SourceWarehouseName);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(t.DestinationWarehouseName);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(t.StatusArabic);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(t.Notes ?? "-");
                        }
                    });
                });
            });
        });

        return doc.GeneratePdf();
    }

    // 8. Customers PDF
    public byte[] GenerateCustomersPdf(List<CustomerDto> customers, CustomerFilterDto? filter = null)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ApplyStandardPageLayout(page, "دليل العملاء والأرصدة المدينة");
                page.Content().PaddingTop(6).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(75);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("كود العميل").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("اسم العميل").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("رقم الهاتف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("العنوان").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الرصيد المستحق (مدين)").Bold();
                        });

                        foreach (var c in customers)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(c.Code);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(c.Name).Bold();
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(c.Phone);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(c.Address ?? "-");
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(c.BalanceFormatted).FontColor(c.Balance > 0 ? Colors.Red.Darken2 : Colors.Grey.Darken3);
                        }
                    });

                    col.Item().PaddingTop(8).AlignLeft().Text($"إجمالي عدد العملاء: {customers.Count} | إجمالي المديونية المستحقة على العملاء: {ArabicLocalizationHelper.FormatCurrency(customers.Sum(c => c.Balance))}").Bold();
                });
            });
        });

        return doc.GeneratePdf();
    }

    // 9. Purchase Orders PDF
    public byte[] GeneratePurchaseOrdersPdf(List<PurchaseOrderDto> orders, PurchaseOrderFilterDto? filter = null)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ApplyStandardPageLayout(page, "سجل أوامر الشراء والتوريد");
                page.Content().PaddingTop(6).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(85);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("رقم الأمر").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("اسم المورد").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("تاريخ الأمر").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("تاريخ التسليم").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الحالة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("إجمالي القيمة").Bold();
                        });

                        foreach (var po in orders)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(po.OrderNumber);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(po.SupplierName).Bold();
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(po.OrderDateFormatted);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(po.ExpectedDeliveryDateFormatted ?? "-");
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(po.StatusArabic);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(po.TotalAmountFormatted).Bold();
                        }
                    });

                    col.Item().PaddingTop(8).AlignLeft().Text($"إجمالي قيمة أوامر الشراء: {ArabicLocalizationHelper.FormatCurrency(orders.Sum(po => po.TotalAmount))}").Bold();
                });
            });
        });

        return doc.GeneratePdf();
    }

    // 10. Purchase Order Details Document PDF
    public byte[] GeneratePurchaseOrderDetailsPdf(PurchaseOrderDetailsDto order)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ApplyStandardPageLayout(page, $"أمر توريد وشراء رقم {order.OrderNumber}", $"المورد: {order.SupplierName} ({order.SupplierCode})");
                page.Content().PaddingTop(6).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text($"تاريخ الأمر: {order.OrderDateFormatted}");
                        row.RelativeItem().Text($"تاريخ التسليم المتوقع: {order.ExpectedDeliveryDateFormatted}");
                        row.RelativeItem().Text($"حالة الأمر: {order.StatusArabic}").Bold();
                    });

                    col.Item().PaddingVertical(6).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(75);
                            columns.RelativeColumn(3);
                            columns.ConstantColumn(50);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("كود الصنف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("اسم الصنف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الوحدة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الكمية المطلوبة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("سعر الوحدة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("المستلم فعلياً").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("إجمالي البند").Bold();
                        });

                        foreach (var item in order.Items)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.ProductCode);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.ProductName).Bold();
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.Unit);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(ArabicLocalizationHelper.FormatQuantity(item.Quantity));
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.UnitPriceFormatted);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(ArabicLocalizationHelper.FormatQuantity(item.ReceivedQuantity));
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.LineTotalFormatted).Bold();
                        }
                    });

                    col.Item().PaddingTop(10).AlignLeft().Column(c =>
                    {
                        c.Item().Text($"المجموع قبل الخصم: {ArabicLocalizationHelper.FormatCurrency(order.SubTotal)}");
                        c.Item().Text($"قيمة الخصم: {ArabicLocalizationHelper.FormatCurrency(order.DiscountAmount)}");
                        c.Item().Text($"الصافي الإجمالي: {order.TotalAmountFormatted}").Bold().FontSize(12).FontColor(Colors.Green.Darken3);
                    });

                    if (!string.IsNullOrWhiteSpace(order.Notes))
                    {
                        col.Item().PaddingTop(8).Text($"ملاحظات وشروط التوريد: {order.Notes}").FontSize(8).FontColor(Colors.Grey.Darken2);
                    }
                });
            });
        });

        return doc.GeneratePdf();
    }

    // 11. Goods Receipts PDF
    public byte[] GenerateGoodsReceiptsPdf(List<GoodsReceiptDto> receipts, GoodsReceiptFilterDto? filter = null)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ApplyStandardPageLayout(page, "سجل أذون استلام البضائع والمشتريات بالمخازن");
                page.Content().PaddingTop(6).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(85);
                            columns.ConstantColumn(85);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("رقم الإذن").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("أمر الشراء").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("المورد").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("مخزن الاستلام").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("تاريخ الاستلام").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("قيمة البضاعة").Bold();
                        });

                        foreach (var r in receipts)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(r.ReceiptNumber);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(r.PurchaseOrderNumber);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(r.SupplierName).Bold();
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(r.WarehouseName);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(r.ReceiptDateFormatted);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(r.TotalAmountFormatted).Bold();
                        }
                    });

                    col.Item().PaddingTop(8).AlignLeft().Text($"إجمالي قيمة البضائع المستلمة: {ArabicLocalizationHelper.FormatCurrency(receipts.Sum(r => r.TotalAmount))}").Bold();
                });
            });
        });

        return doc.GeneratePdf();
    }

    // 12. Goods Receipt Details PDF
    public byte[] GenerateGoodsReceiptDetailsPdf(GoodsReceiptDetailsDto receipt)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ApplyStandardPageLayout(page, $"إذن استلام بضاعة رقم {receipt.ReceiptNumber}", $"أمر الشراء: {receipt.PurchaseOrderNumber} | المخزن: {receipt.WarehouseName}");
                page.Content().PaddingTop(6).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text($"المورد: {receipt.SupplierName} ({receipt.SupplierCode})").Bold();
                        row.RelativeItem().Text($"تاريخ الاستلام الفعلي: {receipt.ReceiptDateFormatted}");
                        row.RelativeItem().Text($"المخزن: {receipt.WarehouseName}");
                    });

                    col.Item().PaddingVertical(6).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(80);
                            columns.RelativeColumn(3);
                            columns.ConstantColumn(50);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("كود الصنف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("اسم الصنف المستلم").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الوحدة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الكمية المستلمة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("سعر التكلفة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الإجمالي").Bold();
                        });

                        foreach (var item in receipt.Items)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.ProductCode);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.ProductName).Bold();
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.Unit);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(ArabicLocalizationHelper.FormatQuantity(item.Quantity));
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.UnitCostFormatted);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.LineTotalFormatted).Bold();
                        }
                    });

                    col.Item().PaddingTop(10).AlignLeft().Text($"إجمالي قيمة إذن الاستلام: {receipt.TotalAmountFormatted}").Bold().FontSize(12).FontColor(Colors.Green.Darken3);

                    if (!string.IsNullOrWhiteSpace(receipt.Notes))
                    {
                        col.Item().PaddingTop(8).Text($"ملاحظات الاستلام والفحص: {receipt.Notes}").FontSize(8).FontColor(Colors.Grey.Darken2);
                    }
                });
            });
        });

        return doc.GeneratePdf();
    }

    // 13. Sales Orders PDF
    public byte[] GenerateSalesOrdersPdf(List<SalesOrderDto> orders, SalesOrderFilterDto? filter = null)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ApplyStandardPageLayout(page, "سجل طلبات البيع من العملاء");
                page.Content().PaddingTop(6).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(85);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("رقم الطلب").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("العميل").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("تاريخ الطلب").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الحالة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("إجمالي القيمة").Bold();
                        });

                        foreach (var so in orders)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(so.OrderNumber);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(so.CustomerName).Bold();
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(so.OrderDateFormatted);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(so.StatusArabic);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(so.TotalAmountFormatted).Bold();
                        }
                    });

                    col.Item().PaddingTop(8).AlignLeft().Text($"إجمالي قيمة طلبات البيع: {ArabicLocalizationHelper.FormatCurrency(orders.Sum(so => so.TotalAmount))}").Bold();
                });
            });
        });

        return doc.GeneratePdf();
    }

    // 14. Sales Invoices PDF
    public byte[] GenerateSalesInvoicesPdf(List<SalesInvoiceDto> invoices, SalesInvoiceFilterDto? filter = null)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ApplyStandardPageLayout(page, "سجل فواتير المبيعات التجارية");
                page.Content().PaddingTop(6).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(85);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("رقم الفاتورة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("اسم العميل").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("تاريخ الفاتورة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("حالة الفاتورة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("حالة السداد").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الإجمالي").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("المتبقي").Bold();
                        });

                        foreach (var inv in invoices)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(inv.InvoiceNumber);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(inv.CustomerName).Bold();
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(inv.InvoiceDateFormatted);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(inv.StatusArabic);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(inv.PaymentStatusArabic);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(inv.TotalAmountFormatted).Bold();
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(inv.RemainingAmountFormatted);
                        }
                    });

                    col.Item().PaddingTop(8).AlignLeft().Text($"إجمالي قيمة الفواتير: {ArabicLocalizationHelper.FormatCurrency(invoices.Sum(i => i.TotalAmount))} | إجمالي المتبقي: {ArabicLocalizationHelper.FormatCurrency(invoices.Sum(i => i.RemainingAmount))}").Bold();
                });
            });
        });

        return doc.GeneratePdf();
    }

    // 15. Sales Invoice Details Document PDF
    public byte[] GenerateSalesInvoiceDetailsPdf(SalesInvoiceDetailsDto invoice)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ApplyStandardPageLayout(page, $"فاتورة مبيعات ضريبية رقم {invoice.InvoiceNumber}", $"العميل: {invoice.CustomerName} ({invoice.CustomerCode})");
                page.Content().PaddingTop(6).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text($"تاريخ الفاتورة: {invoice.InvoiceDateFormatted}").Bold();
                        row.RelativeItem().Text($"حالة التسليم: {invoice.StatusArabic}");
                        row.RelativeItem().Text($"حالة السداد: {invoice.PaymentStatusArabic}").Bold();
                    });

                    col.Item().PaddingVertical(6).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(75);
                            columns.RelativeColumn(3);
                            columns.ConstantColumn(50);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("كود الصنف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("اسم الصنف المباع").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الوحدة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الكمية المباعة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("سعر البيع").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("المسلم للعميل").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("إجمالي البند").Bold();
                        });

                        foreach (var item in invoice.Items)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.ProductCode);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.ProductName).Bold();
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.Unit);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(ArabicLocalizationHelper.FormatQuantity(item.Quantity));
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.UnitPriceFormatted);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(ArabicLocalizationHelper.FormatQuantity(item.DeliveredQuantity));
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.LineTotalFormatted).Bold();
                        }
                    });

                    col.Item().PaddingTop(10).AlignLeft().Column(c =>
                    {
                        c.Item().Text($"الإجمالي قبل الخصم: {ArabicLocalizationHelper.FormatCurrency(invoice.SubTotal)}");
                        c.Item().Text($"قيمة الخصم التجاري: {ArabicLocalizationHelper.FormatCurrency(invoice.DiscountAmount)}");
                        c.Item().Text($"صافي الفاتورة الإجمالي: {invoice.TotalAmountFormatted}").Bold().FontSize(12).FontColor(Colors.Blue.Darken3);
                        c.Item().Text($"المبلغ المسدد: {ArabicLocalizationHelper.FormatCurrency(invoice.PaidAmount)}");
                        c.Item().Text($"المبلغ المتبقي: {invoice.RemainingAmountFormatted}").Bold().FontColor(invoice.RemainingAmount > 0 ? Colors.Red.Darken2 : Colors.Green.Darken2);
                    });

                    if (!string.IsNullOrWhiteSpace(invoice.Notes))
                    {
                        col.Item().PaddingTop(8).Text($"ملاحظات: {invoice.Notes}").FontSize(8).FontColor(Colors.Grey.Darken2);
                    }
                });
            });
        });

        return doc.GeneratePdf();
    }

    // 16. Deliveries PDF
    public byte[] GenerateDeliveriesPdf(List<DeliveryDto> deliveries, DeliveryFilterDto? filter = null)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ApplyStandardPageLayout(page, "سجل أذون الصرف والتسليم للعملاء");
                page.Content().PaddingTop(6).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(85);
                            columns.ConstantColumn(85);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("رقم الإذن").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("فاتورة المبيعات").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("اسم العميل").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("مخزن الصرف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("تاريخ الصرف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("اسم السائق").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الحالة").Bold();
                        });

                        foreach (var d in deliveries)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(d.DeliveryNumber);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(d.SalesInvoiceNumber);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(d.CustomerName).Bold();
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(d.WarehouseName);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(d.DeliveryDateFormatted);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(d.DriverName ?? "-");
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(d.StatusArabic);
                        }
                    });

                    col.Item().PaddingTop(8).AlignLeft().Text($"إجمالي عدد أذون التسليم: {deliveries.Count}").Bold();
                });
            });
        });

        return doc.GeneratePdf();
    }

    // 17. Delivery Details PDF
    public byte[] GenerateDeliveryDetailsPdf(DeliveryDetailsDto delivery)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ApplyStandardPageLayout(page, $"إذن صرف وتسليم بضاعة رقم {delivery.DeliveryNumber}", $"فاتورة المبيعات: {delivery.SalesInvoiceNumber} | مخزن الصرف: {delivery.WarehouseName}");
                page.Content().PaddingTop(6).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text($"العميل المستلم: {delivery.CustomerName}").Bold();
                        row.RelativeItem().Text($"تاريخ الصرف والتسليم: {delivery.DeliveryDateFormatted}");
                        row.RelativeItem().Text($"السائق / رقم السيارة: {(delivery.DriverName ?? "-")} / {(delivery.VehicleNumber ?? "-")}");
                    });

                    col.Item().PaddingVertical(6).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(80);
                            columns.RelativeColumn(4);
                            columns.ConstantColumn(60);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("كود الصنف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("اسم الصنف المصروف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الوحدة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الكمية المصروفة").Bold();
                        });

                        foreach (var item in delivery.Items)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.ProductCode);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.ProductName).Bold();
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.Unit);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(ArabicLocalizationHelper.FormatQuantity(item.Quantity)).Bold();
                        }
                    });

                    col.Item().PaddingTop(25).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("توقيع أمين المخزن:").Bold();
                            c.Item().Text("....................................");
                        });

                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("توقيع السائق الناقل:").Bold();
                            c.Item().Text("....................................");
                        });

                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("توقيع واستلام العميل:").Bold();
                            c.Item().Text("....................................");
                        });
                    });
                });
            });
        });

        return doc.GeneratePdf();
    }

    // 18. Sales Returns PDF
    public byte[] GenerateSalesReturnsPdf(List<SalesReturnDto> returns, SalesReturnFilterDto? filter = null)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ApplyStandardPageLayout(page, "سجل مرتجعات المبيعات");
                page.Content().PaddingTop(6).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(85);
                            columns.ConstantColumn(85);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("رقم المرتجع").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("رقم الفاتورة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("العميل").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("مخزن الاستلام").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("تاريخ المرتجع").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("قيمة المرتجع").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("السبب").Bold();
                        });

                        foreach (var r in returns)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(r.ReturnNumber);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(r.SalesInvoiceNumber);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(r.CustomerName).Bold();
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(r.WarehouseName);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(r.ReturnDateFormatted);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(r.TotalAmountFormatted).Bold();
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(r.Reason);
                        }
                    });

                    col.Item().PaddingTop(8).AlignLeft().Text($"إجمالي قيمة المرتجعات: {ArabicLocalizationHelper.FormatCurrency(returns.Sum(r => r.TotalAmount))}").Bold();
                });
            });
        });

        return doc.GeneratePdf();
    }

    // 19. Supplier Payments PDF
    public byte[] GenerateSupplierPaymentsPdf(List<SupplierPaymentDto> payments, PaymentFilterDto? filter = null)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ApplyStandardPageLayout(page, "سجل سندات الصرف وسداد دفعات الموردين");
                page.Content().PaddingTop(6).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(85);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("رقم السند").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("المورد المستفيد").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الخزينة / الحساب البنكي").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("تاريخ السداد").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("طريقة الدفع").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("المبلغ المسدد").Bold();
                        });

                        foreach (var p in payments)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(p.PaymentNumber);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(p.SupplierName).Bold();
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(p.TreasuryName);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(p.PaymentDateFormatted);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(p.PaymentMethodArabic);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(p.AmountFormatted).Bold();
                        }
                    });

                    col.Item().PaddingTop(8).AlignLeft().Text($"إجمالي المدفوع للموردين: {ArabicLocalizationHelper.FormatCurrency(payments.Sum(p => p.Amount))}").Bold();
                });
            });
        });

        return doc.GeneratePdf();
    }

    // 20. Customer Payments PDF
    public byte[] GenerateCustomerPaymentsPdf(List<CustomerPaymentDto> payments, PaymentFilterDto? filter = null)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ApplyStandardPageLayout(page, "سجل سندات القبض والتحصيلات النقدية من العملاء");
                page.Content().PaddingTop(6).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(85);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(3);
                            columns.ConstantColumn(85);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("رقم السند").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("اسم العميل").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الخزينة المودع بها").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("رقم الفاتورة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("تاريخ القبض").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("طريقة الدفع").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("المبلغ المحصل").Bold();
                        });

                        foreach (var p in payments)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(p.ReceiptNumber);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(p.CustomerName).Bold();
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(p.TreasuryName);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(p.SalesInvoiceNumber ?? "-");
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(p.PaymentDateFormatted);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(p.PaymentMethodArabic);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(p.AmountFormatted).Bold();
                        }
                    });

                    col.Item().PaddingTop(8).AlignLeft().Text($"إجمالي المقبوض من العملاء: {ArabicLocalizationHelper.FormatCurrency(payments.Sum(p => p.Amount))}").Bold();
                });
            });
        });

        return doc.GeneratePdf();
    }

    // 21. Treasury Transactions PDF
    public byte[] GenerateTreasuryTransactionsPdf(List<TreasuryTransactionDto> transactions, TreasuryFilterDto? filter = null)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ApplyStandardPageLayout(page, "كشف حساب حركات الخزينة والحسابات البنكية");
                page.Content().PaddingTop(6).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(85);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("رقم الحركة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("التاريخ").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الخزينة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("نوع الحركة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("التصنيف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("المبلغ").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("رقم المرجع").Bold();
                        });

                        foreach (var t in transactions)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(t.TransactionNumber);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(t.TransactionDateFormatted);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(t.TreasuryName);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(t.TransactionTypeArabic);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(t.CategoryArabic);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(t.AmountFormatted).FontColor(t.TransactionType == Domain.Enums.TreasuryTransactionType.Inflow ? Colors.Green.Darken2 : Colors.Red.Darken2).Bold();
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(t.ReferenceNumber);
                        }
                    });
                });
            });
        });

        return doc.GeneratePdf();
    }

    // 22. Operating Expenses PDF
    public byte[] GenerateExpensesPdf(List<OperatingExpenseDto> expenses, ExpenseFilterDto? filter = null)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ApplyStandardPageLayout(page, "سجل المصروفات التشغيلية والنثرية");
                page.Content().PaddingTop(6).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(85);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("رقم السند").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("التاريخ").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("بند المصروف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("بيان ووصف المصروف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("المدفوع له").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الخزينة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("المبلغ").Bold();
                        });

                        foreach (var e in expenses)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(e.ExpenseNumber);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(e.ExpenseDateFormatted);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(e.Category).Bold();
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(e.Description);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(e.PaidTo ?? "-");
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(e.TreasuryName);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(e.AmountFormatted).Bold();
                        }
                    });

                    col.Item().PaddingTop(8).AlignLeft().Text($"إجمالي المصروفات التشغيلية: {ArabicLocalizationHelper.FormatCurrency(expenses.Sum(e => e.Amount))}").Bold();
                });
            });
        });

        return doc.GeneratePdf();
    }

    // 23. Purchases Report PDF
    public byte[] GeneratePurchasesReportPdf(PurchasesReportDto report)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ApplyStandardPageLayout(page, "تقرير المشتريات والتوريدات", $"الفترة من: {ArabicLocalizationHelper.FormatDate(report.PeriodStart)} إلى: {ArabicLocalizationHelper.FormatDate(report.PeriodEnd)}");
                page.Content().PaddingTop(6).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(6).Column(c =>
                        {
                            c.Item().Text("إجمالي عدد الأوامر").FontSize(8).FontColor(Colors.Grey.Darken1);
                            c.Item().Text(report.TotalOrders.ToString()).Bold().FontSize(12);
                        });
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(6).Column(c =>
                        {
                            c.Item().Text("إجمالي المشتريات").FontSize(8).FontColor(Colors.Grey.Darken1);
                            c.Item().Text(report.TotalPurchasesAmountFormatted).Bold().FontSize(12);
                        });
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(6).Column(c =>
                        {
                            c.Item().Text("إجمالي الخصومات").FontSize(8).FontColor(Colors.Grey.Darken1);
                            c.Item().Text(ArabicLocalizationHelper.FormatCurrency(report.TotalDiscounts)).Bold().FontSize(12);
                        });
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(6).Column(c =>
                        {
                            c.Item().Text("صافي المشتريات").FontSize(8).FontColor(Colors.Grey.Darken1);
                            c.Item().Text(report.TotalNetPurchasesFormatted).Bold().FontSize(12).FontColor(Colors.Green.Darken3);
                        });
                    });

                    col.Item().PaddingVertical(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(90);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("رقم الأمر").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("المورد").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("تاريخ التوريد").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("حالة الأمر").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("القيمة الإجمالية").Bold();
                        });

                        foreach (var item in report.Items)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.OrderNumber);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.SupplierName).Bold();
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.OrderDateFormatted);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.StatusArabic);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.TotalAmountFormatted).Bold();
                        }
                    });
                });
            });
        });

        return doc.GeneratePdf();
    }

    // 24. Sales Report PDF
    public byte[] GenerateSalesReportPdf(SalesReportDto report)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ApplyStandardPageLayout(page, "تقرير المبيعات والربحية", $"الفترة من: {ArabicLocalizationHelper.FormatDate(report.PeriodStart)} إلى: {ArabicLocalizationHelper.FormatDate(report.PeriodEnd)}");
                page.Content().PaddingTop(6).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(6).Column(c =>
                        {
                            c.Item().Text("إجمالي المبيعات").FontSize(8).FontColor(Colors.Grey.Darken1);
                            c.Item().Text(report.TotalSalesAmountFormatted).Bold().FontSize(12).FontColor(Colors.Blue.Darken3);
                        });
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(6).Column(c =>
                        {
                            c.Item().Text("المحصل نقداً").FontSize(8).FontColor(Colors.Grey.Darken1);
                            c.Item().Text(ArabicLocalizationHelper.FormatCurrency(report.TotalPaidAmount)).Bold().FontSize(12).FontColor(Colors.Green.Darken3);
                        });
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(6).Column(c =>
                        {
                            c.Item().Text("المتبقي آجل").FontSize(8).FontColor(Colors.Grey.Darken1);
                            c.Item().Text(ArabicLocalizationHelper.FormatCurrency(report.TotalRemainingAmount)).Bold().FontSize(12).FontColor(Colors.Red.Darken2);
                        });
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(6).Column(c =>
                        {
                            c.Item().Text("مجمل أرباح المبيعات").FontSize(8).FontColor(Colors.Grey.Darken1);
                            c.Item().Text(report.TotalGrossProfitFormatted).Bold().FontSize(12).FontColor(Colors.Green.Darken3);
                        });
                    });

                    col.Item().PaddingVertical(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(85);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("رقم الفاتورة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("العميل").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("التاريخ").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الإجمالي").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("المحصل").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("المتبقي").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("مجمل الربح").Bold();
                        });

                        foreach (var item in report.Items)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.InvoiceNumber);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.CustomerName).Bold();
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.InvoiceDateFormatted);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(ArabicLocalizationHelper.FormatCurrency(item.TotalAmount));
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(ArabicLocalizationHelper.FormatCurrency(item.PaidAmount));
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(ArabicLocalizationHelper.FormatCurrency(item.RemainingAmount));
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(ArabicLocalizationHelper.FormatCurrency(item.GrossProfit)).FontColor(item.GrossProfit >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2).Bold();
                        }
                    });
                });
            });
        });

        return doc.GeneratePdf();
    }

    // 25. Inventory Valuation Report PDF
    public byte[] GenerateInventoryReportPdf(InventoryReportDto report)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ApplyStandardPageLayout(page, "تقرير الجرد المخزني والتقييم المالي للأصناف");
                page.Content().PaddingTop(6).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(6).Column(c =>
                        {
                            c.Item().Text("عدد الأصناف بالمخازن").FontSize(8).FontColor(Colors.Grey.Darken1);
                            c.Item().Text(report.TotalProducts.ToString()).Bold().FontSize(12);
                        });
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(6).Column(c =>
                        {
                            c.Item().Text("إجمالي كميات المخزون").FontSize(8).FontColor(Colors.Grey.Darken1);
                            c.Item().Text(ArabicLocalizationHelper.FormatQuantity(report.TotalStockQuantity)).Bold().FontSize(12);
                        });
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(6).Column(c =>
                        {
                            c.Item().Text("إجمالي القيمة المالية للمخزون").FontSize(8).FontColor(Colors.Grey.Darken1);
                            c.Item().Text(report.TotalStockValuationFormatted).Bold().FontSize(12).FontColor(Colors.Green.Darken3);
                        });
                    });

                    col.Item().PaddingVertical(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(80);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.ConstantColumn(50);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("كود الصنف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("اسم الصنف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("التصنيف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الوحدة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الرصيد").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("متوسط التكلفة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("إجمالي القيمة").Bold();
                        });

                        foreach (var item in report.Items)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.ProductCode);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.ProductName).Bold();
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.CategoryName);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.Unit);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(ArabicLocalizationHelper.FormatQuantity(item.Quantity));
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.AverageCostFormatted);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.TotalValuationFormatted).Bold();
                        }
                    });
                });
            });
        });

        return doc.GeneratePdf();
    }

    // 26. Warehouse Stock Report PDF
    public byte[] GenerateWarehouseStockReportPdf(WarehouseStockReportDto report)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ApplyStandardPageLayout(page, $"تقرير جرد مخزن {report.WarehouseName}", $"كود المخزن: {report.WarehouseCode}");
                page.Content().PaddingTop(6).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(80);
                            columns.RelativeColumn(3);
                            columns.ConstantColumn(50);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("كود الصنف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("اسم الصنف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الوحدة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الرصيد الفعلي").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("المتاح للصرف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("متوسط التكلفة").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("قيمة المخزون").Bold();
                        });

                        foreach (var item in report.Items)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.ProductCode);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.ProductName).Bold();
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.Unit);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(ArabicLocalizationHelper.FormatQuantity(item.Quantity));
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(ArabicLocalizationHelper.FormatQuantity(item.AvailableQuantity));
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(ArabicLocalizationHelper.FormatCurrency(item.AverageCost));
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(ArabicLocalizationHelper.FormatCurrency(item.TotalValuation)).Bold();
                        }
                    });

                    col.Item().PaddingTop(8).AlignLeft().Text($"إجمالي قيمة بضاعة المخزن: {report.TotalValuationFormatted}").Bold();
                });
            });
        });

        return doc.GeneratePdf();
    }

    // 27. Supplier Balances Report PDF
    public byte[] GenerateSupplierBalancesReportPdf(SupplierBalancesReportDto report)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ApplyStandardPageLayout(page, "كشف أرصدة الموردين المستحقة (دائن)");
                page.Content().PaddingTop(6).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(80);
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(3);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("كود المورد").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("اسم المورد").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("رقم الهاتف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الرصيد الدائن المستحق").Bold();
                        });

                        foreach (var item in report.Items)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.SupplierCode);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.SupplierName).Bold();
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.Phone);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.BalanceFormatted).FontColor(item.Balance > 0 ? Colors.Red.Darken2 : Colors.Grey.Darken3).Bold();
                        }
                    });

                    col.Item().PaddingTop(8).AlignLeft().Text($"إجمالي المديونية المستحقة للموردين: {report.TotalOutstandingDebtFormatted}").Bold().FontColor(Colors.Red.Darken2);
                });
            });
        });

        return doc.GeneratePdf();
    }

    // 28. Customer Balances Report PDF
    public byte[] GenerateCustomerBalancesReportPdf(CustomerBalancesReportDto report)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ApplyStandardPageLayout(page, "كشف أرصدة ومديونيات العملاء (مدين)");
                page.Content().PaddingTop(6).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(80);
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(3);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("كود العميل").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("اسم العميل").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("رقم الهاتف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("الرصيد المدين المستحق").Bold();
                        });

                        foreach (var item in report.Items)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.CustomerCode);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.CustomerName).Bold();
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.Phone);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(item.BalanceFormatted).FontColor(item.Balance > 0 ? Colors.Red.Darken2 : Colors.Grey.Darken3).Bold();
                        }
                    });

                    col.Item().PaddingTop(8).AlignLeft().Text($"إجمالي مستحقات الشركة طرف العملاء: {report.TotalOutstandingReceivablesFormatted}").Bold().FontColor(Colors.Red.Darken2);
                });
            });
        });

        return doc.GeneratePdf();
    }

    // 29. Expenses Report PDF
    public byte[] GenerateExpensesReportPdf(ExpensesReportDto report)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ApplyStandardPageLayout(page, "تقرير المصروفات التشغيلية والتحليل النوعي", $"الفترة من: {ArabicLocalizationHelper.FormatDate(report.PeriodStart)} إلى: {ArabicLocalizationHelper.FormatDate(report.PeriodEnd)}");
                page.Content().PaddingTop(6).Column(col =>
                {
                    col.Item().Text("توزيع المصروفات حسب البند:").Bold().FontSize(10);
                    col.Item().PaddingVertical(4).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("بند المصروف").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("إجمالي المبلغ").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("النسبة").Bold();
                        });

                        foreach (var b in report.CategoryBreakdown)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(b.Category);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(b.TotalAmountFormatted).Bold();
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).Text($"{b.Percentage}%");
                        }
                    });

                    col.Item().PaddingTop(8).Text("تفاصيل حركات الصرف:").Bold().FontSize(10);
                    col.Item().PaddingVertical(4).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(80);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("رقم السند").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("التاريخ").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("البند").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("البيان").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("المبلغ").Bold();
                        });

                        foreach (var item in report.Items)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(item.ExpenseNumber);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(item.ExpenseDateFormatted);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(item.Category);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(item.Description);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(item.AmountFormatted).Bold();
                        }
                    });

                    col.Item().PaddingTop(6).AlignLeft().Text($"إجمالي المصروفات التشغيلية: {report.TotalExpensesAmountFormatted}").Bold().FontSize(11).FontColor(Colors.Red.Darken2);
                });
            });
        });

        return doc.GeneratePdf();
    }

    // 30. Profit and Loss Report PDF
    public byte[] GenerateProfitLossReportPdf(ProfitLossReportDto report)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ApplyStandardPageLayout(page, "قائمة الأرباح والخسائر والنتائج التشغيلية", $"الفترة من: {ArabicLocalizationHelper.FormatDate(report.PeriodStart)} إلى: {ArabicLocalizationHelper.FormatDate(report.PeriodEnd)}");
                page.Content().PaddingTop(15).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(5);
                            columns.RelativeColumn(3);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(6).Text("البند المحاسبي").Bold().FontSize(11);
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(6).Text("القيمة المالية").Bold().FontSize(11);
                        });

                        // 1. إيراد المبيعات
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6).Text("إيرادات المبيعات المحققة").Bold();
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(report.TotalRevenueFormatted).Bold().FontColor(Colors.Blue.Darken3);

                        // 2. تكلفة البضاعة المباعة
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6).Text("يخصم: تكلفة البضاعة المباعة (متوسط التكلفة المرجح COGS)");
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6).Text($"({report.TotalCOGSFormatted})").FontColor(Colors.Red.Darken2);

                        // 3. مجمل الربح
                        table.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("مجمل الربح التجاري (Gross Profit)").Bold();
                        table.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text(report.GrossProfitFormatted).Bold().FontColor(report.GrossProfit >= 0 ? Colors.Green.Darken3 : Colors.Red.Darken2);

                        // 4. المصروفات التشغيلية
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6).Text("يخصم: إجمالي المصروفات التشغيلية والنقل والإدارة");
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6).Text($"({report.OperatingExpensesFormatted})").FontColor(Colors.Red.Darken2);

                        // 5. صافي الربح
                        table.Cell().Background(Colors.Green.Lighten4).Padding(8).Text("صافي الأرباح التشغيلية (Net Profit)").Bold().FontSize(12);
                        table.Cell().Background(Colors.Green.Lighten4).Padding(8).Text(report.NetProfitFormatted).Bold().FontSize(12).FontColor(report.NetProfit >= 0 ? Colors.Green.Darken4 : Colors.Red.Darken3);

                        // 6. هامش الربح
                        table.Cell().Padding(6).Text("هامش صافي الربح").Bold();
                        table.Cell().Padding(6).Text($"{report.ProfitMarginPercentage}%").Bold();
                    });

                    col.Item().PaddingTop(25).Text("ملاحظة: يتم احتساب تكلفة البضاعة المباعة بدقة وفق طريقة متوسط التكلفة المرجح (Moving Weighted Average Cost) المعتمدة في النظام.").FontSize(8).FontColor(Colors.Grey.Darken2);
                });
            });
        });

        return doc.GeneratePdf();
    }
}
