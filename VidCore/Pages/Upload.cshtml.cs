using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

namespace VidCore.Pages;

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

    public void OnGet()
    {
    }

public async Task<IActionResult> OnPostUploadAsync()
{
    Console.WriteLine("Upload button clicked!");

    if (VideoFile == null || VideoFile.Length == 0)
    {
        Console.WriteLine("Video file is null or empty!");
        return Page();
    }

    string randomId = Random.Shared.Next(100000, 999999).ToString();

    string videoFileName =
        $"{Path.GetFileNameWithoutExtension(VideoFile.FileName)}_{randomId}{Path.GetExtension(VideoFile.FileName)}";

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

    Console.WriteLine("Video uploaded successfully!");

    if (ThumbnailFile != null && ThumbnailFile.Length > 0)
    {
        thumbnailFileName =
            $"{Path.GetFileNameWithoutExtension(ThumbnailFile.FileName)}_{randomId}{Path.GetExtension(ThumbnailFile.FileName)}";

        string thumbnailFolderPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "thumbnails"
        );

        Directory.CreateDirectory(thumbnailFolderPath);

        string thumbnailPath = Path.Combine(
            thumbnailFolderPath,
            thumbnailFileName
        );

        using (var stream = new FileStream(thumbnailPath, FileMode.Create))
        {
            await ThumbnailFile.CopyToAsync(stream);
        }

        Console.WriteLine("Thumbnail uploaded successfully!");
    }
    else
    {
        Console.WriteLine("Using default thumbnail");
    }

    Console.WriteLine($"Saving video to: {videoPath}");
    Console.WriteLine($"Video file name: {videoFileName}");
    Console.WriteLine($"Video Title: {Title}");
    Console.WriteLine($"Video Description: {Description}");

    Video video = new Video
    {
        Title = Title,
        Description = Description,
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

