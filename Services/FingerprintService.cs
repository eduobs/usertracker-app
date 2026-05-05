using UAParser;
using UserTracker.Models;

namespace UserTracker.Services;

public class FingerprintService
{
    private readonly IHttpContextAccessor _http;
    private readonly ILogger<FingerprintService> _logger;

    public FingerprintService(IHttpContextAccessor http, ILogger<FingerprintService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public UserAccess BuildFromRequest()
    {
        var ctx = _http.HttpContext!;
        var req = ctx.Request;
        var headers = req.Headers;

        string ip = GetIpAddress(ctx);

        // Parse User-Agent
        string ua = headers["User-Agent"].ToString();
        var parser = Parser.GetDefault();
        var clientInfo = parser.Parse(ua);

        var access = new UserAccess
        {
            SessionId      = GetOrCreateSessionId(ctx),
            IpAddress      = ip,
            ForwardedFor   = headers["X-Forwarded-For"].ToString(),
            RealIp         = headers["X-Real-IP"].ToString(),

            UserAgent      = ua,
            AcceptLanguage = headers["Accept-Language"].ToString(),
            AcceptEncoding = headers["Accept-Encoding"].ToString(),
            Referer        = headers["Referer"].ToString(),
            Origin         = headers["Origin"].ToString(),
            Host           = headers["Host"].ToString(),
            Connection     = headers["Connection"].ToString(),
            CacheControl   = headers["Cache-Control"].ToString(),
            Dnt            = headers["DNT"].ToString(),

            // Client Hints (Chrome/Edge)
            SecChUa         = headers["Sec-CH-UA"].ToString(),
            SecChUaMobile   = headers["Sec-CH-UA-Mobile"].ToString(),
            SecChUaPlatform = headers["Sec-CH-UA-Platform"].ToString(),
            SecFetchSite    = headers["Sec-Fetch-Site"].ToString(),
            SecFetchMode    = headers["Sec-Fetch-Mode"].ToString(),
            SecFetchDest    = headers["Sec-Fetch-Dest"].ToString(),

            // Parsed UA
            OsFamily      = clientInfo.OS.Family,
            OsVersion     = $"{clientInfo.OS.Major}.{clientInfo.OS.Minor}",
            BrowserFamily = clientInfo.UA.Family,
            BrowserVersion= $"{clientInfo.UA.Major}.{clientInfo.UA.Minor}",
            DeviceFamily  = clientInfo.Device.Family,
            IsMobile      = clientInfo.Device.IsSpider == false &&
                             (clientInfo.Device.Family.Contains("iPhone") ||
                              clientInfo.Device.Family.Contains("Android") ||
                              clientInfo.Device.Family.Contains("iPad")),

            RequestPath   = req.Path,
            RequestMethod = req.Method,
            QueryString   = req.QueryString.ToString(),
            Protocol      = req.Protocol,
            AccessedAt    = DateTime.UtcNow
        };

        return access;
    }

    private static string GetIpAddress(HttpContext ctx)
    {
        var forwarded = ctx.Request.Headers["X-Forwarded-For"].ToString();
        if (!string.IsNullOrEmpty(forwarded))
            return forwarded.Split(',')[0].Trim();

        var realIp = ctx.Request.Headers["X-Real-IP"].ToString();
        if (!string.IsNullOrEmpty(realIp))
            return realIp;

        return ctx.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private static string GetOrCreateSessionId(HttpContext ctx)
    {
        const string key = "UserTracker_SessionId";
        if (ctx.Session.TryGetValue(key, out var bytes))
            return System.Text.Encoding.UTF8.GetString(bytes);

        var id = Guid.NewGuid().ToString();
        ctx.Session.SetString(key, id);
        return id;
    }
}
