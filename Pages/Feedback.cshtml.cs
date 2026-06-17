using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class FeedbackModel : PageModel
{
    // ── DISPLAY PROPERTIES ───────────────────
    public List<Feedback> Feedbacks { get; set; } = new();
    public List<Modak> Modaks { get; set; } = new();
    public double AverageRating { get; set; } = 0;
    public bool Success { get; set; } = false;

    // ── FORM BOUND PROPERTIES ────────────────
    [BindProperty]
    public string Name { get; set; } = "";

    [BindProperty]
    public string? Phone { get; set; }

    [BindProperty]
    public string? ModakName { get; set; }

    [BindProperty]
    public int Rating { get; set; }

    [BindProperty]
    public string[]? Tags { get; set; }

    [BindProperty]
    public string Message { get; set; } = "";

    [BindProperty]
    public string? Occasion { get; set; }

    [BindProperty]
    public bool WouldRecommend { get; set; } = true;

    // ── GET ──────────────────────────────────
    public IActionResult OnGet()
    {
        Load();
        return Page();
    }

    // ── POST ─────────────────────────────────
    public IActionResult OnPost()
    {
        // Basic validation
        if (string.IsNullOrWhiteSpace(Name) ||
            string.IsNullOrWhiteSpace(Message) ||
            Rating < 1 || Rating > 5)
        {
            Load();
            return Page();
        }

        var service = new ModakService();
        service.AddFeedback(new Feedback
        {
            Name = Name.Trim(),
            Phone = Phone?.Trim() ?? "",
            ModakName = ModakName?.Trim() ?? "",
            Rating = Rating,
            Tags = Tags != null ? string.Join(", ", Tags) : "",
            Message = Message.Trim(),
            Occasion = Occasion?.Trim() ?? "",
            WouldRecommend = WouldRecommend
        });

        Success = true;
        Load();
        return Page();
    }

    // ── HELPER ───────────────────────────────
    private void Load()
    {
        var service = new ModakService();
        Modaks = service.GetAllModaks();
        Feedbacks = service.GetAllFeedbacks();
        AverageRating = service.GetAverageRating();
    }
}
