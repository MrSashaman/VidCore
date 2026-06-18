namespace VidCore;


public static class MusicUtilities
{

    public static string FormatDuration(int durationSeconds)
    {
        if (durationSeconds < 0)
            return "00:00";

        int minutes = durationSeconds / 60;
        int seconds = durationSeconds % 60;

        return $"{minutes:D2}:{seconds:D2}";
    }


    public static string GetGenreEmoji(string genre)
    {
        return genre?.ToLower() switch
        {
            "rock" => "🎸",
            "pop" => "🎤",
            "hip-hop" => "🎧",
            "jazz" => "🎷",
            "classical" => "🎻",
            "electronic" => "🎹",
            "folk" => "🪕",
            "metal" => "🔥",
            "blues" => "💙",
            "reggae" => "🌴",
            _ => "🎵"
        };
    }


    public static bool IsValidAudioExtension(string fileName)
    {
        var allowedExtensions = new[] { ".mp3", ".wav", ".ogg", ".m4a", ".flac" };
        var fileExtension = Path.GetExtension(fileName).ToLower();

        return allowedExtensions.Contains(fileExtension);
    }


    public static bool IsValidAudioFileSize(long fileSizeBytes, long maxSizeBytes = 100 * 1024 * 1024)
    {
        return fileSizeBytes > 0 && fileSizeBytes <= maxSizeBytes;
    }


    public static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }


    public static string GenerateSafeFileName(string originalFileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var safeName = new string(originalFileName
            .Where(c => !invalidChars.Contains(c))
            .ToArray());

        string randomId = Random.Shared.Next(100000, 999999).ToString();
        string extension = Path.GetExtension(safeName);
        string nameWithoutExtension = Path.GetFileNameWithoutExtension(safeName);

        nameWithoutExtension = nameWithoutExtension.Length > 50 
            ? nameWithoutExtension.Substring(0, 50) 
            : nameWithoutExtension;

        return $"{nameWithoutExtension}_{randomId}{extension}";
    }


    public static string GetAudioMimeType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLower();

        return extension switch
        {
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".ogg" => "audio/ogg",
            ".m4a" => "audio/mp4",
            ".flac" => "audio/flac",
            _ => "audio/mpeg"
        };
    }


    public static bool IsPopular(Music music, int minLikes = 100)
    {
        return music.Likes >= minLikes;
    }


    public static string GetRelativeUploadTime(DateTime uploadDate)
    {
        var timeSince = DateTime.Now - uploadDate;

        if (timeSince.TotalSeconds < 60)
            return "только что";
        else if (timeSince.TotalMinutes < 60)
            return $"{(int)timeSince.TotalMinutes} мин. назад";
        else if (timeSince.TotalHours < 24)
            return $"{(int)timeSince.TotalHours} ч. назад";
        else if (timeSince.TotalDays < 7)
            return $"{(int)timeSince.TotalDays} дн. назад";
        else if (timeSince.TotalDays < 30)
            return $"{(int)(timeSince.TotalDays / 7)} нед. назад";
        else
            return uploadDate.ToString("d MMMM yyyy");
    }
}
