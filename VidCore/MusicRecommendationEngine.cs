namespace VidCore;

public static class MusicRecommendationEngine
{

    public static List<Music> GetRecommendationsByGenre(string genre, int limit = 10, Music? excludeMusic = null)
    {
        var recommendations = MusicDatabase.GetMusicByGenre(genre)
            .OrderByDescending(m => m.Likes)
            .ThenByDescending(m => m.Views)
            .Take(limit * 2)
            .ToList();

        if (excludeMusic != null)
        {
            recommendations = recommendations
                .Where(m => m.Id != excludeMusic.Id)
                .Take(limit)
                .ToList();
        }

        return recommendations;
    }


    public static List<Music> GetRecommendationsByArtist(string artist, int limit = 10, Music? excludeMusic = null)
    {
        var recommendations = MusicDatabase.GetMusicByArtist(artist)
            .OrderByDescending(m => m.UploadDate)
            .Take(limit)
            .ToList();

        if (excludeMusic != null)
        {
            recommendations = recommendations
                .Where(m => m.Id != excludeMusic.Id)
                .ToList();
        }

        return recommendations;
    }


    public static List<Music> GetComplementaryMusic(Music music, int limit = 10)
    {
        var complementary = new List<Music>();

        if (!string.IsNullOrEmpty(music.Genre))
        {
            complementary.AddRange(
                MusicDatabase.GetMusicByGenre(music.Genre)
                    .Where(m => m.Id != music.Id)
                    .OrderByDescending(m => m.Likes + m.Views)
                    .Take(limit / 2)
            );
        }

        if (!string.IsNullOrEmpty(music.Artist))
        {
            complementary.AddRange(
                MusicDatabase.GetMusicByArtist(music.Artist)
                    .Where(m => m.Id != music.Id && !complementary.Contains(m))
                    .OrderByDescending(m => m.Likes + m.Views)
                    .Take(limit / 2)
            );
        }

        return complementary.Take(limit).ToList();
    }


    public static List<Music> GetNewMusicInGenre(string genre, int limit = 10)
    {
        return MusicDatabase.GetMusicByGenre(genre)
            .OrderByDescending(m => m.UploadDate)
            .Take(limit)
            .ToList();
    }

   
    public static (List<Music> Popular, List<Music> Recent, List<string> GenresToExplore) GetOnboardingRecommendations()
    {
        var popular = MusicDatabase.GetTopMusicByLikes(8);
        var recent = MusicDatabase.GetRecentMusic(8);
        var genres = MusicDatabase.GetAllGenres();

        return (popular, recent, genres);
    }


    public static double CalculateMusicScore(Music music)
    {
        double baseScore = 5.0;

        baseScore += Math.Min(3.0, music.Likes / 100.0);

        baseScore += Math.Min(1.5, music.Views / 1000.0);

        var daysOld = (DateTime.Now - music.UploadDate).TotalDays;
        if (daysOld < 7)
            baseScore += 0.5;

        return Math.Min(10.0, baseScore);
    }


    public static List<Music> GetMonthlyHits(string? genre = null)
    {
        var thirtyDaysAgo = DateTime.Now.AddDays(-30);
        var allMusic = string.IsNullOrEmpty(genre)
            ? MusicDatabase.GetAccessibleMusic(null)
            : MusicDatabase.GetMusicByGenre(genre);

        return allMusic
            .Where(m => m.UploadDate >= thirtyDaysAgo)
            .OrderByDescending(m => m.Likes + m.Views)
            .Take(20)
            .ToList();
    }


    public static List<Music> GetMusicByMood(string mood)
    {
        var genresByMood = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "happy", new[] { "Pop", "Electronic", "Rock" } },
            { "sad", new[] { "Blues", "Classical", "Folk" } },
            { "energetic", new[] { "Hip-Hop", "Electronic", "Rock" } },
            { "calm", new[] { "Classical", "Jazz", "Folk" } },
            { "party", new[] { "Pop", "Electronic", "Hip-Hop" } },
            { "focus", new[] { "Classical", "Jazz", "Electronic" } }
        };

        if (!genresByMood.TryGetValue(mood, out var genres))
        {
            return MusicDatabase.GetRecentMusic(20);
        }

        var musicByMood = new List<Music>();

        foreach (var genre in genres)
        {
            musicByMood.AddRange(
                MusicDatabase.GetMusicByGenre(genre)
                    .OrderByDescending(m => m.Likes)
                    .Take(7)
            );
        }

        return musicByMood.Distinct().Take(20).ToList();
    }


    public static List<Music> GeneratePlaylist(Music startingMusic, int playlistLength = 20)
    {
        var playlist = new List<Music> { startingMusic };

        var recommended = GetComplementaryMusic(startingMusic, playlistLength - 1);
        playlist.AddRange(recommended);

        if (playlist.Count < playlistLength)
        {
            var additional = MusicDatabase.GetTopMusicByLikes(playlistLength)
                .Where(m => !playlist.Contains(m))
                .Take(playlistLength - playlist.Count);

            playlist.AddRange(additional);
        }

        return playlist.Take(playlistLength).ToList();
    }
}
