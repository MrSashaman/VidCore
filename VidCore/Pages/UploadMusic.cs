using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

namespace VidCore.Pages;

[Authorize]
public class UploadMusicModel : PageModel
{
    [BindProperty]
    public string Title { get; set; }

    [BindProperty]
    public IFormFile AudioFile { get; set; }

    [BindProperty]
    public string Description { get; set; }

    [BindProperty]
    public string Artist { get; set; }

    [BindProperty]
    public string Genre { get; set; }

    [BindProperty]
    public bool IsPrivate { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            if (AudioFile == null || AudioFile.Length == 0)
            {
                ModelState.AddModelError("AudioFile", "Пожалуйста, выберите аудиофайл");
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Title))
            {
                ModelState.AddModelError("Title", "Название обязательно");
                return Page();
            }

            var allowedExtensions = new[] { ".mp3", ".wav", ".ogg", ".m4a", ".flac" };
            var fileExtension = Path.GetExtension(AudioFile.FileName).ToLower();
            
            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("AudioFile", "Поддерживаются только форматы: MP3, WAV, OGG, M4A, FLAC");
                return Page();
            }

            const long maxFileSize = 100 * 1024 * 1024;
            if (AudioFile.Length > maxFileSize)
            {
                ModelState.AddModelError("AudioFile", "Размер файла не должен превышать 100 МБ");
                return Page();
            }

            string randomId = Random.Shared.Next(100000, 999999).ToString();
            string audioFileName = $"{Path.GetFileNameWithoutExtension(AudioFile.FileName)}_{randomId}{fileExtension}";

            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "musics");
            
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, audioFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await AudioFile.CopyToAsync(stream);
            }

            int durationSeconds = 0;

            var music = new Music
            {
                Title = Title.Trim(),
                Description = Description?.Trim() ?? string.Empty,
                Artist = string.IsNullOrWhiteSpace(Artist) ? "Unknown Artist" : Artist.Trim(),
                Genre = string.IsNullOrWhiteSpace(Genre) ? "Other" : Genre.Trim(),
                Owner = User.Identity?.Name ?? string.Empty,
                IsPrivate = IsPrivate,
                AudioPath = $"/musics/{audioFileName}",
                DurationSeconds = durationSeconds,
                UploadDate = DateTime.Now,
                Views = 0,
                Likes = 0,
                IsActive = true
            };

            int musicId = MusicDatabase.AddMusic(music);

            Console.WriteLine($"✓ Музыка успешно загружена");
            Console.WriteLine($"  ID: {musicId}");
            Console.WriteLine($"  Название: {Title}");
            Console.WriteLine($"  Исполнитель: {Artist}");
            Console.WriteLine($"  Жанр: {Genre}");
            Console.WriteLine($"  Файл: {audioFileName}");

            return RedirectToPage("/MusicPage");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Ошибка при загрузке музыки: {ex.Message}");
            ModelState.AddModelError("", "Произошла ошибка при загрузке файла. Пожалуйста, попробуйте позже.");
            return Page();
        }
    }
}

