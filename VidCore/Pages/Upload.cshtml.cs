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
        string randomId = Random.Shared.Next(100000, 999999).ToString();
        string videoFileName =
        $"{Path.GetFileNameWithoutExtension(VideoFile.FileName)}_{randomId}{Path.GetExtension(VideoFile.FileName)}";

        string thumbnailFileName =
            $"{Path.GetFileNameWithoutExtension(ThumbnailFile.FileName)}_{randomId}{Path.GetExtension(ThumbnailFile.FileName)}";

       
        if (VideoFile == null || VideoFile.Length == 0)
        {
            Console.WriteLine("Video file is null or empty!");
            return Page();
        }
        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "videos");
        
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        string path = Path.Combine(folderPath, videoFileName);        
        using (var stream = new FileStream(path, FileMode.Create))
        {
            await VideoFile.CopyToAsync(stream);
            
        }


        Console.WriteLine("Video uploaded successfully!");

        if (ThumbnailFile == null || ThumbnailFile.Length == 0)
        {
            Console.WriteLine("Thumbnail file is null or empty!");
            return Page();
        }


        string ThumbnailfolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "thumbnails");
        
        if (!Directory.Exists(ThumbnailfolderPath))
        {
            Directory.CreateDirectory(ThumbnailfolderPath);
        }
        string Thumbnailpath = Path.Combine(
            ThumbnailfolderPath,
            thumbnailFileName
        );      
        using (var stream = new FileStream(Thumbnailpath, FileMode.Create))
        {
            await ThumbnailFile.CopyToAsync(stream);
        }


        Console.WriteLine("Thumbnail uploaded successfully!");


        Console.WriteLine($"Saving video to: {path}");
        Console.WriteLine($"Video file name: {videoFileName}");
        Console.WriteLine($"Video Title: {Title}");
        Console.WriteLine($"Video Description: {Description}");

        Video video = new Video();
        {
            video.Id = VideoStorage.Videos.Count + 1;
            video.Title = Title;
            video.Description = Description;
            video.UploadDate = DateTime.Now;
            video.VideoPath = $"/videos/{videoFileName}";
            video.ThumbnailPath = $"/thumbnails/{thumbnailFileName}";
        };

        VideoStorage.Videos.Add(video);   
        return RedirectToPage("/Index");
        Console.WriteLine(VideoStorage.Videos.Count);

    }
    

}

