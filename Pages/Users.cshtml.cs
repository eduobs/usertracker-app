using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UserTracker.Data;
using UserTracker.Models;

namespace UserTracker.Pages;

[Authorize(Roles = "Admin")]
public class UsersModel : PageModel
{
    private readonly AppDbContext _db;

    public UsersModel(AppDbContext db)
    {
        _db = db;
    }

    public List<AppUser> Users { get; set; } = new();
    public string CurrentUserEmail { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        CurrentUserEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "";
        Users = await _db.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();
    }

    public async Task<IActionResult> OnPostApproveAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user != null)
        {
            user.IsApproved = true;
            await _db.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMakeAdminAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user != null)
        {
            user.Role = UserRole.Admin;
            await _db.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMakeCommonAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId);
        var currentEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

        if (user != null && user.Email != currentEmail)
        {
            user.Role = UserRole.Common;
            await _db.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}
