using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class IndexModel : PageModel
{
    public List<Modak> Modaks { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<TrustBadge> TrustBadges { get; set; } = new();
    public FestiveEvent? FestiveEvent { get; set; }
    public List<Feedback> LatestFeedbacks { get; set; } = new();

    public IActionResult OnGet()
    {
        var service = new ModakService();
        Modaks = service.GetAllModaks();
        Categories = service.GetCategories();
        TrustBadges = service.GetTrustBadges();
        FestiveEvent = service.GetActiveFestiveEvent();
        LatestFeedbacks = service.GetLatestFeedbacks(4);
        return Page();
    }

    // Mini feedback form handler
    public IActionResult OnPostFeedback(
        string FbName,
        string FbModak,
        int FbRating,
        string FbMessage)
    {
        if (!string.IsNullOrWhiteSpace(FbName) &&
            !string.IsNullOrWhiteSpace(FbMessage) &&
            FbRating >= 1 && FbRating <= 5)
        {
            var service = new ModakService();
            service.AddFeedback(new Feedback
            {
                Name = FbName,
                ModakName = FbModak ?? "",
                Rating = FbRating,
                Message = FbMessage
            });
        }

        // Redirect back with success flag
        return RedirectToPage(new { feedbackSuccess = true });
    }

    [BindProperty(SupportsGet = true)]
    public bool FeedbackSuccess { get; set; }
}
