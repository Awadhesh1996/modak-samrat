using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class ManageSettingsModel : PageModel
{
    public FestiveEvent FestiveEvent { get; set; } = new();
    public List<TrustBadge> TrustBadges { get; set; } = new();
    public string Message { get; set; } = "";
    public bool IsError { get; set; } = false;

    private IActionResult AuthGuard()
    {
        if (HttpContext.Session.GetString("AdminUser") == null)
            return RedirectToPage("/Admin/Login");
        return null!;
    }

    public IActionResult OnGet()
    {
        var guard = AuthGuard();
        if (guard != null) return guard;

        Load();
        return Page();
    }

    // ── TOGGLE FESTIVE ON/OFF ────────────────
    public IActionResult OnPostToggleFestive()
    {
        var guard = AuthGuard();
        if (guard != null) return guard;

        var service = new ModakService();
        var ev = service.GetFestiveEventForAdmin();
        ev.IsActive = !ev.IsActive;
        service.UpdateFestiveEvent(ev);

        Message = ev.IsActive
            ? "✅ Festive banner is now LIVE on the store."
            : "🔒 Festive banner has been hidden.";

        Load();
        return Page();
    }

    // ── SAVE FESTIVE EVENT ───────────────────
    public async Task<IActionResult> OnPostSaveFestiveAsync(
        string Title,
        string Description,
        DateTime EventDate,
        IFormFile? FestiveImage)
    {
        var guard = AuthGuard();
        if (guard != null) return guard;

        var service = new ModakService();
        var ev = service.GetFestiveEventForAdmin();

        ev.Title = Title;
        ev.Description = Description;
        ev.EventDate = EventDate;

        // Handle image upload
        if (FestiveImage != null && FestiveImage.Length > 0)
        {
            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "festive");
            Directory.CreateDirectory(uploadsDir);

            var ext = Path.GetExtension(FestiveImage.FileName);
            var fileName = "festive_" + DateTime.Now.Ticks + ext;
            var filePath = Path.Combine(uploadsDir, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await FestiveImage.CopyToAsync(stream);

            ev.ImagePath = "/images/festive/" + fileName;
        }

        service.UpdateFestiveEvent(ev);
        Message = "✅ Festive event saved successfully!";

        Load();
        return Page();
    }

    // ── SAVE TRUST BADGES ───────────────────
    public IActionResult OnPostSaveBadges(List<TrustBadge> Badges)
    {
        var guard = AuthGuard();
        if (guard != null) return guard;

        var service = new ModakService();
        foreach (var badge in Badges)
        {
            if (badge.Id > 0)
                service.UpdateTrustBadge(badge);
        }

        Message = "✅ Trust badges updated successfully!";
        Load();
        return Page();
    }

    private void Load()
    {
        var service = new ModakService();
        FestiveEvent = service.GetFestiveEventForAdmin();
        TrustBadges = service.GetTrustBadges();
    }
}
