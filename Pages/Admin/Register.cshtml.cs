using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class RegisterModel : PageModel
{
    public string Message { get; set; } = "";

    public void OnGet() { }

    public void OnPost(string Name, string Phone, string Username, string Password)
    {
        var service = new ModakService();

        bool success = service.RegisterAdmin(Name, Phone, Username, Password);

        Message = success ? "Registered Successfully!" : "Username already exists!";
    }
}