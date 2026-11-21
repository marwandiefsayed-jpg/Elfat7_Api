using Elfat7.Domain.Common;

namespace Elfat7.Domain.Entities;

public class Supplier : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? CommercialRegistration { get; set; }
    public string? TaxNumber { get; set; }
    public decimal Balance { get; set; } = 0; // الرصيد الحالي (مستحق للمورد دائن)
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    public ICollection<SupplierPayment> Payments { get; set; } = new List<SupplierPayment>();
    public ICollection<GoodsReceipt> GoodsReceipts { get; set; } = new List<GoodsReceipt>();
}
