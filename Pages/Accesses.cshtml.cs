using Microsoft.AspNetCore.Mvc.RazorPages;

using Microsoft.AspNetCore.Authorization;

namespace UserTracker.Pages;

[Authorize(Policy = "ApprovedUser")]
public class AccessesModel : PageModel
{
    public void OnGet() { }
}
