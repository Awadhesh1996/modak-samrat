using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class DashboardModel : PageModel
{
    public List<Modak> Modaks { get; set; } = new();

    public IActionResult OnGet()
    {
        // 🔐 Protect page
        if (HttpContext.Session.GetString("AdminUser") == null)
        {
            return RedirectToPage("/Admin/Login");
        }

        var service = new ModakService();
        Modaks = service.GetAllModaks();

        return Page();
    }

    // ❌ DELETE
    public IActionResult OnPostDelete(int id)
    {
        var service = new ModakService();
        service.DeleteModak(id);

        return RedirectToPage();
    }

    // 🚪 LOGOUT
    public IActionResult OnPostLogout()
    {
        HttpContext.Session.Clear();
        return RedirectToPage("/Admin/Login");
    }
}