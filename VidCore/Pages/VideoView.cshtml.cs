using System.Collections.Specialized;
using System.Security.Cryptography.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VidCore.Pages;

public class VideoViewModel : PageModel
{
    public Video CurrentVideo { get; set; }

    [BindProperty]
    public string Text { get; set; }

    [BindProperty]
    public int id {get; set; }

    [BindProperty]
    public string CText { get; set; }

    [BindProperty]
    public DateTime UploadDate { get; set; }

    [BindProperty]
    public string Author { get; set; }

    public List<Comment> Comments { get; set; } = new();
    public IActionResult OnGet(int id)
    {
        CurrentVideo = Database.GetVideoById(id);

        if (CurrentVideo == null)
        {
            return RedirectToPage("/Error404");
        }

        Comments = CommentDatabase.GetCommentsByVideoId(id);
        Database.AddView(id);

        return Page();
    }




    public IActionResult OnPostComment(int id)
    {
        if (string.IsNullOrWhiteSpace(CText))
        {
            return RedirectToPage("/VideoView", new { id });
        }

        var comment = new Comment
        {
            Text = CText,
            VideoId = id,
            Author = "RandomPeople" + Random.Shared.Next(0, 999999),
            UploadDate = DateTime.Now
        };

        CommentDatabase.AddComment(comment);

        return RedirectToPage("/VideoView", new { id });
    }

    public string GetTimeSinceUpload()
    {
        TimeSpan diff = DateTime.Now - CurrentVideo.UploadDate;

        if (diff.TotalSeconds < 10)
            return "только что";

        if (diff.TotalMinutes < 1)
            return $"{(int)diff.TotalSeconds} сек. назад";

        if (diff.TotalHours < 1)
            return $"{(int)diff.TotalMinutes} мин. назад";

        if (diff.TotalDays < 1)
            return $"{(int)diff.TotalHours} ч. назад";

        if (diff.TotalDays < 30)
            return $"{(int)diff.TotalDays} дн. назад";

        if (diff.TotalDays < 365)
            return $"{(int)(diff.TotalDays / 30)} мес. назад";

        return $"{(int)(diff.TotalDays / 365)} г. назад";
    }

    
    public IActionResult OnPostDelete(int id, string videoPath, string thumbnailPath)
    {
        RemoveVideo(id, videoPath, thumbnailPath);

        return RedirectToPage("/Index");
    }

    public IActionResult OnPostLike(int id)
    {
        CurrentVideo = Database.GetVideoById(id);

        if (CurrentVideo == null)
        {
            return NotFound();
        }

        Database.AddLike(id);
        return RedirectToPage("/VideoView", new { id });
    }


    public IActionResult OnPostEdit(int id, string title, string description)
    {
        var video = Database.GetVideoById(id);
        if (video != null)
        {
            Database.UpdateVideo(id, title, description);
        }

        return RedirectToPage("/Index");
    }

    private void RemoveVideo(int id, string videoPath, string thumbnailPath)
    {

        var video = Database.GetVideoById(id);
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

            bool isDefaultThumbnail =
                video.ThumbnailPath.Contains("nullthumbail.png");

            if (!isDefaultThumbnail)
            {
                if (System.IO.File.Exists(fullThumbnailPath))
                {
                    System.IO.File.Delete(fullThumbnailPath);
                }
            }
            else
            {
                Console.WriteLine("Default thumbnail, skip delete");
            }

            CommentDatabase.DeleteCommentsByVideoId(id);
            Database.DeleteVideo(id);
        }
    }
}
