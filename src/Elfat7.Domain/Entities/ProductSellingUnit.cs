using Elfat7.Domain.Common;

namespace Elfat7.Domain.Entities;

public class ProductSellingUnit : BaseEntity
{
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    /// <summary>
    /// اسم وحدة البيع (مثال: كيلوجرام، شيكارة 50كجم، شيكارة 25كجم، طن، قطعة)
    /// </summary>
    public string UnitName { get; set; } = string.Empty;

    /// <summary>
    /// معامل التحويل للوحدة الأساسية للمخزون (1 وحدة بيع = معامل التحويل * الوحدة الأساسية)
    /// مثال: إذا كانت الوحدة الأساسية "كجم"، والوحدة "شيكارة 50كجم"، فإن المعامل = 50
    /// </summary>
    public decimal ConversionFactor { get; set; } = 1;

    /// <summary>
    /// سعر بيع الوحدة الافتراضي
    /// </summary>
    public decimal SellingPrice { get; set; } = 0;

    /// <summary>
    /// سعر الشراء أو التكلفة التقديرية للوحدة
    /// </summary>
    public decimal PurchasePrice { get; set; } = 0;

    /// <summary>
    /// هل هي وحدة البيع الافتراضية
    /// </summary>
    public bool IsDefault { get; set; } = false;
}
