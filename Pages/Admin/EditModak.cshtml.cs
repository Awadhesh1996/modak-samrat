using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class EditModakModel : PageModel
{
    private readonly IWebHostEnvironment _env;

    public Modak Modak { get; set; } = new Modak();

    public EditModakModel(IWebHostEnvironment env)
    {
        _env = env;
    }

    public IActionResult OnGet(int id)
    {
        if (HttpContext.Session.GetString("AdminUser") == null)
            return RedirectToPage("/Admin/Login");

        var service = new ModakService();
        Modak = service.GetAllModaks().FirstOrDefault(x => x.Id == id);

        return Page();
    }

    public async Task<IActionResult> OnPost(int Id, string Name, int Price, string Description, List<IFormFile> Images)
    {
        var service = new ModakService();

        var modak = new Modak
        {
            Id = Id,
            Name = Name,
            Price = Price,
            Description = Description,
            Images = new List<string>()
        };

        string folderPath = Path.Combine(_env.WebRootPath, "images");

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        // 🔥 If new images uploaded → replace old images
        if (Images != null && Images.Count > 0)
        {
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
        }

        service.UpdateModak(modak);

        return RedirectToPage("/Admin/Dashboard");
    }
}