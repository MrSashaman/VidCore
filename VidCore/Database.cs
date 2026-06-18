using Microsoft.Data.Sqlite;

namespace VidCore;

public static class Database
{
    private const string ConnectionString = "Data Source=vidcore.db";

    public static void Initialize()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
        CREATE TABLE IF NOT EXISTS Videos (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Title TEXT NOT NULL,
            Description TEXT,
            Owner TEXT NOT NULL DEFAULT '',
            IsPrivate INTEGER DEFAULT 0,
            VideoPath TEXT NOT NULL,
            ThumbnailPath TEXT NOT NULL,
            UploadDate TEXT NOT NULL,
            Views INTEGER DEFAULT 0,
            Likes INTEGER DEFAULT 0
        );";
        command.ExecuteNonQuery();

        EnsureColumnExists(connection, "Videos", "Owner", "Owner TEXT NOT NULL DEFAULT ''");
        EnsureColumnExists(connection, "Videos", "IsPrivate", "IsPrivate INTEGER DEFAULT 0");
    }

    public static void AddVideo(Video video)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
        INSERT INTO Videos
        (
            Title,
            Description,
            Owner,
            IsPrivate,
            VideoPath,
            ThumbnailPath,
            UploadDate,
            Views,
            Likes
        )
        VALUES
        (
            @Title,
            @Description,
            @Owner,
            @IsPrivate,
            @VideoPath,
            @ThumbnailPath,
            @UploadDate,
            @Views,
            @Likes
        );";

        command.Parameters.AddWithValue("@Title", video.Title);
        command.Parameters.AddWithValue("@Description", video.Description);
        command.Parameters.AddWithValue("@Owner", video.Owner);
        command.Parameters.AddWithValue("@IsPrivate", video.IsPrivate ? 1 : 0);
        command.Parameters.AddWithValue("@VideoPath", video.VideoPath);
        command.Parameters.AddWithValue("@ThumbnailPath", video.ThumbnailPath);
        command.Parameters.AddWithValue("@UploadDate", video.UploadDate.ToString("o"));
        command.Parameters.AddWithValue("@Views", video.Views);
        command.Parameters.AddWithValue("@Likes", video.Likes);

        command.ExecuteNonQuery();
    }

    public static List<Video> GetAccessibleVideos(string? userName)
    {
        var videos = new List<Video>();

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        if (string.IsNullOrWhiteSpace(userName))
        {
            command.CommandText = "SELECT * FROM Videos WHERE IsPrivate = 0 ORDER BY UploadDate DESC";
        }
        else
        {
            command.CommandText = @"SELECT * FROM Videos WHERE IsPrivate = 0 OR Owner = @Owner ORDER BY UploadDate DESC";
            command.Parameters.AddWithValue("@Owner", userName);
        }

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            videos.Add(MapReaderToVideo(reader));
        }

        return videos;
    }

    public static Video? GetVideoById(int id)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Videos WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapReaderToVideo(reader);
        }

        return null;
    }

    public static void DeleteVideo(int id, string owner)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Videos WHERE Id = @Id AND Owner = @Owner";
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@Owner", owner);

        command.ExecuteNonQuery();
    }

    public static void AddView(int id)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
        UPDATE Videos
        SET Views = Views + 1
        WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id);

        command.ExecuteNonQuery();
    }

    public static void AddLike(int id)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
        UPDATE Videos
        SET Likes = Likes + 1
        WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id);

        command.ExecuteNonQuery();
    }

    public static void UpdateVideo(int id, string title, string description, string owner)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
        UPDATE Videos
        SET Title = @Title,
            Description = @Description
        WHERE Id = @Id AND Owner = @Owner";

        command.Parameters.AddWithValue("@Title", title);
        command.Parameters.AddWithValue("@Description", description);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@Owner", owner);

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

    private static Video MapReaderToVideo(SqliteDataReader reader)
    {
        return new Video
        {
            Id = reader.GetInt32(0),
            Title = reader.GetString(1),
            Description = reader.GetString(2),
            Owner = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
            IsPrivate = reader.IsDBNull(4) ? false : reader.GetInt32(4) == 1,
            VideoPath = reader.GetString(5),
            ThumbnailPath = reader.GetString(6),
            UploadDate = DateTime.Parse(reader.GetString(7)),
            Views = reader.GetInt32(8),
            Likes = reader.GetInt32(9)
        };
    }
}
