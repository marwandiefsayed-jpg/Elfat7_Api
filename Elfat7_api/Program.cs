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

// 2. Add Clean Architecture Layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// 3. Add Controllers
builder.Services.AddControllers();

// 4. Configure Swagger / OpenAPI
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

// 5. Database Auto-Creation & Seeding
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<Elfat7DbContext>();
        logger.LogInformation("التحقق من إنشاء قاعدة البيانات وجداول النظام...");
        await db.Database.EnsureCreatedAsync();
        logger.LogInformation("بدء تهيئة البيانات الأولية الافتراضية (Seeding)...");
        await DatabaseSeeder.SeedAsync(db);
        logger.LogInformation("تمت تهيئة قاعدة البيانات والبيانات الأولية بنجاح.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "حدث خطأ أثناء تهيئة قاعدة البيانات: {Message}", ex.Message);
    }
}

// 6. Global Exception Handling Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 7. HTTP Pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "شركة الفتح للحاصلات الزراعية v1");
    c.RoutePrefix = string.Empty; // Swagger at root URL
});

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

// For integration tests
public partial class Program { }
