using FluentValidation;
using Elfat7.Application.DTOs.Customers;
using Elfat7.Application.DTOs.Deliveries;
using Elfat7.Application.DTOs.Financial;
using Elfat7.Application.DTOs.GoodsReceiving;
using Elfat7.Application.DTOs.Inventory;
using Elfat7.Application.DTOs.Products;
using Elfat7.Application.DTOs.Purchasing;
using Elfat7.Application.DTOs.Sales;
using Elfat7.Application.DTOs.SalesReturns;
using Elfat7.Application.DTOs.Suppliers;
using Elfat7.Application.DTOs.Warehouses;

namespace Elfat7.Application.Validators;

public class CreateSupplierDtoValidator : AbstractValidator<CreateSupplierDto>
{
    public CreateSupplierDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("اسم المورد مطلوب ولا يمكن تركه فارغاً");
        RuleFor(x => x.Phone).NotEmpty().WithMessage("رقم هاتف المورد مطلوب");
    }
}

public class UpdateSupplierDtoValidator : AbstractValidator<UpdateSupplierDto>
{
    public UpdateSupplierDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("اسم المورد مطلوب");
        RuleFor(x => x.Phone).NotEmpty().WithMessage("رقم هاتف المورد مطلوب");
    }
}

public class CreateProductCategoryDtoValidator : AbstractValidator<CreateProductCategoryDto>
{
    public CreateProductCategoryDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("اسم تصنيف الأصناف مطلوب");
    }
}

public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("اسم الصنف مطلوب");
        RuleFor(x => x.CategoryCode).NotEmpty().WithMessage("كود تصنيف الصنف مطلوب");
        RuleFor(x => x.Unit).NotEmpty().WithMessage("وحدة القياس مطلوبة (مثل: طن، شيكارة)");
        When(x => x.DefaultPurchasePrice.HasValue, () =>
        {
            RuleFor(x => x.DefaultPurchasePrice!.Value).GreaterThanOrEqualTo(0).WithMessage("سعر الشراء الافتراضي لا يمكن أن يكون سالباً");
        });
        When(x => x.DefaultSellingPrice.HasValue, () =>
        {
            RuleFor(x => x.DefaultSellingPrice!.Value).GreaterThanOrEqualTo(0).WithMessage("سعر البيع الافتراضي لا يمكن أن يكون سالباً");
        });
    }
}

public class CreateWarehouseDtoValidator : AbstractValidator<CreateWarehouseDto>
{
    public CreateWarehouseDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("اسم المخزن مطلوب");
    }
}

public class CreateCustomerDtoValidator : AbstractValidator<CreateCustomerDto>
{
    public CreateCustomerDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("اسم العميل مطلوب");
        RuleFor(x => x.Phone).NotEmpty().WithMessage("رقم هاتف العميل مطلوب");
    }
}

public class CreatePurchaseOrderDtoValidator : AbstractValidator<CreatePurchaseOrderDto>
{
    public CreatePurchaseOrderDtoValidator()
    {
        RuleFor(x => x.SupplierCode).NotEmpty().WithMessage("كود المورد مطلوب");
        RuleFor(x => x.Items).NotEmpty().WithMessage("يجب إضافة بند واحد على الأقل في أمر الشراء");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductCode).NotEmpty().WithMessage("كود الصنف مطلوب");
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("الكمية يجب أن تكون أكبر من الصفر");
            item.RuleFor(i => i.UnitPrice).GreaterThan(0).WithMessage("سعر الوحدة يجب أن يكون أكبر من الصفر");
        });
    }
}

public class CreateGoodsReceiptDtoValidator : AbstractValidator<CreateGoodsReceiptDto>
{
    public CreateGoodsReceiptDtoValidator()
    {
        RuleFor(x => x.PurchaseOrderNumber).NotEmpty().WithMessage("رقم أمر الشراء المرتبط مطلوب");
        RuleFor(x => x.WarehouseCode).NotEmpty().WithMessage("كود مخزن الاستلام مطلوب");
        RuleFor(x => x.Items).NotEmpty().WithMessage("يجب إضافة صنف واحد على الأقل تم استلامه");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductCode).NotEmpty().WithMessage("كود الصنف مطلوب");
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("الكمية المستلمة يجب أن تكون أكبر من صفر");
            item.RuleFor(i => i.UnitCost).GreaterThan(0).WithMessage("تكلفة الوحدة يجب أن تكون أكبر من صفر");
        });
    }
}

public class CreateSalesOrderDtoValidator : AbstractValidator<CreateSalesOrderDto>
{
    public CreateSalesOrderDtoValidator()
    {
        RuleFor(x => x.CustomerCode).NotEmpty().WithMessage("كود العميل مطلوب");
        RuleFor(x => x.Items).NotEmpty().WithMessage("يجب إضافة بند واحد على الأقل في أمر البيع");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductCode).NotEmpty().WithMessage("كود الصنف مطلوب");
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("الكمية يجب أن تكون أكبر من صفر");
            item.RuleFor(i => i.UnitPrice).GreaterThan(0).WithMessage("سعر الوحدة يجب أن يكون أكبر من صفر");
        });
    }
}

public class CreateSalesInvoiceDtoValidator : AbstractValidator<CreateSalesInvoiceDto>
{
    public CreateSalesInvoiceDtoValidator()
    {
        RuleFor(x => x.CustomerCode).NotEmpty().WithMessage("كود العميل مطلوب");
        RuleFor(x => x.Items).NotEmpty().WithMessage("يجب إضافة صنف واحد على الأقل في الفاتورة");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductCode).NotEmpty().WithMessage("كود الصنف مطلوب");
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("الكمية المباعة يجب أن تكون أكبر من صفر");
            item.RuleFor(i => i.UnitPrice).GreaterThan(0).WithMessage("سعر البيع يجب أن يكون أكبر من صفر");
            item.RuleFor(i => i.DiscountAmount).GreaterThanOrEqualTo(0).WithMessage("الخصم لا يمكن أن يكون سالباً");
        });
    }
}

public class CreateDeliveryDtoValidator : AbstractValidator<CreateDeliveryDto>
{
    public CreateDeliveryDtoValidator()
    {
        RuleFor(x => x.SalesInvoiceNumber).NotEmpty().WithMessage("رقم فاتورة المبيعات مطلوب");
        RuleFor(x => x.WarehouseCode).NotEmpty().WithMessage("كود مخزن الصرف مطلوب");
        RuleFor(x => x.Items).NotEmpty().WithMessage("يجب تحديد الكميات المصروفة للتسليم");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductCode).NotEmpty().WithMessage("كود الصنف مطلوب");
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("الكمية المصروفة يجب أن تكون أكبر من صفر");
        });
    }
}

public class CreateSalesReturnDtoValidator : AbstractValidator<CreateSalesReturnDto>
{
    public CreateSalesReturnDtoValidator()
    {
        RuleFor(x => x.SalesInvoiceNumber).NotEmpty().WithMessage("رقم فاتورة المبيعات الأصلية مطلوب");
        RuleFor(x => x.WarehouseCode).NotEmpty().WithMessage("كود مخزن استلام المرتجع مطلوب");
        RuleFor(x => x.Reason).NotEmpty().WithMessage("سبب المرتجع مطلوب (مثلاً: بضاعة تالفة أو غير مطابقة)");
        RuleFor(x => x.Items).NotEmpty().WithMessage("يجب تحديد الأصناف المرتجعة");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductCode).NotEmpty().WithMessage("كود الصنف مطلوب");
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("الكمية المرتجعة يجب أن تكون أكبر من صفر");
        });
    }
}

public class CreateStockTransferDtoValidator : AbstractValidator<CreateStockTransferDto>
{
    public CreateStockTransferDtoValidator()
    {
        RuleFor(x => x.SourceWarehouseCode).NotEmpty().WithMessage("كود المخزن المحول منه (المصدر) مطلوب");
        RuleFor(x => x.DestinationWarehouseCode).NotEmpty().WithMessage("كود المخزن المحول إليه (الوجهة) مطلوب");
        RuleFor(x => x).Must(x => x.SourceWarehouseCode != x.DestinationWarehouseCode)
            .WithMessage("لا يمكن التحويل من وإلى نفس المخزن");
        RuleFor(x => x.Items).NotEmpty().WithMessage("يجب إضافة صنف واحد على الأقل للتحويل");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductCode).NotEmpty().WithMessage("كود الصنف مطلوب");
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("الكمية المحولة يجب أن تكون أكبر من صفر");
        });
    }
}

public class CreateStockAdjustmentDtoValidator : AbstractValidator<CreateStockAdjustmentDto>
{
    public CreateStockAdjustmentDtoValidator()
    {
        RuleFor(x => x.WarehouseCode).NotEmpty().WithMessage("كود المخزن مطلوب");
        RuleFor(x => x.Reason).NotEmpty().WithMessage("سبب التسوية الجردية مطلوب (مثلاً: عجز جرد سنوي، هالك)");
        RuleFor(x => x.Items).NotEmpty().WithMessage("يجب تحديد الأصناف المراد تسويتها");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductCode).NotEmpty().WithMessage("كود الصنف مطلوب");
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("كمية التسوية يجب أن تكون أكبر من صفر");
        });
    }
}

public class CreateSupplierPaymentDtoValidator : AbstractValidator<CreateSupplierPaymentDto>
{
    public CreateSupplierPaymentDtoValidator()
    {
        RuleFor(x => x.SupplierCode).NotEmpty().WithMessage("كود المورد مطلوب");
        RuleFor(x => x.TreasuryCode).NotEmpty().WithMessage("كود الخزينة أو الحساب البنكي مطلوب");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("مبلغ السداد يجب أن يكون أكبر من صفر");
    }
}

public class CreateCustomerPaymentDtoValidator : AbstractValidator<CreateCustomerPaymentDto>
{
    public CreateCustomerPaymentDtoValidator()
    {
        RuleFor(x => x.CustomerCode).NotEmpty().WithMessage("كود العميل مطلوب");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("مبلغ التحصيل يجب أن يكون أكبر من صفر");
    }
}

public class CreateOperatingExpenseDtoValidator : AbstractValidator<CreateOperatingExpenseDto>
{
    public CreateOperatingExpenseDtoValidator()
    {
        RuleFor(x => x.TreasuryCode).NotEmpty().WithMessage("كود الخزينة المصروف منها مطلوب");
        RuleFor(x => x.Category).NotEmpty().WithMessage("تصنيف المصروف مطلوب (نقل، عمالة، تعبئة، مرافق، إيجار)");
        RuleFor(x => x.Description).NotEmpty().WithMessage("بيان ووصف المصروف مطلوب");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("قيمة المصروف يجب أن تكون أكبر من صفر");
    }
}
