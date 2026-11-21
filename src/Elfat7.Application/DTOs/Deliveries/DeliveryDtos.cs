using Elfat7.Application.Common;
using Elfat7.Domain.Enums;

namespace Elfat7.Application.DTOs.Deliveries;

public class DeliveryDto
{
    public string DeliveryNumber { get; set; } = string.Empty;
    public string SalesInvoiceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public DateTime DeliveryDate { get; set; }
    public string DeliveryDateFormatted => ArabicLocalizationHelper.FormatDate(DeliveryDate);
    public DeliveryStatus Status { get; set; }
    public string StatusArabic => Status.ToArabic();
    public string? DriverName { get; set; }
    public string? VehicleNumber { get; set; }
    public int ItemsCount { get; set; }
}

public class DeliveryDetailsDto : DeliveryDto
{
    public string? Notes { get; set; }
    public List<DeliveryItemDto> Items { get; set; } = new();
}

public class DeliveryItemDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}

public class CreateDeliveryDto
{
    public string SalesInvoiceNumber { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public DateTime? DeliveryDate { get; set; }
    public string? DriverName { get; set; }
    public string? VehicleNumber { get; set; }
    public string? Notes { get; set; }
    public List<CreateDeliveryItemDto> Items { get; set; } = new();
}

public class CreateDeliveryItemDto
{
    public string ProductCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}

public class DeliveryFilterDto : PagedFilterRequest
{
    public string? SalesInvoiceNumber { get; set; }
    public string? WarehouseCode { get; set; }
    public DeliveryStatus? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
