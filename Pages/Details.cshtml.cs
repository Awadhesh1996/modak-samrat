using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class DetailsModel : PageModel
{
    public Modak Modak { get; set; } = new();

    public List<Modak> RelatedModaks { get; set; } = new();

    public IActionResult OnGet(int id)
    {
        var service = new ModakService();

        Modak =
        service.GetAllModaks()
        .FirstOrDefault(x => x.Id == id)
        ?? new Modak();

        // Product not found
        if (Modak.Id == 0)
        {
            return RedirectToPage("/Index");
        }

        // Related products
        RelatedModaks =
        service.GetAllModaks()
        .Where(x =>
            x.CategoryId == Modak.CategoryId &&
            x.Id != Modak.Id)
        .Take(6)
        .ToList();

        return Page();
    }
}