using Microsoft.AspNetCore.Mvc.RazorPages;

public class IndexModel : PageModel
{
    public List<Modak> Modaks { get; set; } = new List<Modak>();

    public void OnGet()
    {
        var service = new ModakService();
        Modaks = service.GetAllModaks();
    }
}