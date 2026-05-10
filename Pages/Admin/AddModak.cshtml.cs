using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class AddModakModel : PageModel
{
    private readonly IWebHostEnvironment _env;

    public List<Category> Categories { get; set; } = new();

    public AddModakModel(IWebHostEnvironment env)
    {
        _env = env;
    }

    // LOAD PAGE
    public IActionResult OnGet()
    {
        if (HttpContext.Session.GetString("AdminUser") == null)
            return RedirectToPage("/Admin/Login");

        var service = new ModakService();

        // LOAD CATEGORIES
        Categories = service.GetCategories();

        return Page();
    }

    // SAVE MODAK
    public async Task<IActionResult> OnPost(
        string Name,
        int Price,
        string Description,
        int CategoryId,
        List<IFormFile> Images)
    {
        var service = new ModakService();

        var modak = new Modak
        {
            Name = Name,

            Price = Price,

            Description = Description,

            CategoryId = CategoryId,

            Images = new List<string>()
        };

        string folderPath =
        Path.Combine(_env.WebRootPath, "images");

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        foreach (var file in Images)
        {
            if (file.Length > 0)
            {
                string fileName =
                Guid.NewGuid()
                + Path.GetExtension(file.FileName);

                string filePath =
                Path.Combine(folderPath, fileName);

                using var stream =
                new FileStream(filePath, FileMode.Create);

                await file.CopyToAsync(stream);

                modak.Images.Add("/images/" + fileName);
            }
        }

        service.AddModak(modak);

        return RedirectToPage("/Admin/Dashboard");
    }
}