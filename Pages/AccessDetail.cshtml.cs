using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UserTracker.Pages;

public class AccessDetailModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public int AccessId => Id;

    public void OnGet() { }
}
