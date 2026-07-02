using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace UserTracker.Pages;

[Authorize]
public class PendingApprovalModel : PageModel
{
    public void OnGet()
    {
    }
}
