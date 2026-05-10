using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class DetailsModel : PageModel
{
    public Modak Modak { get; set; } = new Modak();

    public void OnGet(int id)
    {
        var service = new ModakService();
        Modak = service.GetAllModaks().FirstOrDefault(x => x.Id == id);
    }
}