using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
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

// Configure Forwarded Headers for Azure App Service (Linux reverse proxy)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.Google.GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/access-denied";
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
})
.AddCookie("ExternalCookie")
.AddGoogle(options =>
{
    options.SignInScheme = "ExternalCookie";
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "placeholder-client-id";
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "placeholder-client-secret";
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApprovedUser", policy => 
        policy.RequireClaim("Approved", "True"));
});

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

app.UseForwardedHeaders();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
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
