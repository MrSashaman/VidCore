using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VidCore.Pages;

public class MusicPageModel : PageModel
{

    public Music CurrentMusic { get; set; }

    public List<Music> Musics { get; set; }

    public void OnGet()
    {
        Musics = MusicStorage.Musics;
        Console.WriteLine($"Music count: {Musics.Count}");

    }



    public IActionResult OnPostLikeMus(int id, int Likes )
    {
        CurrentMusic = MusicStorage.Musics.FirstOrDefault(v => v.Id == id);

        if (CurrentMusic == null)
        {
            Console.WriteLine("Nihua");
            return NotFound();
        }

        CurrentMusic.Likes += 1;

        return Page();
    }
}
