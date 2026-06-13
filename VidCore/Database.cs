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
            VideoPath TEXT NOT NULL,
            ThumbnailPath TEXT NOT NULL,
            UploadDate TEXT NOT NULL,
            Views INTEGER DEFAULT 0,
            Likes INTEGER DEFAULT 0
        );";

        command.ExecuteNonQuery();
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
        @VideoPath,
        @ThumbnailPath,
        @UploadDate,
        @Views,
        @Likes
    );";

    command.Parameters.AddWithValue("@Title", video.Title);
    command.Parameters.AddWithValue("@Description", video.Description);
    command.Parameters.AddWithValue("@VideoPath", video.VideoPath);
    command.Parameters.AddWithValue("@ThumbnailPath", video.ThumbnailPath);
    command.Parameters.AddWithValue("@UploadDate", video.UploadDate.ToString("o"));
    command.Parameters.AddWithValue("@Views", video.Views);
    command.Parameters.AddWithValue("@Likes", video.Likes);

    command.ExecuteNonQuery();
}


public static List<Video> GetVideos()
{
    var videos = new List<Video>();

    using var connection = new SqliteConnection(ConnectionString);
    connection.Open();

    var command = connection.CreateCommand();
    command.CommandText = "SELECT * FROM Videos";

    using var reader = command.ExecuteReader();

    while (reader.Read())
    {
        videos.Add(new Video
        {
            Id = reader.GetInt32(0),
            Title = reader.GetString(1),
            Description = reader.GetString(2),
            VideoPath = reader.GetString(3),
            ThumbnailPath = reader.GetString(4),
            UploadDate = DateTime.Parse(reader.GetString(5)),
            Views = reader.GetInt32(6),
            Likes = reader.GetInt32(7)
        });
    }

    return videos;
}


public static Video? GetVideoById(int id)
{
    using var connection = new SqliteConnection(ConnectionString);
    connection.Open();

    var command = connection.CreateCommand();

    command.CommandText =
        "SELECT * FROM Videos WHERE Id = @Id";

    command.Parameters.AddWithValue("@Id", id);

    using var reader = command.ExecuteReader();

    if (reader.Read())
    {
        return new Video
        {
            Id = reader.GetInt32(0),
            Title = reader.GetString(1),
            Description = reader.GetString(2),
            VideoPath = reader.GetString(3),
            ThumbnailPath = reader.GetString(4),
            UploadDate = DateTime.Parse(reader.GetString(5)),
            Views = reader.GetInt32(6),
            Likes = reader.GetInt32(7)
        };
    }

    return null;
}


public static void DeleteVideo(int id)
{
    using var connection = new SqliteConnection(ConnectionString);
    connection.Open();

    var command = connection.CreateCommand();

    command.CommandText =
        "DELETE FROM Videos WHERE Id = @Id";

    command.Parameters.AddWithValue("@Id", id);

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


public static void UpdateVideo(int id, string title, string description)
{
    using var connection = new SqliteConnection(ConnectionString);
    connection.Open();

    var command = connection.CreateCommand();

    command.CommandText = @"
        UPDATE Videos
        SET Title = @Title,
            Description = @Description
        WHERE Id = @Id";

    command.Parameters.AddWithValue("@Title", title);
    command.Parameters.AddWithValue("@Description", description);
    command.Parameters.AddWithValue("@Id", id);

    command.ExecuteNonQuery();
}


}