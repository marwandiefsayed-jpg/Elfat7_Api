using Elfat7.Domain.Common;
using Elfat7.Domain.Enums;

namespace Elfat7.Domain.Entities;

public class Delivery : BaseEntity
{
    public string DeliveryNumber { get; set; } = string.Empty;
    public int SalesInvoiceId { get; set; }
    public SalesInvoice? SalesInvoice { get; set; }
    public int WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    
    public DateTime DeliveryDate { get; set; } = DateTime.UtcNow;
    public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;
    public string? DriverName { get; set; }
    public string? VehicleNumber { get; set; }
    public string? Notes { get; set; }

    public ICollection<DeliveryItem> Items { get; set; } = new List<DeliveryItem>();
}

public class DeliveryItem : BaseEntity
{
    public int DeliveryId { get; set; }
    public Delivery? Delivery { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    
    public decimal Quantity { get; set; }
}
