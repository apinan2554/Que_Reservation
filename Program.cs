using Microsoft.EntityFrameworkCore;
using Reservation_Que.Data;
using Reservation_Que.Hubs;
using Reservation_Que.Models;

var builder = WebApplication.CreateBuilder(args);

// Render (และ PaaS อื่น) ส่งพอร์ตมาทาง env "PORT" — ให้แอปฟังพอร์ตนั้น
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://+:{port}");
}

// ---------------------------------------------------------------------------
// [ส่วนที่ 1.3] ตั้งค่าระบบส่วนกลาง (Service Registration)
// ---------------------------------------------------------------------------

// 0) รองรับ reverse proxy ของ Render — อ่าน X-Forwarded-Proto/For
//    เพื่อให้ Request.Scheme เป็น https (จำเป็นสำหรับ QR และ GPS)
builder.Services.Configure<Microsoft.AspNetCore.Builder.ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
        Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
    // ล้างค่า default เพื่อรับจาก proxy ของ Render ได้ (ไม่รู้ IP ล่วงหน้า)
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// 1) เปิดใช้งาน MVC (Controllers + Views)
builder.Services.AddControllersWithViews();

// 2) ลงทะเบียน EF Core (ใช้ SQLite เพื่อให้รันได้ทันที)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3) เปิดใช้งาน SignalR
builder.Services.AddSignalR();

// 3.1) ลงทะเบียน QueueService (Logic การจัดการคิว)
builder.Services.AddScoped<Reservation_Que.Services.QueueService>();

// 4) ผูกค่าตั้งค่าร้านจาก appsettings.json
builder.Services.Configure<StoreSettings>(builder.Configuration.GetSection("StoreSettings"));

// 5) ตั้งค่า CORS ให้รองรับการทำงานข้ามอุปกรณ์ (มือถือลูกค้า, จอหน้าร้าน, แดชบอร์ด)
const string CorsPolicy = "AllowAll";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        policy.SetIsOriginAllowed(_ => true) // อนุญาตทุก origin (ต้องใช้คู่กับ AllowCredentials สำหรับ SignalR)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// ---------------------------------------------------------------------------
// สร้างฐานข้อมูลอัตโนมัติเมื่อเริ่มระบบ (สะดวกสำหรับ Dev / Demo)
// ---------------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// ---------------------------------------------------------------------------
// HTTP Request Pipeline
// ---------------------------------------------------------------------------

// ต้องมาก่อนสุด: ให้แอปเชื่อ header จาก proxy (https จริงของ Render)
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    // บังคับ HTTPS เฉพาะตอน production (ตอน dev ปล่อยให้ยิง http ได้เพื่อทดสอบง่าย)
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

// CORS ต้องมาก่อน Authorization และ Map endpoints
app.UseCors(CorsPolicy);

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map SignalR Hub
app.MapHub<QueueHub>("/queueHub");

app.Run();

// เปิดให้ integration test เข้าถึง Program class ได้
public partial class Program { }
