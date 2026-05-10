using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class LoginModel : PageModel
{
    public string Message { get; set; } = "";

    public void OnGet() { }

    public IActionResult OnPost(string Username, string Password)
    {
        var service = new ModakService();

        bool isValid = service.LoginAdmin(Username, Password);

        if (isValid)
        {
            HttpContext.Session.SetString("AdminUser", Username);
            return RedirectToPage("/Admin/Dashboard");
        }

        Message = "Invalid username or password";
        return Page();
    }

}