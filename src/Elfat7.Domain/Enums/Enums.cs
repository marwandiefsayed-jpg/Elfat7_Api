namespace Elfat7.Domain.Enums;

public enum PurchaseOrderStatus
{
    Draft = 1,              // مسودة
    Confirmed = 2,          // مؤكد
    PartiallyReceived = 3,  // مستلم جزئياً
    FullyReceived = 4,      // مستلم بالكامل
    Cancelled = 5           // ملغي
}

public enum SalesOrderStatus
{
    Draft = 1,              // مسودة
    Confirmed = 2,          // مؤكد
    PartiallyDelivered = 3, // تم تسليم جزء منه
    FullyDelivered = 4,     // مكتمل التسليم
    Cancelled = 5           // ملغي
}

public enum SalesInvoiceStatus
{
    Draft = 1,              // مسودة
    Confirmed = 2,          // معتمدة
    PartiallyDelivered = 3, // مسلمة جزئياً
    FullyDelivered = 4,     // مكتملة التسليم
    Cancelled = 5           // ملغاة
}

public enum InvoicePaymentStatus
{
    Unpaid = 1,         // غير مسددة
    PartiallyPaid = 2,  // مسددة جزئياً
    FullyPaid = 3       // مسددة بالكامل
}

public enum DeliveryStatus
{
    Pending = 1,        // قيد التجهيز
    Dispatched = 2,     // خرج للتسليم
    Delivered = 3,      // تم التسليم
    Cancelled = 4       // ملغي
}

public enum StockMovementType
{
    PurchaseReceipt = 1,    // استلام مشتريات
    SalesDelivery = 2,      // تسليم مبيعات
    TransferOut = 3,        // تحويل صادر
    TransferIn = 4,         // تحويل وارد
    AdjustmentIncrease = 5, // تسوية جردية إضافة
    AdjustmentDecrease = 6, // تسوية جردية خصم
    SalesReturn = 7         // مرتجع مبيعات
}

public enum PaymentMethod
{
    Cash = 1,           // نقدي
    BankTransfer = 2,   // تحويل بنكي
    Check = 3           // شيك
}

public enum TreasuryTransactionType
{
    Inflow = 1,     // وارد / إيداع
    Outflow = 2     // صادر / صرف
}

public enum TreasuryTransactionCategory
{
    CustomerPayment = 1,    // تحصيل عميل
    SupplierPayment = 2,    // سداد مورد
    OperatingExpense = 3,   // مصروف تشغيلي
    CapitalAdjustment = 4   // تسوية نقدية
}

public enum AdjustmentType
{
    Increase = 1,   // إضافة مخزنية
    Decrease = 2    // خصم مخزني
}

public enum TransferStatus
{
    Draft = 1,      // مسودة
    Completed = 2,  // مكتمل
    Cancelled = 3   // ملغي
}
