using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ASPA007_1.Pages;

public sealed class IndexModel : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/Celebrities");
}
