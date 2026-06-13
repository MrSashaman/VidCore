using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

namespace VidCore.Pages;

public class UploadMusicModel : PageModel
{

    [BindProperty]
    public string Title { get; set; }

    [BindProperty]
    public IFormFile AudioFile { get; set; }

    [BindProperty]
    public string Description { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostUploadAsync()
{
        Console.WriteLine("Upload button clicked!");
        string randomId = Random.Shared.Next(100000, 999999).ToString();
        string audioFileName = $"{Path.GetFileNameWithoutExtension(AudioFile.FileName)}_{randomId}{Path.GetExtension(AudioFile.FileName)}";



       
        if (AudioFile == null || AudioFile.Length == 0)
        {
            Console.WriteLine("Video file is null or empty!");
            return Page();
        }
        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "musics");
        
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        string path = Path.Combine(folderPath, audioFileName);        
        using (var stream = new FileStream(path, FileMode.Create))
        {
            await AudioFile.CopyToAsync(stream);
            
        }


        Console.WriteLine("Video uploaded successfully!");




        Console.WriteLine($"Saving video to: {path}");
        Console.WriteLine($"Video file name: {audioFileName}");
        Console.WriteLine($"Video Title: {Title}");
        Console.WriteLine($"Video Description: {Description}");

        Music music = new Music();
        {
            music.Id = MusicStorage.Musics.Count + 1;
            music.Title = Title;
            music.Description = Description;
            music.UploadDate = DateTime.Now;
            music.AudioPath = $"/musics/{audioFileName}";
        };

        MusicStorage.Musics.Add(music);   
        return RedirectToPage("/Index");
        Console.WriteLine(MusicStorage.Musics.Count);

    }
    

}

