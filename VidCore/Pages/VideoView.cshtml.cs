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
    public DateTime UploadDate { get; set; }

    [BindProperty]
    public string Author { get; set; }


    public List<Comment> Comments { get; set; }
    public IActionResult OnGet(int id)
    {
        CurrentVideo = VideoStorage.Videos
            .FirstOrDefault(v => v.Id == id);

        if (CurrentVideo == null)
        {
            return NotFound();
        }



        
        Comments = CommentStorage.Comments;

        CurrentVideo.Views++;
        return Page();
    }

       public async Task<IActionResult> OnPostCommentasync()
    {
        Console.WriteLine("Comment share start!");
        int randomId = Random.Shared.Next(100000, 999999);

        Comment comment = new Comment();
        {
            comment.id = randomId;
            comment.Text = "hey, its so good you know!";
            comment.Author = "RandomPeople" + Random.Shared.Next(0, 999999);
            comment.UploadDate = DateTime.Now;
        };

        Comments.Add(comment);
        return RedirectToPage("/Index");
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

    public IActionResult OnPostLike(int id, int Likes )
    {
        CurrentVideo = VideoStorage.Videos.FirstOrDefault(v => v.Id == id);

        if (CurrentVideo == null)
        {
            return NotFound();
        }

        CurrentVideo.Likes += 1;

        return Page();
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