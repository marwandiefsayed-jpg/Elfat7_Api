using Elfat7.Domain.Common;
using Elfat7.Domain.Enums;

namespace Elfat7.Domain.Entities;

public class StockTransfer : BaseEntity
{
    public string TransferNumber { get; set; } = string.Empty;
    public int SourceWarehouseId { get; set; }
    public Warehouse? SourceWarehouse { get; set; }
    public int DestinationWarehouseId { get; set; }
    public Warehouse? DestinationWarehouse { get; set; }
    
    public DateTime TransferDate { get; set; } = DateTime.UtcNow;
    public TransferStatus Status { get; set; } = TransferStatus.Draft;
    public string? Notes { get; set; }

    public ICollection<StockTransferItem> Items { get; set; } = new List<StockTransferItem>();
}

public class StockTransferItem : BaseEntity
{
    public int StockTransferId { get; set; }
    public StockTransfer? StockTransfer { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
}
