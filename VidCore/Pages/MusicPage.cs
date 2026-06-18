using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VidCore.Pages;

public class MusicPageModel : PageModel
{
    public List<Music> Musics { get; set; }

    public void OnGet()
    {
        Musics = MusicDatabase.GetAllMusic();
    }

    public IActionResult OnPostLike(int id)
    {
        MusicDatabase.AddLike(id);
        var music = MusicDatabase.GetMusicById(id);
        Console.WriteLine($"✓ Лайк добавлен: {music?.Title}");
        return RedirectToPage();
    }

    public IActionResult OnPostDelete(int id)
    {
        MusicDatabase.DeleteMusic(id);
        Console.WriteLine($"✓ Музыка с ID {id} удалена");
        return RedirectToPage();
    }
}
