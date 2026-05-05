using System.ComponentModel.DataAnnotations;

namespace UserTracker.Models;

public class UserAccess
{
    [Key]
    public int Id { get; set; }

    // --- Identificação da sessão ---
    public string SessionId { get; set; } = string.Empty;
    public string FingerprintHash { get; set; } = string.Empty;

    // --- Rede ---
    public string IpAddress { get; set; } = string.Empty;
    public string? ForwardedFor { get; set; }
    public string? RealIp { get; set; }

    // --- Headers HTTP (servidor) ---
    public string? UserAgent { get; set; }
    public string? AcceptLanguage { get; set; }
    public string? AcceptEncoding { get; set; }
    public string? Referer { get; set; }
    public string? Origin { get; set; }
    public string? Host { get; set; }
    public string? Connection { get; set; }
    public string? CacheControl { get; set; }
    public string? Dnt { get; set; }              // Do Not Track
    public string? SecChUa { get; set; }          // Client Hints: UA string
    public string? SecChUaMobile { get; set; }     // Client Hints: mobile?
    public string? SecChUaPlatform { get; set; }   // Client Hints: plataforma
    public string? SecFetchSite { get; set; }
    public string? SecFetchMode { get; set; }
    public string? SecFetchDest { get; set; }

    // --- Dados do navegador (JavaScript) ---
    public string? BrowserLanguage { get; set; }
    public string? BrowserLanguages { get; set; }
    public string? Platform { get; set; }
    public string? Timezone { get; set; }
    public int? TimezoneOffset { get; set; }
    public bool? CookiesEnabled { get; set; }
    public string? PluginsList { get; set; }
    public string? MimeTypesList { get; set; }

    // --- Tela e hardware ---
    public int? ScreenWidth { get; set; }
    public int? ScreenHeight { get; set; }
    public int? AvailWidth { get; set; }
    public int? AvailHeight { get; set; }
    public int? ColorDepth { get; set; }
    public double? PixelRatio { get; set; }
    public int? HardwareConcurrency { get; set; }
    public double? DeviceMemoryGb { get; set; }
    public int? MaxTouchPoints { get; set; }

    // --- Capacidades do navegador ---
    public bool? LocalStorageAvailable { get; set; }
    public bool? SessionStorageAvailable { get; set; }
    public bool? IndexedDbAvailable { get; set; }
    public bool? WebRtcAvailable { get; set; }
    public string? ConnectionType { get; set; }
    public double? ConnectionDownlink { get; set; }

    // --- Canvas / WebGL fingerprint ---
    public string? CanvasHash { get; set; }
    public string? WebGlVendor { get; set; }
    public string? WebGlRenderer { get; set; }
    public string? WebGlVersion { get; set; }

    // --- Parsed do User-Agent (servidor) ---
    public string? OsFamily { get; set; }
    public string? OsVersion { get; set; }
    public string? BrowserFamily { get; set; }
    public string? BrowserVersion { get; set; }
    public string? DeviceFamily { get; set; }
    public bool? IsMobile { get; set; }

    // --- Página acessada ---
    public string? RequestPath { get; set; }
    public string? RequestMethod { get; set; }
    public string? QueryString { get; set; }
    public string? Protocol { get; set; }

    // --- Metadados ---
    public DateTime AccessedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
}
