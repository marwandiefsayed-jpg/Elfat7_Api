using Elfat7.Application.Common;
using Elfat7.Application.DTOs.Suppliers;
using Elfat7.Application.Interfaces;
using Elfat7.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Elfat7.Infrastructure.Services;

public class SupplierService : ISupplierService
{
    private readonly IApplicationDbContext _context;

    public SupplierService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<SupplierDto>> GetSuppliersAsync(SupplierFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.Suppliers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(s => s.Name.Contains(term) || s.Code.Contains(term) || s.Phone.Contains(term));
        }

        if (filter.IsActive.HasValue)
            query = query.Where(s => s.IsActive == filter.IsActive.Value);

        if (filter.MinBalance.HasValue)
            query = query.Where(s => s.Balance >= filter.MinBalance.Value);

        if (filter.MaxBalance.HasValue)
            query = query.Where(s => s.Balance <= filter.MaxBalance.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(s => s.Id)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(s => new SupplierDto
            {
                Code = s.Code,
                Name = s.Name,
                Phone = s.Phone,
                Address = s.Address,
                CommercialRegistration = s.CommercialRegistration,
                TaxNumber = s.TaxNumber,
                Balance = s.Balance,
                Notes = s.Notes,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync(ct);

        return new PagedResult<SupplierDto>(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<List<SupplierDto>> GetAllFilteredSuppliersAsync(SupplierFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.Suppliers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(s => s.Name.Contains(term) || s.Code.Contains(term) || s.Phone.Contains(term));
        }

        if (filter.IsActive.HasValue)
            query = query.Where(s => s.IsActive == filter.IsActive.Value);

        if (filter.MinBalance.HasValue)
            query = query.Where(s => s.Balance >= filter.MinBalance.Value);

        if (filter.MaxBalance.HasValue)
            query = query.Where(s => s.Balance <= filter.MaxBalance.Value);

        return await query
            .OrderByDescending(s => s.Id)
            .Select(s => new SupplierDto
            {
                Code = s.Code,
                Name = s.Name,
                Phone = s.Phone,
                Address = s.Address,
                CommercialRegistration = s.CommercialRegistration,
                TaxNumber = s.TaxNumber,
                Balance = s.Balance,
                Notes = s.Notes,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync(ct);
    }

    public async Task<SupplierDetailsDto?> GetSupplierByCodeAsync(string code, CancellationToken ct = default)
    {
        var supplier = await _context.Suppliers
            .AsNoTracking()
            .Include(s => s.PurchaseOrders.OrderByDescending(po => po.Id).Take(5))
            .Include(s => s.Payments.OrderByDescending(p => p.Id).Take(5))
            .FirstOrDefaultAsync(s => s.Code == code, ct);

        if (supplier == null) return null;

        return new SupplierDetailsDto
        {
            Code = supplier.Code,
            Name = supplier.Name,
            Phone = supplier.Phone,
            Address = supplier.Address,
            CommercialRegistration = supplier.CommercialRegistration,
            TaxNumber = supplier.TaxNumber,
            Balance = supplier.Balance,
            Notes = supplier.Notes,
            IsActive = supplier.IsActive,
            CreatedAt = supplier.CreatedAt,
            RecentOrders = supplier.PurchaseOrders.Select(po => new SupplierRecentOrderDto
            {
                OrderNumber = po.OrderNumber,
                OrderDate = po.OrderDate,
                Status = po.Status.ToArabic(),
                TotalAmount = po.TotalAmount
            }).ToList(),
            RecentPayments = supplier.Payments.Select(p => new SupplierRecentPaymentDto
            {
                PaymentNumber = p.PaymentNumber,
                PaymentDate = p.PaymentDate,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod.ToArabic()
            }).ToList()
        };
    }

    public async Task<Result<SupplierDto>> CreateSupplierAsync(CreateSupplierDto dto, CancellationToken ct = default)
    {
        var count = await _context.Suppliers.CountAsync(ct);
        var code = $"SUP-{(count + 1):D4}";

        var supplier = new Supplier
        {
            Code = code,
            Name = dto.Name.Trim(),
            Phone = dto.Phone.Trim(),
            Address = dto.Address?.Trim(),
            CommercialRegistration = dto.CommercialRegistration?.Trim(),
            TaxNumber = dto.TaxNumber?.Trim(),
            Notes = dto.Notes?.Trim(),
            Balance = 0,
            IsActive = true
        };

        await _context.Suppliers.AddAsync(supplier, ct);
        await _context.SaveChangesAsync(ct);

        return Result<SupplierDto>.Success(new SupplierDto
        {
            Code = supplier.Code,
            Name = supplier.Name,
            Phone = supplier.Phone,
            Address = supplier.Address,
            CommercialRegistration = supplier.CommercialRegistration,
            TaxNumber = supplier.TaxNumber,
            Balance = supplier.Balance,
            Notes = supplier.Notes,
            IsActive = supplier.IsActive,
            CreatedAt = supplier.CreatedAt
        }, "تم إضافة المورد بنجاح");
    }

    public async Task<Result<SupplierDto>> UpdateSupplierAsync(string code, UpdateSupplierDto dto, CancellationToken ct = default)
    {
        var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Code == code, ct);
        if (supplier == null)
            return Result<SupplierDto>.Failure("المورد المطلوب غير موجود");

        supplier.Name = dto.Name.Trim();
        supplier.Phone = dto.Phone.Trim();
        supplier.Address = dto.Address?.Trim();
        supplier.CommercialRegistration = dto.CommercialRegistration?.Trim();
        supplier.TaxNumber = dto.TaxNumber?.Trim();
        supplier.Notes = dto.Notes?.Trim();
        supplier.IsActive = dto.IsActive;

        await _context.SaveChangesAsync(ct);

        return Result<SupplierDto>.Success(new SupplierDto
        {
            Code = supplier.Code,
            Name = supplier.Name,
            Phone = supplier.Phone,
            Address = supplier.Address,
            CommercialRegistration = supplier.CommercialRegistration,
            TaxNumber = supplier.TaxNumber,
            Balance = supplier.Balance,
            Notes = supplier.Notes,
            IsActive = supplier.IsActive,
            CreatedAt = supplier.CreatedAt
        }, "تم تحديث بيانات المورد بنجاح");
    }

    public async Task<Result> DeleteSupplierAsync(string code, CancellationToken ct = default)
    {
        var supplier = await _context.Suppliers
            .Include(s => s.PurchaseOrders)
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.Code == code, ct);

        if (supplier == null)
            return Result.Failure("المورد المطلوب غير موجود");

        if (supplier.PurchaseOrders.Any() || supplier.Payments.Any() || supplier.Balance != 0)
        {
            supplier.IsActive = false;
            await _context.SaveChangesAsync(ct);
            return Result.Success("تم إلغاء تنشيط المورد لوجود حركات مالية ومعاملات سابقة مرتبطة به");
        }

        _context.Suppliers.Remove(supplier);
        await _context.SaveChangesAsync(ct);
        return Result.Success("تم حذف المورد بنجاح");
    }
}
