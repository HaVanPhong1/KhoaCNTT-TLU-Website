using KhoaCNTT.API.Utils;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore; // Thêm dòng này

var builder = WebApplication.CreateBuilder(args);

// --- 1. Cấu hình Kestrel (Upload file lớn) ---
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 250 * 1024 * 1024; // 250MB
});

// --- 2. Cấu hình CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// --- 3. Đăng ký các dịch vụ hệ thống ---
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSwagger();

builder.Services.AddControllers(options => {
    options.Filters.Add<ApiExceptionFilter>();
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// ============================================================
// --- TỰ ĐỘNG KHỞI TẠO DATABASE KHI CHẠY DOCKER ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Thay 'AppDbContext' bằng tên DbContext thực tế trong project của ông nếu khác nhé
        var context = services.GetRequiredService<KhoaCNTT.Infrastructure.Persistence.AppDbContext>();

        // Lệnh này sẽ tự động tạo Database và các bảng dựa trên code C# của ông
        context.Database.EnsureCreated();
        Console.WriteLine("-----> DATABASE KHOACNTT DA SAN SANG! <-----");
    }
    catch (Exception ex)
    {
        Console.WriteLine("-----> LOI KHOI TAO DB: " + ex.Message);
    }
}
// ============================================================

// --- 4. Cấu hình Swagger ---
app.UseSwagger();
app.UseSwaggerUI(options => {
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "KhoaCNTT API V1");
    options.RoutePrefix = "swagger";
});

app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();