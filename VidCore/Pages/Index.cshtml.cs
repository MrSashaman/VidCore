using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VidCore.Pages;

public class IndexModel : PageModel
{

    public List<Video> Videos { get; set; }
    [BindProperty(SupportsGet = true)]
    public string Search { get; set; }
    
    public void OnGet()
    {
        Videos = Database.GetVideos();

        if (!string.IsNullOrWhiteSpace(Search))
        {
            Videos = Videos
                .Where(v =>
                    v.Title.Contains(Search, StringComparison.OrdinalIgnoreCase)
                    ||
                    v.Description.Contains(Search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }


}
