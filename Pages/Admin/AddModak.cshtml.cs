using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class AddModakModel : PageModel
{
    private readonly IWebHostEnvironment _env;

    public AddModakModel(IWebHostEnvironment env)
    {
        _env = env;
    }

    public IActionResult OnGet()
    {
        if (HttpContext.Session.GetString("AdminUser") == null)
            return RedirectToPage("/Admin/Login");

        return Page();
    }

    public async Task<IActionResult> OnPost(string Name, int Price, string Description, List<IFormFile> Images)
    {
        var service = new ModakService();

        var modak = new Modak
        {
            Name = Name,
            Price = Price,
            Description = Description,
            Images = new List<string>()
        };

        string folderPath = Path.Combine(_env.WebRootPath, "images");

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        foreach (var file in Images)
        {
            if (file.Length > 0)
            {
                string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                string filePath = Path.Combine(folderPath, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                modak.Images.Add("/images/" + fileName);
            }
        }

        service.AddModak(modak);

        return RedirectToPage("/Admin/Dashboard");
    }
}