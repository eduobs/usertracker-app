using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserTracker.Data;
using UserTracker.Models;
using UserTracker.Services;

namespace UserTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrackingController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly FingerprintService _fp;

    public TrackingController(AppDbContext db, FingerprintService fp)
    {
        _db = db;
        _fp = fp;
    }

    /// <summary>
    /// Recebe o fingerprint coletado pelo JavaScript e mescla com dados do servidor.
    /// </summary>
    [HttpPost("collect")]
    public async Task<IActionResult> Collect([FromBody] ClientFingerprintDto dto)
    {
        // Monta base com dados do servidor (headers, IP, UA parseado...)
        var access = _fp.BuildFromRequest();

        // Mescla com dados do cliente (JavaScript)
        access.FingerprintHash       = dto.FingerprintHash ?? string.Empty;
        access.BrowserLanguage       = dto.Language;
        access.BrowserLanguages      = dto.Languages;
        access.Platform              = dto.Platform;
        access.Timezone              = dto.Timezone;
        access.TimezoneOffset        = dto.TimezoneOffset;
        access.CookiesEnabled        = dto.CookiesEnabled;
        access.PluginsList           = dto.Plugins;
        access.ScreenWidth           = dto.ScreenWidth;
        access.ScreenHeight          = dto.ScreenHeight;
        access.AvailWidth            = dto.AvailWidth;
        access.AvailHeight           = dto.AvailHeight;
        access.ColorDepth            = dto.ColorDepth;
        access.PixelRatio            = dto.PixelRatio;
        access.HardwareConcurrency   = dto.HardwareConcurrency;
        access.DeviceMemoryGb        = dto.DeviceMemory;
        access.MaxTouchPoints        = dto.MaxTouchPoints;
        access.LocalStorageAvailable = dto.LocalStorage;
        access.SessionStorageAvailable = dto.SessionStorage;
        access.IndexedDbAvailable    = dto.IndexedDb;
        access.WebRtcAvailable       = dto.WebRtc;
        access.ConnectionType        = dto.ConnectionType;
        access.ConnectionDownlink    = dto.ConnectionDownlink;
        access.CanvasHash            = dto.CanvasHash;
        access.WebGlVendor           = dto.WebGlVendor;
        access.WebGlRenderer         = dto.WebGlRenderer;
        access.WebGlVersion          = dto.WebGlVersion;

        _db.UserAccesses.Add(access);
        await _db.SaveChangesAsync();

        return Ok(new { id = access.Id, fingerprintHash = access.FingerprintHash });
    }

    /// <summary>
    /// Retorna todos os acessos paginados (para o dashboard).
    /// </summary>
    [HttpGet("accesses")]
    public async Task<IActionResult> GetAccesses([FromQuery] int page = 1, [FromQuery] int size = 20,
                                                  [FromQuery] string? ip = null, [FromQuery] string? hash = null)
    {
        var query = _db.UserAccesses.AsQueryable();

        if (!string.IsNullOrEmpty(ip))
            query = query.Where(x => x.IpAddress.Contains(ip));

        if (!string.IsNullOrEmpty(hash))
            query = query.Where(x => x.FingerprintHash.Contains(hash));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.AccessedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return Ok(new { total, page, size, items });
    }

    /// <summary>
    /// Retorna detalhes de um acesso específico.
    /// </summary>
    [HttpGet("accesses/{id:int}")]
    public async Task<IActionResult> GetAccess(int id)
    {
        var access = await _db.UserAccesses.FindAsync(id);
        if (access is null) return NotFound();
        return Ok(access);
    }

    /// <summary>
    /// Retorna métricas resumidas para o dashboard.
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var total = await _db.UserAccesses.CountAsync();
        var uniqueIps = await _db.UserAccesses.Select(x => x.IpAddress).Distinct().CountAsync();
        var uniqueFingerprints = await _db.UserAccesses.Select(x => x.FingerprintHash).Distinct().CountAsync();
        var today = await _db.UserAccesses.Where(x => x.AccessedAt >= DateTime.UtcNow.Date).CountAsync();
        var topIps = await _db.UserAccesses
            .GroupBy(x => x.IpAddress)
            .Select(g => new { ip = g.Key, count = g.Count() })
            .OrderByDescending(x => x.count)
            .Take(5)
            .ToListAsync();
        var topBrowsers = await _db.UserAccesses
            .Where(x => x.BrowserFamily != null)
            .GroupBy(x => x.BrowserFamily)
            .Select(g => new { browser = g.Key, count = g.Count() })
            .OrderByDescending(x => x.count)
            .Take(5)
            .ToListAsync();
        var topOs = await _db.UserAccesses
            .Where(x => x.OsFamily != null)
            .GroupBy(x => x.OsFamily)
            .Select(g => new { os = g.Key, count = g.Count() })
            .OrderByDescending(x => x.count)
            .Take(5)
            .ToListAsync();

        return Ok(new { total, uniqueIps, uniqueFingerprints, today, topIps, topBrowsers, topOs });
    }

    /// <summary>
    /// Deleta um registro de acesso.
    /// </summary>
    [HttpDelete("accesses/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var access = await _db.UserAccesses.FindAsync(id);
        if (access is null) return NotFound();
        _db.UserAccesses.Remove(access);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("debug-files")]
    public IActionResult DebugFiles()
    {
        var contentRoot = Directory.GetCurrentDirectory();
        var files = Directory.GetFileSystemEntries(contentRoot, "*", SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(contentRoot, f))
            .ToList();
        return Ok(new { contentRoot, files });
    }
}

public class ClientFingerprintDto
{
    public string? FingerprintHash { get; set; }
    public string? Language { get; set; }
    public string? Languages { get; set; }
    public string? Platform { get; set; }
    public string? Timezone { get; set; }
    public int? TimezoneOffset { get; set; }
    public bool? CookiesEnabled { get; set; }
    public string? Plugins { get; set; }
    public int? ScreenWidth { get; set; }
    public int? ScreenHeight { get; set; }
    public int? AvailWidth { get; set; }
    public int? AvailHeight { get; set; }
    public int? ColorDepth { get; set; }
    public double? PixelRatio { get; set; }
    public int? HardwareConcurrency { get; set; }
    public double? DeviceMemory { get; set; }
    public int? MaxTouchPoints { get; set; }
    public bool? LocalStorage { get; set; }
    public bool? SessionStorage { get; set; }
    public bool? IndexedDb { get; set; }
    public bool? WebRtc { get; set; }
    public string? ConnectionType { get; set; }
    public double? ConnectionDownlink { get; set; }
    public string? CanvasHash { get; set; }
    public string? WebGlVendor { get; set; }
    public string? WebGlRenderer { get; set; }
    public string? WebGlVersion { get; set; }
}
