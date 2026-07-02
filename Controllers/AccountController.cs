using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserTracker.Data;
using UserTracker.Models;

namespace UserTracker.Controllers;

[Route("[controller]")]
public class AccountController : Controller
{
    private readonly AppDbContext _db;

    public AccountController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("/login")]
    public IActionResult Login(string returnUrl = "/dashboard")
    {
        var callbackUrl = $"/login-callback?returnUrl={Uri.EscapeDataString(returnUrl)}";
        var properties = new AuthenticationProperties { RedirectUri = callbackUrl };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("/logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }

    [HttpGet("/login-callback")]
    public async Task<IActionResult> GoogleResponse(string returnUrl = "/dashboard")
    {
        var result = await HttpContext.AuthenticateAsync("ExternalCookie");
        
        if (!result.Succeeded)
            return Redirect("/login");

        var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
        var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var googleId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (email == null)
            return Redirect("/login");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        
        if (user == null)
        {
            var userCount = await _db.Users.CountAsync();
            user = new AppUser
            {
                Email = email,
                Name = name ?? email,
                GoogleSubjectId = googleId ?? "",
                Role = userCount == 0 ? UserRole.Admin : UserRole.Common,
                IsApproved = userCount == 0
            };
            
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        // Generate custom claims
        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
        identity.AddClaim(new Claim(ClaimTypes.Name, user.Name));
        identity.AddClaim(new Claim(ClaimTypes.Email, user.Email));
        identity.AddClaim(new Claim(ClaimTypes.Role, user.Role.ToString()));
        identity.AddClaim(new Claim("Approved", user.IsApproved.ToString()));

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        await HttpContext.SignOutAsync("ExternalCookie");

        if (!user.IsApproved)
        {
            return Redirect("/pending");
        }

        return LocalRedirect(returnUrl);
    }

    [HttpGet("/access-denied")]
    public IActionResult AccessDenied()
    {
        return Redirect("/pending");
    }
}
