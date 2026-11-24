using Elfat7.Api.Middleware;
using Elfat7.Application;
using Elfat7.Infrastructure;
using Elfat7.Infrastructure.Persistence;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// 2. Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 3. Add Clean Architecture Layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// 4. Add Controllers
builder.Services.AddControllers();

// 5. Configure Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "نظام شركة الفتح لتجارة وتوزيع الحاصلات الزراعية (Elfat7 Agri Trading API)",
        Version = "v1",
        Description = "واجهة برمجة التطبيقات الخلفية لإدارة عمليات المشتريات، المخازن، المبيعات، أذون التسليم، الحسابات المالية، وتقارير الأعمال مع دعم كامل لتصدير ملفات PDF باللغة العربية والاتجاه من اليمين لليسار (RTL)."
    });
});

var app = builder.Build();

// 6. Database Auto-Creation
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<Elfat7DbContext>();
        logger.LogInformation("التحقق من إنشاء قاعدة البيانات وجداول النظام...");
        await db.Database.EnsureCreatedAsync();
        logger.LogInformation("تم التأكد من وجود قاعدة البيانات والجداول (البيانات الأولية معطلة).");

        // ضمان قابلية القيمة الفارغة لحقل الخزينة في جدول CustomerPayments
        try
        {
            await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(db.Database, @"
                IF EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'CustomerPayments' 
                      AND COLUMN_NAME = 'TreasuryId' 
                      AND IS_NULLABLE = 'NO'
                )
                BEGIN
                    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CustomerPayments_Treasuries_TreasuryId')
                        ALTER TABLE CustomerPayments DROP CONSTRAINT FK_CustomerPayments_Treasuries_TreasuryId;
                    
                    ALTER TABLE CustomerPayments ALTER COLUMN TreasuryId INT NULL;
                    
                    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CustomerPayments_Treasuries_TreasuryId')
                        ALTER TABLE CustomerPayments ADD CONSTRAINT FK_CustomerPayments_Treasuries_TreasuryId 
                        FOREIGN KEY (TreasuryId) REFERENCES Treasuries(Id) ON DELETE SET NULL;
                END
            ");
            logger.LogInformation("تم التأكد من دعم سداد المديونية دون إلزامية الخزينة في قاعدة البيانات.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "تنبيه أثناء التحقق من مخطط جدول سندات قبض العملاء: {Message}", ex.Message);
        }

        // Seeding is disabled to keep the database empty:
        // await DatabaseSeeder.SeedAsync(db);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "حدث خطأ أثناء تهيئة قاعدة البيانات: {Message}", ex.Message);
    }
}

// 7. Global Exception Handling Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 8. Enable CORS
app.UseCors("AllowAll");

// 9. HTTP Pipeline & Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "شركة الفتح للحاصلات الزراعية v1");
    c.RoutePrefix = "swagger";
});

// Redirect root to swagger so both '/' and '/swagger' work
app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapControllers();

app.Run();

// For integration tests
public partial class Program { }
