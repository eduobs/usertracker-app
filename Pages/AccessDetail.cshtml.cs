using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Microsoft.AspNetCore.Authorization;

namespace UserTracker.Pages;

[Authorize(Policy = "ApprovedUser")]
public class AccessDetailModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public int AccessId => Id;

    public void OnGet() { }
}
