using Elfat7.Application.Common;
using Elfat7.Application.DTOs.Sales;
using Elfat7.Application.Interfaces;
using Elfat7.Domain.Entities;
using Elfat7.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Elfat7.Infrastructure.Services;

public class SalesOrderService : ISalesOrderService
{
    private readonly IApplicationDbContext _context;

    public SalesOrderService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<SalesOrderDto>> GetSalesOrdersAsync(SalesOrderFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.SalesOrders
            .AsNoTracking()
            .Include(so => so.Customer)
            .Include(so => so.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(so => so.OrderNumber.Contains(term) || (so.Customer != null && so.Customer.Name.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(filter.CustomerCode))
            query = query.Where(so => so.Customer != null && so.Customer.Code == filter.CustomerCode);

        if (filter.Status.HasValue)
            query = query.Where(so => so.Status == filter.Status.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(so => so.OrderDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(so => so.OrderDate <= filter.EndDate.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(so => so.Id)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(so => new SalesOrderDto
            {
                OrderNumber = so.OrderNumber,
                CustomerCode = so.Customer != null ? so.Customer.Code : "",
                CustomerName = so.Customer != null ? so.Customer.Name : "",
                OrderDate = so.OrderDate,
                Status = so.Status,
                SubTotal = so.SubTotal,
                DiscountAmount = so.DiscountAmount,
                TotalAmount = so.TotalAmount
            })
            .ToListAsync(ct);

        return new PagedResult<SalesOrderDto>(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<List<SalesOrderDto>> GetAllFilteredSalesOrdersAsync(SalesOrderFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.SalesOrders
            .AsNoTracking()
            .Include(so => so.Customer)
            .Include(so => so.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(so => so.OrderNumber.Contains(term) || (so.Customer != null && so.Customer.Name.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(filter.CustomerCode))
            query = query.Where(so => so.Customer != null && so.Customer.Code == filter.CustomerCode);

        if (filter.Status.HasValue)
            query = query.Where(so => so.Status == filter.Status.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(so => so.OrderDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(so => so.OrderDate <= filter.EndDate.Value);

        return await query
            .OrderByDescending(so => so.Id)
            .Select(so => new SalesOrderDto
            {
                OrderNumber = so.OrderNumber,
                CustomerCode = so.Customer != null ? so.Customer.Code : "",
                CustomerName = so.Customer != null ? so.Customer.Name : "",
                OrderDate = so.OrderDate,
                Status = so.Status,
                SubTotal = so.SubTotal,
                DiscountAmount = so.DiscountAmount,
                TotalAmount = so.TotalAmount
            })
            .ToListAsync(ct);
    }

    public async Task<SalesOrderDetailsDto?> GetSalesOrderByNumberAsync(string orderNumber, CancellationToken ct = default)
    {
        var so = await _context.SalesOrders
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(s => s.OrderNumber == orderNumber, ct);

        if (so == null) return null;

        return new SalesOrderDetailsDto
        {
            OrderNumber = so.OrderNumber,
            CustomerCode = so.Customer?.Code ?? "",
            CustomerName = so.Customer?.Name ?? "",
            OrderDate = so.OrderDate,
            Status = so.Status,
            SubTotal = so.SubTotal,
            DiscountAmount = so.DiscountAmount,
            TotalAmount = so.TotalAmount,
            Notes = so.Notes,
            Items = so.Items.Select(i => new SalesOrderItemDto
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

    public async Task<Result<SalesOrderDto>> CreateSalesOrderAsync(CreateSalesOrderDto dto, CancellationToken ct = default)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Code == dto.CustomerCode, ct);
        if (customer == null) return Result<SalesOrderDto>.Failure("العميل المحدد غير موجود");

        var count = await _context.SalesOrders.CountAsync(ct);
        var orderNumber = $"SO-{DateTime.UtcNow.Year}-{(count + 1):D4}";

        decimal subTotal = 0;
        var items = new List<SalesOrderItem>();

        foreach (var item in dto.Items)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Code == item.ProductCode, ct);
            if (product == null) return Result<SalesOrderDto>.Failure($"الصنف {item.ProductCode} غير موجود");

            var lineTotal = item.Quantity * item.UnitPrice;
            subTotal += lineTotal;

            items.Add(new SalesOrderItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                LineTotal = lineTotal
            });
        }

        var totalAmount = subTotal - dto.DiscountAmount;

        var so = new SalesOrder
        {
            OrderNumber = orderNumber,
            CustomerId = customer.Id,
            OrderDate = dto.OrderDate ?? DateTime.UtcNow,
            Status = SalesOrderStatus.Draft,
            SubTotal = subTotal,
            DiscountAmount = dto.DiscountAmount,
            TotalAmount = totalAmount > 0 ? totalAmount : 0,
            Notes = dto.Notes?.Trim(),
            Items = items
        };

        await _context.SalesOrders.AddAsync(so, ct);
        await _context.SaveChangesAsync(ct);

        return Result<SalesOrderDto>.Success(new SalesOrderDto
        {
            OrderNumber = so.OrderNumber,
            CustomerCode = customer.Code,
            CustomerName = customer.Name,
            OrderDate = so.OrderDate,
            Status = so.Status,
            SubTotal = so.SubTotal,
            DiscountAmount = so.DiscountAmount,
            TotalAmount = so.TotalAmount
        }, "تم إنشاء طلب البيع بنجاح");
    }

    public async Task<Result> ConfirmSalesOrderAsync(string orderNumber, CancellationToken ct = default)
    {
        var so = await _context.SalesOrders.FirstOrDefaultAsync(s => s.OrderNumber == orderNumber, ct);
        if (so == null) return Result.Failure("طلب البيع غير موجود");

        if (so.Status != SalesOrderStatus.Draft)
            return Result.Failure("يمكن تأكيد طلبات البيع التي في حالة مسودة فقط");

        so.Status = SalesOrderStatus.Confirmed;
        await _context.SaveChangesAsync(ct);

        return Result.Success("تم تأكيد طلب البيع ويمكن الآن تحويله إلى فاتورة بيع");
    }

    public async Task<Result> CancelSalesOrderAsync(string orderNumber, CancellationToken ct = default)
    {
        var so = await _context.SalesOrders.Include(s => s.SalesInvoices).FirstOrDefaultAsync(s => s.OrderNumber == orderNumber, ct);
        if (so == null) return Result.Failure("طلب البيع غير موجود");

        if (so.SalesInvoices.Any())
            return Result.Failure("لا يمكن إلغاء طلب بيع تم إصدار فواتير بيع مرتبطة به");

        so.Status = SalesOrderStatus.Cancelled;
        await _context.SaveChangesAsync(ct);

        return Result.Success("تم إلغاء طلب البيع بنجاح");
    }
}
