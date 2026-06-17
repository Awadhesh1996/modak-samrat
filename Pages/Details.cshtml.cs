using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class DetailsModel : PageModel
{
    public Modak Modak { get; set; } = new();
    public List<Modak> RelatedModaks { get; set; } = new();
    public List<Feedback> Feedbacks { get; set; } = new();
    public double AverageRating { get; set; } = 0;
    public int ReviewCount { get; set; } = 0;

    public IActionResult OnGet(int id)
    {
        var service = new ModakService();

        var modak = service.GetModakById(id);
        if (modak == null)
            return RedirectToPage("/Index");

        Modak = modak;

        // Related modaks — same category, exclude current
        RelatedModaks = service.GetAllModaks()
            .Where(m => m.CategoryId == Modak.CategoryId && m.Id != Modak.Id)
            .Take(4)
            .ToList();

        // Feedbacks for this specific modak
        Feedbacks = service.GetAllFeedbacks()
            .Where(f => !string.IsNullOrEmpty(f.ModakName) &&
                        f.ModakName.Equals(Modak.Name, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(f => f.CreatedAt)
            .ToList();

        ReviewCount = Feedbacks.Count;

        AverageRating = Feedbacks.Any()
            ? Feedbacks.Average(f => f.Rating)
            : Modak.AverageRating;

        return Page();
    }
}
