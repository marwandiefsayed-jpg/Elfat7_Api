using Elfat7.Domain.Common;

namespace Elfat7.Domain.Entities;

public class Customer : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? CommercialRegistration { get; set; }
    public decimal CreditLimit { get; set; } = 0; // الحد الائتماني
    public decimal Balance { get; set; } = 0; // الرصيد الحالي (مستحق على العميل مدين)
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
    public ICollection<SalesInvoice> SalesInvoices { get; set; } = new List<SalesInvoice>();
    public ICollection<CustomerPayment> Payments { get; set; } = new List<CustomerPayment>();
    public ICollection<SalesReturn> SalesReturns { get; set; } = new List<SalesReturn>();
}
