using Elfat7.Domain.Enums;
using System.Globalization;

namespace Elfat7.Application.Common;

public static class ArabicLocalizationHelper
{
    private static readonly CultureInfo ArabicCulture = new("ar-EG");

    public static string FormatCurrency(decimal amount)
        => $"{amount:N2} ج.م";

    public static string FormatQuantity(decimal quantity, string unit = "")
        => string.IsNullOrWhiteSpace(unit) ? $"{quantity:G29}" : $"{quantity:G29} {unit}";

    public static string FormatDate(DateTime date)
        => date.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);

    public static string FormatDateTime(DateTime dateTime)
        => dateTime.ToString("yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture);

    public static string ToArabic(this PurchaseOrderStatus status) => status switch
    {
        PurchaseOrderStatus.Draft => "مسودة",
        PurchaseOrderStatus.Confirmed => "مؤكد",
        PurchaseOrderStatus.PartiallyReceived => "مستلم جزئياً",
        PurchaseOrderStatus.FullyReceived => "مستلم بالكامل",
        PurchaseOrderStatus.Cancelled => "ملغي",
        _ => "غير محدد"
    };

    public static string ToArabic(this SalesOrderStatus status) => status switch
    {
        SalesOrderStatus.Draft => "مسودة",
        SalesOrderStatus.Confirmed => "مؤكد",
        SalesOrderStatus.PartiallyDelivered => "مسلم جزئياً",
        SalesOrderStatus.FullyDelivered => "مكتمل التسليم",
        SalesOrderStatus.Cancelled => "ملغي",
        _ => "غير محدد"
    };

    public static string ToArabic(this SalesInvoiceStatus status) => status switch
    {
        SalesInvoiceStatus.Draft => "مسودة",
        SalesInvoiceStatus.Confirmed => "معتمدة",
        SalesInvoiceStatus.PartiallyDelivered => "مسلمة جزئياً",
        SalesInvoiceStatus.FullyDelivered => "مكتملة التسليم",
        SalesInvoiceStatus.Cancelled => "ملغاة",
        _ => "غير محدد"
    };

    public static string ToArabic(this InvoicePaymentStatus status) => status switch
    {
        InvoicePaymentStatus.Unpaid => "غير مسددة",
        InvoicePaymentStatus.PartiallyPaid => "مسددة جزئياً",
        InvoicePaymentStatus.FullyPaid => "مسددة بالكامل",
        _ => "غير محدد"
    };

    public static string ToArabic(this DeliveryStatus status) => status switch
    {
        DeliveryStatus.Pending => "قيد التجهيز",
        DeliveryStatus.Dispatched => "خرج للتسليم",
        DeliveryStatus.Delivered => "تم التسليم",
        DeliveryStatus.Cancelled => "ملغي",
        _ => "غير محدد"
    };

    public static string ToArabic(this StockMovementType type) => type switch
    {
        StockMovementType.PurchaseReceipt => "إذن استلام مشتريات",
        StockMovementType.SalesDelivery => "إذن صرف مبيعات",
        StockMovementType.TransferOut => "تحويل مخزني صادر",
        StockMovementType.TransferIn => "تحويل مخزني وارد",
        StockMovementType.AdjustmentIncrease => "تسوية جردية (إضافة)",
        StockMovementType.AdjustmentDecrease => "تسوية جردية (خصم)",
        StockMovementType.SalesReturn => "مرتجع مبيعات",
        _ => "حركة مخزون"
    };

    public static string ToArabic(this PaymentMethod method) => method switch
    {
        PaymentMethod.Cash => "نقدي",
        PaymentMethod.BankTransfer => "تحويل بنكي",
        PaymentMethod.Check => "شيك",
        _ => "نقدي"
    };

    public static string ToArabic(this TreasuryTransactionType type) => type switch
    {
        TreasuryTransactionType.Inflow => "إيداع / وارد",
        TreasuryTransactionType.Outflow => "صرف / صادر",
        _ => "حركة مالية"
    };

    public static string ToArabic(this TreasuryTransactionCategory category) => category switch
    {
        TreasuryTransactionCategory.CustomerPayment => "تحصيل من عميل",
        TreasuryTransactionCategory.SupplierPayment => "سداد لمورد",
        TreasuryTransactionCategory.OperatingExpense => "مصروف تشغيلي",
        TreasuryTransactionCategory.CapitalAdjustment => "تسوية نقدية",
        _ => "معاملة مالية"
    };

    public static string ToArabic(this AdjustmentType type) => type switch
    {
        AdjustmentType.Increase => "إضافة",
        AdjustmentType.Decrease => "خصم",
        _ => "تسوية"
    };

    public static string ToArabic(this TransferStatus status) => status switch
    {
        TransferStatus.Draft => "مسودة",
        TransferStatus.Completed => "مكتمل",
        TransferStatus.Cancelled => "ملغي",
        _ => "غير محدد"
    };
}
