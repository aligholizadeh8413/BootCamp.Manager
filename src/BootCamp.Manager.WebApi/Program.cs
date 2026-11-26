using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using BootCamp.Manager.Application.Interfaces;
using BootCamp.Manager.Application.Services;
using BootCamp.Manager.Infrastructure.Data;
using BootCamp.Manager.Infrastructure.Repositories;
using BootCamp.Manager.WebApi.Filters;
using BootCamp.Manager.WebApi.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews(options => options.Filters.Add<ActionLoggingFilter>())
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(
        new JsonStringEnumConverter(allowIntegerValues: false)));
// پیام‌های ناشی از JSON نامعتبر هم برای کاربر فارسی می‌شوند.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Where(x => x.Value!.Errors.Count > 0)
            .ToDictionary(x => x.Key, x => x.Value!.Errors.Select(error =>
                error.ErrorMessage.Any(c => c >= '\u0600' && c <= '\u06ff')
                    ? error.ErrorMessage : "ساختار یا نوع داده ارسالی معتبر نیست.").ToArray());
        return new BadRequestObjectResult(new ValidationProblemDetails(errors)
        {
            Status = 400, Title = "اطلاعات ورودی معتبر نیست.",
            Extensions = { ["traceId"] = context.HttpContext.TraceIdentifier }
        });
    };
});
// مسیر پیش‌فرض نسبت به خود پروژه است تا تغییر پوشه ترمینال، دیتابیس جدید نسازد.
var connection = builder.Configuration.GetConnectionString("Bootcamp");
if (string.IsNullOrWhiteSpace(connection))
{
    var dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
    Directory.CreateDirectory(dataDirectory);
    connection = $"Data Source={Path.Combine(dataDirectory, "bootcamp.db")};Foreign Keys=True";
}
builder.Services.AddDbContext<BootcampDbContext>(options => options.UseSqlite(connection));
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IInstructorRepository, InstructorRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<IEvaluationRepository, EvaluationRepository>();
builder.Services.AddScoped<StudentService>();
builder.Services.AddScoped<InstructorService>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<EvaluationService>();
var app = builder.Build();
// Migration خودکار فقط برای اجرای محلی این تمرین است.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BootcampDbContext>();
    await db.Database.MigrateAsync();
}
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
app.Run();
