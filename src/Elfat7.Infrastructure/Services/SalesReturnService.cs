using Elfat7.Application.Common;
using Elfat7.Application.DTOs.SalesReturns;
using Elfat7.Application.Interfaces;
using Elfat7.Domain.Entities;
using Elfat7.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Elfat7.Infrastructure.Services;

public class SalesReturnService : ISalesReturnService
{
    private readonly IApplicationDbContext _context;

    public SalesReturnService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<SalesReturnDto>> GetSalesReturnsAsync(SalesReturnFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.SalesReturns
            .AsNoTracking()
            .Include(r => r.SalesInvoice)
            .Include(r => r.Customer)
            .Include(r => r.Warehouse)
            .Include(r => r.Items)
            .ThenInclude(i => i.Product)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(r => r.ReturnNumber.Contains(term) ||
                                     (r.Customer != null && r.Customer.Name.Contains(term)) ||
                                     (r.SalesInvoice != null && r.SalesInvoice.InvoiceNumber.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(filter.CustomerCode))
            query = query.Where(r => r.Customer != null && r.Customer.Code == filter.CustomerCode);

        if (!string.IsNullOrWhiteSpace(filter.SalesInvoiceNumber))
            query = query.Where(r => r.SalesInvoice != null && r.SalesInvoice.InvoiceNumber == filter.SalesInvoiceNumber);

        if (filter.StartDate.HasValue)
            query = query.Where(r => r.ReturnDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(r => r.ReturnDate <= filter.EndDate.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(r => r.Id)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(r => new SalesReturnDto
            {
                ReturnNumber = r.ReturnNumber,
                SalesInvoiceNumber = r.SalesInvoice != null ? r.SalesInvoice.InvoiceNumber : "",
                CustomerCode = r.Customer != null ? r.Customer.Code : "",
                CustomerName = r.Customer != null ? r.Customer.Name : "",
                WarehouseCode = r.Warehouse != null ? r.Warehouse.Code : "",
                WarehouseName = r.Warehouse != null ? r.Warehouse.Name : "",
                ReturnDate = r.ReturnDate,
                TotalAmount = r.TotalAmount,
                Reason = r.Reason,
                Notes = r.Notes,
                Items = r.Items.Select(i => new SalesReturnItemDto
                {
                    ProductCode = i.Product != null ? i.Product.Code : "",
                    ProductName = i.Product != null ? i.Product.Name : "",
                    Unit = i.Product != null ? i.Product.Unit : "",
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.LineTotal
                }).ToList()
            })
            .ToListAsync(ct);

        return new PagedResult<SalesReturnDto>(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<List<SalesReturnDto>> GetAllFilteredSalesReturnsAsync(SalesReturnFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.SalesReturns
            .AsNoTracking()
            .Include(r => r.SalesInvoice)
            .Include(r => r.Customer)
            .Include(r => r.Warehouse)
            .Include(r => r.Items)
            .ThenInclude(i => i.Product)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(r => r.ReturnNumber.Contains(term) ||
                                     (r.Customer != null && r.Customer.Name.Contains(term)) ||
                                     (r.SalesInvoice != null && r.SalesInvoice.InvoiceNumber.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(filter.CustomerCode))
            query = query.Where(r => r.Customer != null && r.Customer.Code == filter.CustomerCode);

        if (!string.IsNullOrWhiteSpace(filter.SalesInvoiceNumber))
            query = query.Where(r => r.SalesInvoice != null && r.SalesInvoice.InvoiceNumber == filter.SalesInvoiceNumber);

        if (filter.StartDate.HasValue)
            query = query.Where(r => r.ReturnDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(r => r.ReturnDate <= filter.EndDate.Value);

        return await query
            .OrderByDescending(r => r.Id)
            .Select(r => new SalesReturnDto
            {
                ReturnNumber = r.ReturnNumber,
                SalesInvoiceNumber = r.SalesInvoice != null ? r.SalesInvoice.InvoiceNumber : "",
                CustomerCode = r.Customer != null ? r.Customer.Code : "",
                CustomerName = r.Customer != null ? r.Customer.Name : "",
                WarehouseCode = r.Warehouse != null ? r.Warehouse.Code : "",
                WarehouseName = r.Warehouse != null ? r.Warehouse.Name : "",
                ReturnDate = r.ReturnDate,
                TotalAmount = r.TotalAmount,
                Reason = r.Reason,
                Notes = r.Notes,
                Items = r.Items.Select(i => new SalesReturnItemDto
                {
                    ProductCode = i.Product != null ? i.Product.Code : "",
                    ProductName = i.Product != null ? i.Product.Name : "",
                    Unit = i.Product != null ? i.Product.Unit : "",
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.LineTotal
                }).ToList()
            })
            .ToListAsync(ct);
    }

    public async Task<SalesReturnDto?> GetSalesReturnByNumberAsync(string returnNumber, CancellationToken ct = default)
    {
        var ret = await _context.SalesReturns
            .AsNoTracking()
            .Include(r => r.SalesInvoice)
            .Include(r => r.Customer)
            .Include(r => r.Warehouse)
            .Include(r => r.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(r => r.ReturnNumber == returnNumber, ct);

        if (ret == null) return null;

        return new SalesReturnDto
        {
            ReturnNumber = ret.ReturnNumber,
            SalesInvoiceNumber = ret.SalesInvoice?.InvoiceNumber ?? "",
            CustomerCode = ret.Customer?.Code ?? "",
            CustomerName = ret.Customer?.Name ?? "",
            WarehouseCode = ret.Warehouse?.Code ?? "",
            WarehouseName = ret.Warehouse?.Name ?? "",
            ReturnDate = ret.ReturnDate,
            TotalAmount = ret.TotalAmount,
            Reason = ret.Reason,
            Notes = ret.Notes,
            Items = ret.Items.Select(i => new SalesReturnItemDto
            {
                ProductCode = i.Product?.Code ?? "",
                ProductName = i.Product?.Name ?? "",
                Unit = i.Product?.Unit ?? "",
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                LineTotal = i.LineTotal
            }).ToList()
        };
    }

    public async Task<Result<SalesReturnDto>> CreateSalesReturnAsync(CreateSalesReturnDto dto, CancellationToken ct = default)
    {
        using var transaction = await _context.BeginTransactionAsync(ct);

        var invoice = await _context.SalesInvoices
            .Include(i => i.Items)
            .Include(i => i.Customer)
            .FirstOrDefaultAsync(i => i.InvoiceNumber == dto.SalesInvoiceNumber, ct);

        if (invoice == null) return Result<SalesReturnDto>.Failure("فاتورة المبيعات الأصلية غير موجودة");

        var warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.Code == dto.WarehouseCode, ct);
        if (warehouse == null) return Result<SalesReturnDto>.Failure("مخزن استلام المرتجع غير موجود");

        var count = await _context.SalesReturns.CountAsync(ct);
        var returnNumber = $"RET-{DateTime.UtcNow.Year}-{(count + 1):D4}";

        var salesReturn = new SalesReturn
        {
            ReturnNumber = returnNumber,
            SalesInvoiceId = invoice.Id,
            CustomerId = invoice.CustomerId,
            WarehouseId = warehouse.Id,
            ReturnDate = dto.ReturnDate ?? DateTime.UtcNow,
            Reason = dto.Reason.Trim(),
            Notes = dto.Notes?.Trim()
        };

        decimal totalReturnAmount = 0;
        var returnItems = new List<SalesReturnItem>();
        var stockMovements = new List<StockMovement>();

        foreach (var item in dto.Items)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Code == item.ProductCode, ct);
            if (product == null) return Result<SalesReturnDto>.Failure($"الصنف {item.ProductCode} غير موجود");

            var invoiceItem = invoice.Items.FirstOrDefault(i => i.ProductId == product.Id);
            if (invoiceItem == null) return Result<SalesReturnDto>.Failure($"الصنف {product.Name} غير موجود في فاتورة المبيعات");

            if (item.Quantity > invoiceItem.DeliveredQuantity)
            {
                return Result<SalesReturnDto>.Failure($"الكمية المرتجعة ({item.Quantity}) تتجاوز الكمية المسلمة بالفعل ({invoiceItem.DeliveredQuantity}) للصنف {product.Name}");
            }

            var lineTotal = item.Quantity * invoiceItem.UnitPrice;
            totalReturnAmount += lineTotal;

            returnItems.Add(new SalesReturnItem
            {
                SalesReturn = salesReturn,
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = invoiceItem.UnitPrice,
                LineTotal = lineTotal
            });

            // 1. إعادة إدخال البضاعة إلى المخزن
            var stock = await _context.WarehouseStocks
                .FirstOrDefaultAsync(ws => ws.WarehouseId == warehouse.Id && ws.ProductId == product.Id, ct);

            if (stock == null)
            {
                stock = new WarehouseStock
                {
                    WarehouseId = warehouse.Id,
                    ProductId = product.Id,
                    Quantity = 0,
                    ReservedQuantity = 0,
                    LastUpdated = DateTime.UtcNow
                };
                await _context.WarehouseStocks.AddAsync(stock, ct);
            }

            stock.Quantity += item.Quantity;
            stock.LastUpdated = DateTime.UtcNow;

            // 2. تسجيل حركة المخزون
            var movCount = await _context.StockMovements.CountAsync(ct);
            stockMovements.Add(new StockMovement
            {
                MovementNumber = $"MOV-{DateTime.UtcNow.Year}-{(movCount + stockMovements.Count + 1):D5}",
                WarehouseId = warehouse.Id,
                ProductId = product.Id,
                MovementType = StockMovementType.SalesReturn,
                Quantity = item.Quantity,
                UnitCost = invoiceItem.UnitCost,
                TotalCost = item.Quantity * invoiceItem.UnitCost,
                ReferenceNumber = returnNumber,
                Notes = $"مرتجع مبيعات بموجب فاتورة {invoice.InvoiceNumber}: {dto.Reason}"
            });
        }

        salesReturn.TotalAmount = totalReturnAmount;
        salesReturn.Items = returnItems;

        // 3. تخفيض مديونية العميل بقيمة المرتجع
        if (invoice.Customer != null)
        {
            invoice.Customer.Balance -= totalReturnAmount;
        }

        await _context.SalesReturns.AddAsync(salesReturn, ct);
        await _context.StockMovements.AddRangeAsync(stockMovements, ct);

        await _context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return Result<SalesReturnDto>.Success(new SalesReturnDto
        {
            ReturnNumber = salesReturn.ReturnNumber,
            SalesInvoiceNumber = invoice.InvoiceNumber,
            CustomerCode = invoice.Customer?.Code ?? "",
            CustomerName = invoice.Customer?.Name ?? "",
            WarehouseCode = warehouse.Code,
            WarehouseName = warehouse.Name,
            ReturnDate = salesReturn.ReturnDate,
            TotalAmount = salesReturn.TotalAmount,
            Reason = salesReturn.Reason,
            Notes = salesReturn.Notes
        }, "تم تسجيل مرتجع المبيعات وإعادة البضاعة للمخزن وتخفيض رصيد مديونية العميل بنجاح");
    }
}
