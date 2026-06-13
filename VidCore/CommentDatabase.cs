using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace VidCore;

public static class CommentDatabase
{
    private const string ConnectionString = "Data Source=comments.db";

    public static void Initialize()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
        CREATE TABLE IF NOT EXISTS Comments (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            VideoId INTEGER NOT NULL,
            Author TEXT NOT NULL,
            Text TEXT NOT NULL,
            UploadDate TEXT NOT NULL
        );";

        command.ExecuteNonQuery();
    }

    public static void AddComment(Comment comment)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
        INSERT INTO Comments (VideoId, Author, Text, UploadDate)
        VALUES (@VideoId, @Author, @Text, @UploadDate);";

        command.Parameters.AddWithValue("@VideoId", comment.VideoId);
        command.Parameters.AddWithValue("@Author", comment.Author ?? string.Empty);
        command.Parameters.AddWithValue("@Text", comment.Text ?? string.Empty);
        command.Parameters.AddWithValue("@UploadDate", comment.UploadDate.ToString("o"));

        command.ExecuteNonQuery();
    }

    public static List<Comment> GetCommentsByVideoId(int videoId)
    {
        var comments = new List<Comment>();

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
        SELECT Id, VideoId, Author, Text, UploadDate
        FROM Comments
        WHERE VideoId = @VideoId
        ORDER BY UploadDate ASC;";

        command.Parameters.AddWithValue("@VideoId", videoId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            comments.Add(new Comment
            {
                id = reader.GetInt32(0),
                VideoId = reader.GetInt32(1),
                Author = reader.GetString(2),
                Text = reader.GetString(3),
                UploadDate = DateTime.Parse(reader.GetString(4))
            });
        }

        return comments;
    }

    public static void DeleteCommentsByVideoId(int videoId)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
        DELETE FROM Comments
        WHERE VideoId = @VideoId;";

        command.Parameters.AddWithValue("@VideoId", videoId);
        command.ExecuteNonQuery();
    }
}
