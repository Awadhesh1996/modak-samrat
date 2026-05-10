using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class EditModakModel : PageModel
{
    private readonly IWebHostEnvironment _env;

    public Modak Modak { get; set; } = new();

    public List<Category> Categories { get; set; } = new();

    public EditModakModel(IWebHostEnvironment env)
    {
        _env = env;
    }

    // LOAD PAGE
    public IActionResult OnGet(int id)
    {
        if (HttpContext.Session.GetString("AdminUser") == null)
            return RedirectToPage("/Admin/Login");

        var service = new ModakService();

        Modak =
        service.GetAllModaks()
        .FirstOrDefault(x => x.Id == id)
        ?? new Modak();

        Categories = service.GetCategories();

        return Page();
    }

    // UPDATE MODAK
    public async Task<IActionResult> OnPost(
        int Id,
        string Name,
        int Price,
        string Description,
        int CategoryId,
        List<IFormFile> Images)
    {
        var service = new ModakService();

        // Get old product
        var oldModak =
        service.GetAllModaks()
        .FirstOrDefault(x => x.Id == Id);

        var modak = new Modak
        {
            Id = Id,

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

        // NEW IMAGES
        if (Images != null && Images.Count > 0)
        {
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
        }
        else
        {
            // KEEP OLD IMAGES
            if (oldModak != null)
            {
                modak.Images = oldModak.Images;
            }
        }

        service.UpdateModak(modak);

        return RedirectToPage("/Admin/Dashboard");
    }
}