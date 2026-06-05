using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VidCore.Pages;

public class IndexModel : PageModel
{

    public List<Video> Videos { get; set; }

    public void OnGet()
    {
        Videos = VideoStorage.Videos;
        Console.WriteLine($"Videos count: {Videos.Count}");

    }


}
