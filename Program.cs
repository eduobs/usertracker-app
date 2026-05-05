using Microsoft.EntityFrameworkCore;
using UserTracker.Data;
using UserTracker.Services;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);

// Ensure WebRoot is correctly set (especially on Azure Linux)
var contentRoot = builder.Environment.ContentRootPath;
var webRoot = Path.Combine(contentRoot, "wwwroot");
if (Directory.Exists(webRoot))
{
    builder.Environment.WebRootPath = webRoot;
}

// --- Services ---
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// SQLite via EF Core
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "Data", "usertracker.db");
if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && Directory.Exists("/home"))
{
    dbPath = "/home/Data/usertracker.db";
}
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite($"Data Source={dbPath}"));

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromHours(2);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
    o.Cookie.Name = ".UserTracker.Session";
});

// App services
builder.Services.AddScoped<FingerprintService>();

// Swagger (dev only)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "UserTracker API", Version = "v1" });
});

var app = builder.Build();

// --- Middleware ---
// Swagger sempre ativo (remova em produção se quiser restringir)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserTracker API v1");
    c.RoutePrefix = "swagger";
});

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.Run();
