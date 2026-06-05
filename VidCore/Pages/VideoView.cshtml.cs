using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VidCore.Pages;

public class VideoViewModel : PageModel
{
    public Video CurrentVideo { get; set; }

    public IActionResult OnGet(int id)
    {
        CurrentVideo = VideoStorage.Videos
            .FirstOrDefault(v => v.Id == id);

        if (CurrentVideo == null)
        {
            return NotFound();
        }



        CurrentVideo.Views++;
        return Page();
    }

    public string  GetTimeSinceUpload()
    {
        DateTime now = DateTime.Now;
        TimeSpan difference = now - CurrentVideo.UploadDate;
        return difference.TotalMinutes.ToString("F1") + " minutes ago";  
    }

    
    public IActionResult OnPostDelete(int id, string videoPath, string thumbnailPath)
    {
        RemoveVideo(id, videoPath, thumbnailPath);

        return RedirectToPage("/Index");
    }

    public IActionResult OnPostEdit(int id, string title, string description)
    {
        var video = VideoStorage.Videos.FirstOrDefault(v => v.Id == id);
        if (video != null)
        {
            video.Title = title;
            video.Description = description;
        }

        return RedirectToPage("/Index");
    }

    private void RemoveVideo(int id, string videoPath, string thumbnailPath)
    {

        var video = VideoStorage.Videos.FirstOrDefault(v => v.Id == id);
        if (video != null)
        { 


            Console.WriteLine($"VideoPath: {video.VideoPath}");
            Console.WriteLine($"ThumbnailPath: {video.ThumbnailPath}");

            var fullVideoPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            video.VideoPath.TrimStart('/')
        );
            var fullThumbnailPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            video.ThumbnailPath.TrimStart('/')
        );

            if (System.IO.File.Exists(fullVideoPath))
            {
                System.IO.File.Delete(fullVideoPath);
            }

            if (System.IO.File.Exists(fullThumbnailPath))
            {
                System.IO.File.Delete(fullThumbnailPath);
            }

            VideoStorage.Videos.Remove(video);
        }
    }
}