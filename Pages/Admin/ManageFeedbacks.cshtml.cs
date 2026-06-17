using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class ManageFeedbacksModel : PageModel
{
    public List<Feedback> Feedbacks { get; set; } = new();
    public double AverageRating { get; set; } = 0;

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

    public IActionResult OnPostDelete(int id)
    {
        var guard = AuthGuard();
        if (guard != null) return guard;

        using var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=modak.db");
        connection.Open();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM Feedbacks WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();

        return RedirectToPage();
    }

    private void Load()
    {
        var service = new ModakService();
        Feedbacks = service.GetAllFeedbacks();
        AverageRating = service.GetAverageRating();
    }
}
