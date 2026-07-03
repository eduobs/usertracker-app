using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.DataProtection;
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

// Data Protection (Essencial para OAuth no Azure Linux)
var keysFolder = Path.Combine(contentRoot, "Data", "Keys");
if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && Directory.Exists("/home"))
{
    keysFolder = "/home/DataProtection-Keys";
}
Directory.CreateDirectory(keysFolder);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysFolder))
    .SetApplicationName("UserTracker");

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
    
    options.Events.OnRemoteFailure = context =>
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(context.Failure, "Falha na Autenticação Remota com o Google.");
        
        context.Response.Redirect("/login?error=" + Uri.EscapeDataString(context.Failure?.Message ?? "Erro desconhecido"));
        context.HandleResponse();
        return Task.CompletedTask;
    };
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

    // Cria a tabela Users manualmente caso o banco de dados já existisse 
    // antes da implementação do sistema de Login
    db.Database.ExecuteSqlRaw(@"
        CREATE TABLE IF NOT EXISTS ""Users"" (
            ""Id"" TEXT NOT NULL CONSTRAINT ""PK_Users"" PRIMARY KEY,
            ""GoogleSubjectId"" TEXT NOT NULL,
            ""Email"" TEXT NOT NULL,
            ""Name"" TEXT NOT NULL,
            ""Role"" INTEGER NOT NULL,
            ""IsApproved"" INTEGER NOT NULL,
            ""CreatedAt"" TEXT NOT NULL
        );
        CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Users_Email"" ON ""Users"" (""Email"");
    ");
}

app.Run();
