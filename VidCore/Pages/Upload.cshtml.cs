using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

namespace VidCore.Pages;

[Authorize]
public class UploadModel : PageModel
{
    [BindProperty]
    public string Title { get; set; }

    [BindProperty]
    public IFormFile VideoFile { get; set; }

    [BindProperty]
    public IFormFile ThumbnailFile { get; set; }

    [BindProperty]
    public string Description { get; set; }

    [BindProperty]
    public bool IsPrivate { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostUploadAsync()
    {
        if (VideoFile == null || VideoFile.Length == 0)
        {
            return Page();
        }

        string randomId = Random.Shared.Next(100000, 999999).ToString();

        string videoFileName = $"{Path.GetFileNameWithoutExtension(VideoFile.FileName)}_{randomId}{Path.GetExtension(VideoFile.FileName)}";
        string thumbnailFileName = "nullthumbail.png";

        string folderPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "videos"
        );

        Directory.CreateDirectory(folderPath);

        string videoPath = Path.Combine(folderPath, videoFileName);

        using (var stream = new FileStream(videoPath, FileMode.Create))
        {
            await VideoFile.CopyToAsync(stream);
        }

        if (ThumbnailFile != null && ThumbnailFile.Length > 0)
        {
            thumbnailFileName = $"{Path.GetFileNameWithoutExtension(ThumbnailFile.FileName)}_{randomId}{Path.GetExtension(ThumbnailFile.FileName)}";

            string thumbnailFolderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "thumbnails"
            );

            Directory.CreateDirectory(thumbnailFolderPath);

            string thumbnailPath = Path.Combine(thumbnailFolderPath, thumbnailFileName);

            using (var stream = new FileStream(thumbnailPath, FileMode.Create))
            {
                await ThumbnailFile.CopyToAsync(stream);
            }
        }

        Video video = new Video
        {
            Title = Title,
            Description = Description,
            Owner = User.Identity?.Name ?? string.Empty,
            IsPrivate = IsPrivate,
            UploadDate = DateTime.Now,
            VideoPath = $"/videos/{videoFileName}",
            ThumbnailPath = ThumbnailFile != null
                ? $"/thumbnails/{thumbnailFileName}"
                : "/spic/nullthumbail.png"
        };

        Database.AddVideo(video);

        return RedirectToPage("/Index");
    }
}

