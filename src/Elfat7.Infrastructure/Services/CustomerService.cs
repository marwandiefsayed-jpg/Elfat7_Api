using Elfat7.Application.Common;
using Elfat7.Application.DTOs.Customers;
using Elfat7.Application.Interfaces;
using Elfat7.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Elfat7.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly IApplicationDbContext _context;

    public CustomerService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<CustomerDto>> GetCustomersAsync(CustomerFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.Customers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(c => c.Name.Contains(term) || c.Code.Contains(term) || c.Phone.Contains(term));
        }

        if (filter.IsActive.HasValue)
            query = query.Where(c => c.IsActive == filter.IsActive.Value);

        if (filter.MinBalance.HasValue)
            query = query.Where(c => c.Balance >= filter.MinBalance.Value);

        if (filter.MaxBalance.HasValue)
            query = query.Where(c => c.Balance <= filter.MaxBalance.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(c => c.Id)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(c => new CustomerDto
            {
                Code = c.Code,
                Name = c.Name,
                Phone = c.Phone,
                Address = c.Address,
                CommercialRegistration = c.CommercialRegistration,
                CreditLimit = c.CreditLimit,
                Balance = c.Balance,
                Notes = c.Notes,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(ct);

        return new PagedResult<CustomerDto>(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<List<CustomerDto>> GetAllFilteredCustomersAsync(CustomerFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.Customers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(c => c.Name.Contains(term) || c.Code.Contains(term) || c.Phone.Contains(term));
        }

        if (filter.IsActive.HasValue)
            query = query.Where(c => c.IsActive == filter.IsActive.Value);

        if (filter.MinBalance.HasValue)
            query = query.Where(c => c.Balance >= filter.MinBalance.Value);

        if (filter.MaxBalance.HasValue)
            query = query.Where(c => c.Balance <= filter.MaxBalance.Value);

        return await query
            .OrderByDescending(c => c.Id)
            .Select(c => new CustomerDto
            {
                Code = c.Code,
                Name = c.Name,
                Phone = c.Phone,
                Address = c.Address,
                CommercialRegistration = c.CommercialRegistration,
                CreditLimit = c.CreditLimit,
                Balance = c.Balance,
                Notes = c.Notes,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(ct);
    }

    public async Task<CustomerDetailsDto?> GetCustomerByCodeAsync(string code, CancellationToken ct = default)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .Include(c => c.SalesInvoices.OrderByDescending(i => i.Id).Take(5))
            .Include(c => c.Payments.OrderByDescending(p => p.Id).Take(5))
            .FirstOrDefaultAsync(c => c.Code == code, ct);

        if (customer == null) return null;

        return new CustomerDetailsDto
        {
            Code = customer.Code,
            Name = customer.Name,
            Phone = customer.Phone,
            Address = customer.Address,
            CommercialRegistration = customer.CommercialRegistration,
            CreditLimit = customer.CreditLimit,
            Balance = customer.Balance,
            Notes = customer.Notes,
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt,
            RecentInvoices = customer.SalesInvoices.Select(i => new CustomerRecentInvoiceDto
            {
                InvoiceNumber = i.InvoiceNumber,
                InvoiceDate = i.InvoiceDate,
                Status = i.Status.ToArabic(),
                TotalAmount = i.TotalAmount,
                RemainingAmount = i.RemainingAmount
            }).ToList(),
            RecentPayments = customer.Payments.Select(p => new CustomerRecentPaymentDto
            {
                ReceiptNumber = p.ReceiptNumber,
                PaymentDate = p.PaymentDate,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod.ToArabic()
            }).ToList()
        };
    }

    public async Task<Result<CustomerDto>> CreateCustomerAsync(CreateCustomerDto dto, CancellationToken ct = default)
    {
        var count = await _context.Customers.CountAsync(ct);
        var code = $"CUST-{(count + 1):D4}";

        var customer = new Customer
        {
            Code = code,
            Name = dto.Name.Trim(),
            Phone = dto.Phone.Trim(),
            Address = dto.Address?.Trim(),
            CommercialRegistration = dto.CommercialRegistration?.Trim(),
            CreditLimit = dto.CreditLimit,
            Notes = dto.Notes?.Trim(),
            Balance = 0,
            IsActive = true
        };

        await _context.Customers.AddAsync(customer, ct);
        await _context.SaveChangesAsync(ct);

        return Result<CustomerDto>.Success(new CustomerDto
        {
            Code = customer.Code,
            Name = customer.Name,
            Phone = customer.Phone,
            Address = customer.Address,
            CommercialRegistration = customer.CommercialRegistration,
            CreditLimit = customer.CreditLimit,
            Balance = customer.Balance,
            Notes = customer.Notes,
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt
        }, "تم إضافة العميل بنجاح");
    }

    public async Task<Result<CustomerDto>> UpdateCustomerAsync(string code, UpdateCustomerDto dto, CancellationToken ct = default)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Code == code, ct);
        if (customer == null)
            return Result<CustomerDto>.Failure("العميل المطلوب غير موجود");

        customer.Name = dto.Name.Trim();
        customer.Phone = dto.Phone.Trim();
        customer.Address = dto.Address?.Trim();
        customer.CommercialRegistration = dto.CommercialRegistration?.Trim();
        customer.CreditLimit = dto.CreditLimit;
        customer.Notes = dto.Notes?.Trim();
        customer.IsActive = dto.IsActive;

        await _context.SaveChangesAsync(ct);

        return Result<CustomerDto>.Success(new CustomerDto
        {
            Code = customer.Code,
            Name = customer.Name,
            Phone = customer.Phone,
            Address = customer.Address,
            CommercialRegistration = customer.CommercialRegistration,
            CreditLimit = customer.CreditLimit,
            Balance = customer.Balance,
            Notes = customer.Notes,
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt
        }, "تم تحديث بيانات العميل بنجاح");
    }

    public async Task<Result> DeleteCustomerAsync(string code, CancellationToken ct = default)
    {
        var customer = await _context.Customers
            .Include(c => c.SalesInvoices)
            .Include(c => c.Payments)
            .FirstOrDefaultAsync(c => c.Code == code, ct);

        if (customer == null)
            return Result.Failure("العميل المطلوب غير موجود");

        if (customer.SalesInvoices.Any() || customer.Payments.Any() || customer.Balance != 0)
        {
            customer.IsActive = false;
            await _context.SaveChangesAsync(ct);
            return Result.Success("تم إلغاء تنشيط العميل لوجود حركات مالية ومعاملات سابقة مرتبطة به");
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync(ct);
        return Result.Success("تم حذف العميل بنجاح");
    }
}
