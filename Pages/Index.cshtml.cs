using Microsoft.AspNetCore.Mvc.RazorPages;

public class IndexModel : PageModel
{
    public List<Modak> Modaks { get; set; } = new();

    public List<Category> Categories { get; set; } = new();

    public void OnGet()
    {
        var service = new ModakService();

        Modaks = service.GetAllModaks();

        Categories = service.GetCategories();
    }
}