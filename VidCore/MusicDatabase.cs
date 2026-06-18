using Microsoft.Data.Sqlite;

namespace VidCore;

public static class MusicDatabase
{
    private const string ConnectionString = "Data Source=music.db";

    public static void Initialize()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
        CREATE TABLE IF NOT EXISTS Music (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Title TEXT NOT NULL,
            Description TEXT,
            Artist TEXT,
            Genre TEXT,
            Owner TEXT NOT NULL DEFAULT '',
            IsPrivate INTEGER DEFAULT 0,
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

        EnsureColumnExists(connection, "Music", "Owner", "Owner TEXT NOT NULL DEFAULT ''");
        EnsureColumnExists(connection, "Music", "IsPrivate", "IsPrivate INTEGER DEFAULT 0");
    }

    public static int AddMusic(Music music)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
        INSERT INTO Music
        (
            Title,
            Description,
            Artist,
            Genre,
            Owner,
            IsPrivate,
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
            @Owner,
            @IsPrivate,
            @AudioPath,
            @DurationSeconds,
            @Likes,
            @Views,
            @UploadDate,
            @IsActive
        );
        SELECT last_insert_rowid();
        ";

        command.Parameters.AddWithValue("@Title", music.Title ?? string.Empty);
        command.Parameters.AddWithValue("@Description", music.Description ?? string.Empty);
        command.Parameters.AddWithValue("@Artist", music.Artist ?? "Unknown Artist");
        command.Parameters.AddWithValue("@Genre", music.Genre ?? "Other");
        command.Parameters.AddWithValue("@Owner", music.Owner ?? string.Empty);
        command.Parameters.AddWithValue("@IsPrivate", music.IsPrivate ? 1 : 0);
        command.Parameters.AddWithValue("@AudioPath", music.AudioPath ?? string.Empty);
        command.Parameters.AddWithValue("@DurationSeconds", music.DurationSeconds);
        command.Parameters.AddWithValue("@Likes", music.Likes);
        command.Parameters.AddWithValue("@Views", music.Views);
        command.Parameters.AddWithValue("@UploadDate", music.UploadDate.ToString("o"));
        command.Parameters.AddWithValue("@IsActive", music.IsActive ? 1 : 0);

        var result = command.ExecuteScalar();
        return result != null ? Convert.ToInt32(result) : 0;
    }

    public static List<Music> GetAccessibleMusic(string? userName)
    {
        var sql = string.IsNullOrWhiteSpace(userName)
            ? "SELECT * FROM Music WHERE IsActive = 1 AND IsPrivate = 0 ORDER BY UploadDate DESC"
            : "SELECT * FROM Music WHERE IsActive = 1 AND (IsPrivate = 0 OR Owner = @Owner) ORDER BY UploadDate DESC";

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        if (!string.IsNullOrWhiteSpace(userName))
        {
            command.Parameters.AddWithValue("@Owner", userName);
        }

        return ExecuteQueryAndMapResults(command);
    }

    public static Music? GetMusicById(int id)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Music WHERE Id = @Id AND IsActive = 1";
        command.Parameters.AddWithValue("@Id", id);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapReaderToMusic(reader);
        }

        return null;
    }

    public static List<Music> GetMusicByGenre(string genre, string? userName = null)
    {
        var sql = string.IsNullOrWhiteSpace(userName)
            ? "SELECT * FROM Music WHERE Genre = @Genre AND IsActive = 1 AND IsPrivate = 0 ORDER BY Likes DESC"
            : "SELECT * FROM Music WHERE Genre = @Genre AND IsActive = 1 AND (IsPrivate = 0 OR Owner = @Owner) ORDER BY Likes DESC";

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@Genre", genre);
        if (!string.IsNullOrWhiteSpace(userName))
        {
            command.Parameters.AddWithValue("@Owner", userName);
        }

        return ExecuteQueryAndMapResults(command);
    }

    public static List<Music> GetMusicByArtist(string artist, string? userName = null)
    {
        var sql = string.IsNullOrWhiteSpace(userName)
            ? "SELECT * FROM Music WHERE Artist = @Artist AND IsActive = 1 AND IsPrivate = 0 ORDER BY UploadDate DESC"
            : "SELECT * FROM Music WHERE Artist = @Artist AND IsActive = 1 AND (IsPrivate = 0 OR Owner = @Owner) ORDER BY UploadDate DESC";

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@Artist", artist);
        if (!string.IsNullOrWhiteSpace(userName))
        {
            command.Parameters.AddWithValue("@Owner", userName);
        }

        return ExecuteQueryAndMapResults(command);
    }

    public static List<Music> SearchMusic(string query, string? userName = null)
    {
        var sql = string.IsNullOrWhiteSpace(userName)
            ? @"SELECT * FROM Music WHERE IsActive = 1 AND IsPrivate = 0 AND (
                Title LIKE @Query OR 
                Description LIKE @Query OR 
                Artist LIKE @Query
            ) ORDER BY Likes DESC, UploadDate DESC LIMIT 100"
            : @"SELECT * FROM Music WHERE IsActive = 1 AND (IsPrivate = 0 OR Owner = @Owner) AND (
                Title LIKE @Query OR 
                Description LIKE @Query OR 
                Artist LIKE @Query
            ) ORDER BY Likes DESC, UploadDate DESC LIMIT 100";

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@Query", $"%{query}%");
        if (!string.IsNullOrWhiteSpace(userName))
        {
            command.Parameters.AddWithValue("@Owner", userName);
        }

        return ExecuteQueryAndMapResults(command);
    }

    public static List<Music> GetTopMusicByLikes(int limit = 20, string? userName = null)
    {
        var sql = string.IsNullOrWhiteSpace(userName)
            ? $"SELECT * FROM Music WHERE IsActive = 1 AND IsPrivate = 0 ORDER BY Likes DESC LIMIT {limit}"
            : $"SELECT * FROM Music WHERE IsActive = 1 AND (IsPrivate = 0 OR Owner = @Owner) ORDER BY Likes DESC LIMIT {limit}";

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        if (!string.IsNullOrWhiteSpace(userName))
        {
            command.Parameters.AddWithValue("@Owner", userName);
        }

        return ExecuteQueryAndMapResults(command);
    }

    public static List<Music> GetPopularMusic(int limit = 20, string? userName = null)
    {
        var sql = string.IsNullOrWhiteSpace(userName)
            ? $"SELECT * FROM Music WHERE IsActive = 1 AND IsPrivate = 0 ORDER BY Views DESC LIMIT {limit}"
            : $"SELECT * FROM Music WHERE IsActive = 1 AND (IsPrivate = 0 OR Owner = @Owner) ORDER BY Views DESC LIMIT {limit}";

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        if (!string.IsNullOrWhiteSpace(userName))
        {
            command.Parameters.AddWithValue("@Owner", userName);
        }

        return ExecuteQueryAndMapResults(command);
    }

    public static List<Music> GetRecentMusic(int limit = 20, string? userName = null)
    {
        var sql = string.IsNullOrWhiteSpace(userName)
            ? $"SELECT * FROM Music WHERE IsActive = 1 AND IsPrivate = 0 ORDER BY UploadDate DESC LIMIT {limit}"
            : $"SELECT * FROM Music WHERE IsActive = 1 AND (IsPrivate = 0 OR Owner = @Owner) ORDER BY UploadDate DESC LIMIT {limit}";

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        if (!string.IsNullOrWhiteSpace(userName))
        {
            command.Parameters.AddWithValue("@Owner", userName);
        }

        return ExecuteQueryAndMapResults(command);
    }



    public static List<Music> GetAllMusic(string? userName = null)
    {
        var sql = string.IsNullOrWhiteSpace(userName)
            ? "SELECT * FROM Music WHERE IsActive = 1 AND IsPrivate = 0"
            : "SELECT * FROM Music WHERE IsActive = 1 AND (IsPrivate = 0 OR Owner = @Owner)";

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        if (!string.IsNullOrWhiteSpace(userName))
        {
            command.Parameters.AddWithValue("@Owner", userName);
        }

        return ExecuteQueryAndMapResults(command);
    }

    public static List<string> GetAllGenres(string? userName = null)
    {
        var sql = string.IsNullOrWhiteSpace(userName)
            ? @"SELECT DISTINCT Genre FROM Music WHERE IsActive = 1 AND IsPrivate = 0 AND Genre IS NOT NULL ORDER BY Genre"
            : @"SELECT DISTINCT Genre FROM Music WHERE IsActive = 1 AND (IsPrivate = 0 OR Owner = @Owner) AND Genre IS NOT NULL ORDER BY Genre";

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        if (!string.IsNullOrWhiteSpace(userName))
        {
            command.Parameters.AddWithValue("@Owner", userName);
        }

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

    public static List<string> GetAllArtists(string? userName = null)
    {
        var sql = string.IsNullOrWhiteSpace(userName)
            ? @"SELECT DISTINCT Artist FROM Music WHERE IsActive = 1 AND IsPrivate = 0 AND Artist IS NOT NULL ORDER BY Artist"
            : @"SELECT DISTINCT Artist FROM Music WHERE IsActive = 1 AND (IsPrivate = 0 OR Owner = @Owner) AND Artist IS NOT NULL ORDER BY Artist";

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        if (!string.IsNullOrWhiteSpace(userName))
        {
            command.Parameters.AddWithValue("@Owner", userName);
        }

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

        using var command = connection.CreateCommand();
        command.CommandText = @"
        UPDATE Music 
        SET 
            Title = @Title,
            Description = @Description,
            Artist = @Artist,
            Genre = @Genre,
            DurationSeconds = @DurationSeconds
        WHERE Id = @Id";

        command.Parameters.AddWithValue("@Title", music.Title ?? string.Empty);
        command.Parameters.AddWithValue("@Description", music.Description ?? string.Empty);
        command.Parameters.AddWithValue("@Artist", music.Artist ?? "Unknown Artist");
        command.Parameters.AddWithValue("@Genre", music.Genre ?? "Other");
        command.Parameters.AddWithValue("@DurationSeconds", music.DurationSeconds);
        command.Parameters.AddWithValue("@Id", music.Id);

        return command.ExecuteNonQuery() > 0;
    }

    public static bool DeleteMusic(int id, string owner)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Music SET IsActive = 0 WHERE Id = @Id AND Owner = @Owner";
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@Owner", owner);

        return command.ExecuteNonQuery() > 0;
    }

    public static (int TotalMusic, int TotalLikes, long TotalViews) GetStatistics(string? userName = null)
    {
        var sql = string.IsNullOrWhiteSpace(userName)
            ? @"SELECT COUNT(*) as TotalMusic, COALESCE(SUM(Likes), 0) as TotalLikes, COALESCE(SUM(Views), 0) as TotalViews FROM Music WHERE IsActive = 1 AND IsPrivate = 0"
            : @"SELECT COUNT(*) as TotalMusic, COALESCE(SUM(Likes), 0) as TotalLikes, COALESCE(SUM(Views), 0) as TotalViews FROM Music WHERE IsActive = 1 AND (IsPrivate = 0 OR Owner = @Owner)";

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        if (!string.IsNullOrWhiteSpace(userName))
        {
            command.Parameters.AddWithValue("@Owner", userName);
        }

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return (
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetInt64(2)
            );
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
            Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
            Artist = reader.IsDBNull(3) ? "Unknown Artist" : reader.GetString(3),
            Genre = reader.IsDBNull(4) ? "Other" : reader.GetString(4),
            Owner = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
            IsPrivate = reader.IsDBNull(6) ? false : reader.GetInt32(6) == 1,
            AudioPath = reader.GetString(7),
            DurationSeconds = reader.GetInt32(8),
            Likes = reader.GetInt32(9),
            Views = reader.GetInt32(10),
            UploadDate = DateTime.Parse(reader.GetString(11)),
            IsActive = reader.GetInt32(12) == 1
        };
    }

    private static void UpdateIntField(int musicId, string fieldName, int increment)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = $@"
        UPDATE Music 
        SET {fieldName} = {fieldName} + @Increment 
        WHERE Id = @Id";

        command.Parameters.AddWithValue("@Increment", increment);
        command.Parameters.AddWithValue("@Id", musicId);

        command.ExecuteNonQuery();
    }

    private static void EnsureColumnExists(SqliteConnection connection, string tableName, string columnName, string columnDefinition)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info({tableName});";

        using var reader = command.ExecuteReader();
        var exists = false;
        while (reader.Read())
        {
            if (reader.GetString(1) == columnName)
            {
                exists = true;
                break;
            }
        }

        if (!exists)
        {
            using var alterCommand = connection.CreateCommand();
            alterCommand.CommandText = $"ALTER TABLE {tableName} ADD COLUMN {columnDefinition};";
            alterCommand.ExecuteNonQuery();
        }
    }
}
