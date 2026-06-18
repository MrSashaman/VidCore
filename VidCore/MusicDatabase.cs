using Microsoft.Data.Sqlite;

namespace VidCore;


 public static class MusicDatabase
{
    private const string ConnectionString = "Data Source=music.db";


    public static void Initialize()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();

        command.CommandText = @"
        CREATE TABLE IF NOT EXISTS Music (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Title TEXT NOT NULL,
            Description TEXT,
            Artist TEXT,
            Genre TEXT,
            AudioPath TEXT NOT NULL,
            DurationSeconds INTEGER DEFAULT 0,
            Likes INTEGER DEFAULT 0,
            Views INTEGER DEFAULT 0,
            UploadDate TEXT NOT NULL,
            IsActive INTEGER DEFAULT 1
        );
        
        CREATE INDEX IF NOT EXISTS idx_genre ON Music(Genre);
        CREATE INDEX IF NOT EXISTS idx_artist ON Music(Artist);
        CREATE INDEX IF NOT EXISTS idx_upload_date ON Music(UploadDate);
        CREATE INDEX IF NOT EXISTS idx_is_active ON Music(IsActive);
        ";

        command.ExecuteNonQuery();
    }


    public static int AddMusic(Music music)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();

        command.CommandText = @"
        INSERT INTO Music
        (
            Title,
            Description,
            Artist,
            Genre,
            AudioPath,
            DurationSeconds,
            Likes,
            Views,
            UploadDate,
            IsActive
        )
        VALUES
        (
            @Title,
            @Description,
            @Artist,
            @Genre,
            @AudioPath,
            @DurationSeconds,
            @Likes,
            @Views,
            @UploadDate,
            @IsActive
        );
        SELECT last_insert_rowid();
        ";

        command.Parameters.AddWithValue("@Title", music.Title ?? "");
        command.Parameters.AddWithValue("@Description", music.Description ?? "");
        command.Parameters.AddWithValue("@Artist", music.Artist ?? "Unknown Artist");
        command.Parameters.AddWithValue("@Genre", music.Genre ?? "Other");
        command.Parameters.AddWithValue("@AudioPath", music.AudioPath ?? "");
        command.Parameters.AddWithValue("@DurationSeconds", music.DurationSeconds);
        command.Parameters.AddWithValue("@Likes", music.Likes);
        command.Parameters.AddWithValue("@Views", music.Views);
        command.Parameters.AddWithValue("@UploadDate", music.UploadDate.ToString("o"));
        command.Parameters.AddWithValue("@IsActive", music.IsActive ? 1 : 0);

        var result = command.ExecuteScalar();
        return result != null ? Convert.ToInt32(result) : 0;
    }


    public static List<Music> GetAllMusic()
    {
        return GetMusicByQuery("SELECT * FROM Music WHERE IsActive = 1 ORDER BY UploadDate DESC");
    }


    public static Music? GetMusicById(int id)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Music WHERE Id = @Id AND IsActive = 1";
        command.Parameters.AddWithValue("@Id", id);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapReaderToMusic(reader);
        }

        return null;
    }


    public static List<Music> GetMusicByGenre(string genre)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Music WHERE Genre = @Genre AND IsActive = 1 ORDER BY Likes DESC";
        command.Parameters.AddWithValue("@Genre", genre);

        return ExecuteQueryAndMapResults(command);
    }


    public static List<Music> GetMusicByArtist(string artist)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Music WHERE Artist = @Artist AND IsActive = 1 ORDER BY UploadDate DESC";
        command.Parameters.AddWithValue("@Artist", artist);

        return ExecuteQueryAndMapResults(command);
    }

    public static List<Music> SearchMusic(string query)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
        SELECT * FROM Music 
        WHERE IsActive = 1 AND (
            Title LIKE @Query OR 
            Description LIKE @Query OR 
            Artist LIKE @Query
        ) 
        ORDER BY Likes DESC, UploadDate DESC
        LIMIT 100";
        
        command.Parameters.AddWithValue("@Query", $"%{query}%");

        return ExecuteQueryAndMapResults(command);
    }


    public static List<Music> GetTopMusicByLikes(int limit = 20)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = $@"
        SELECT * FROM Music 
        WHERE IsActive = 1 
        ORDER BY Likes DESC 
        LIMIT {limit}";

        return ExecuteQueryAndMapResults(command);
    }

    public static List<Music> GetPopularMusic(int limit = 20)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = $@"
        SELECT * FROM Music 
        WHERE IsActive = 1 
        ORDER BY Views DESC 
        LIMIT {limit}";

        return ExecuteQueryAndMapResults(command);
    }


    public static List<Music> GetRecentMusic(int limit = 20)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = $@"
        SELECT * FROM Music 
        WHERE IsActive = 1 
        ORDER BY UploadDate DESC 
        LIMIT {limit}";

        return ExecuteQueryAndMapResults(command);
    }

    public static List<string> GetAllGenres()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
        SELECT DISTINCT Genre FROM Music 
        WHERE IsActive = 1 AND Genre IS NOT NULL 
        ORDER BY Genre";

        var genres = new List<string>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var genre = reader.GetString(0);
            if (!string.IsNullOrEmpty(genre))
            {
                genres.Add(genre);
            }
        }

        return genres;
    }

    public static List<string> GetAllArtists()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
        SELECT DISTINCT Artist FROM Music 
        WHERE IsActive = 1 AND Artist IS NOT NULL 
        ORDER BY Artist";

        var artists = new List<string>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var artist = reader.GetString(0);
            if (!string.IsNullOrEmpty(artist))
            {
                artists.Add(artist);
            }
        }

        return artists;
    }
    public static void AddLike(int musicId)
    {
        UpdateIntField(musicId, "Likes", 1);
    }


    public static void AddView(int musicId)
    {
        UpdateIntField(musicId, "Views", 1);
    }


    public static bool UpdateMusic(Music music)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();

        command.CommandText = @"
        UPDATE Music 
        SET 
            Title = @Title,
            Description = @Description,
            Artist = @Artist,
            Genre = @Genre,
            DurationSeconds = @DurationSeconds
        WHERE Id = @Id";

        command.Parameters.AddWithValue("@Title", music.Title ?? "");
        command.Parameters.AddWithValue("@Description", music.Description ?? "");
        command.Parameters.AddWithValue("@Artist", music.Artist ?? "Unknown Artist");
        command.Parameters.AddWithValue("@Genre", music.Genre ?? "Other");
        command.Parameters.AddWithValue("@DurationSeconds", music.DurationSeconds);
        command.Parameters.AddWithValue("@Id", music.Id);

        return command.ExecuteNonQuery() > 0;
    }


    public static bool DeleteMusic(int id)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "UPDATE Music SET IsActive = 0 WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id);

        return command.ExecuteNonQuery() > 0;
    }

    public static (int TotalMusic, int TotalLikes, long TotalViews) GetStatistics()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
        SELECT 
            COUNT(*) as TotalMusic,
            COALESCE(SUM(Likes), 0) as TotalLikes,
            COALESCE(SUM(Views), 0) as TotalViews
        FROM Music 
        WHERE IsActive = 1";

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            int totalMusic = reader.GetInt32(0);
            int totalLikes = reader.GetInt32(1);
            long totalViews = reader.GetInt64(2);

            return (totalMusic, totalLikes, totalViews);
        }

        return (0, 0, 0);
    }


    private static List<Music> ExecuteQueryAndMapResults(SqliteCommand command)
    {
        var musics = new List<Music>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            musics.Add(MapReaderToMusic(reader));
        }

        return musics;
    }

    private static Music MapReaderToMusic(SqliteDataReader reader)
    {
        return new Music
        {
            Id = reader.GetInt32(0),
            Title = reader.GetString(1),
            Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
            Artist = reader.IsDBNull(3) ? "Unknown Artist" : reader.GetString(3),
            Genre = reader.IsDBNull(4) ? "Other" : reader.GetString(4),
            AudioPath = reader.GetString(5),
            DurationSeconds = reader.GetInt32(6),
            Likes = reader.GetInt32(7),
            Views = reader.GetInt32(8),
            UploadDate = DateTime.Parse(reader.GetString(9)),
            IsActive = reader.GetInt32(10) == 1
        };
    }

    private static void UpdateIntField(int musicId, string fieldName, int increment)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = $@"
        UPDATE Music 
        SET {fieldName} = {fieldName} + @Increment 
        WHERE Id = @Id";
        
        command.Parameters.AddWithValue("@Increment", increment);
        command.Parameters.AddWithValue("@Id", musicId);

        command.ExecuteNonQuery();
    }

    private static List<Music> GetMusicByQuery(string query)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = query;

        return ExecuteQueryAndMapResults(command);
    }
}
