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

        var createCommand = connection.CreateCommand();
        createCommand.CommandText = @"
        CREATE TABLE IF NOT EXISTS Comments (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            VideoId INTEGER NOT NULL,
            Author TEXT NOT NULL,
            Text TEXT NOT NULL,
            UploadDate TEXT NOT NULL,
            IsPrivate INTEGER DEFAULT 0
        );";
        createCommand.ExecuteNonQuery();

        EnsureColumnExists(connection, "Comments", "IsPrivate", "IsPrivate INTEGER DEFAULT 0");
    }

    public static void AddComment(Comment comment)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
        INSERT INTO Comments (VideoId, Author, Text, UploadDate, IsPrivate)
        VALUES (@VideoId, @Author, @Text, @UploadDate, @IsPrivate);";

        command.Parameters.AddWithValue("@VideoId", comment.VideoId);
        command.Parameters.AddWithValue("@Author", comment.Author ?? string.Empty);
        command.Parameters.AddWithValue("@Text", comment.Text ?? string.Empty);
        command.Parameters.AddWithValue("@UploadDate", comment.UploadDate.ToString("o"));
        command.Parameters.AddWithValue("@IsPrivate", comment.IsPrivate ? 1 : 0);

        command.ExecuteNonQuery();
    }

    public static List<Comment> GetCommentsByVideoId(int videoId, string? currentUser, bool includePrivate)
    {
        var comments = new List<Comment>();

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
        SELECT Id, VideoId, Author, Text, UploadDate, IsPrivate
        FROM Comments
        WHERE VideoId = @VideoId
          AND (IsPrivate = 0 OR Author = @CurrentUser OR @IncludePrivate = 1)
        ORDER BY UploadDate ASC;";

        command.Parameters.AddWithValue("@VideoId", videoId);
        command.Parameters.AddWithValue("@CurrentUser", currentUser ?? string.Empty);
        command.Parameters.AddWithValue("@IncludePrivate", includePrivate ? 1 : 0);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            comments.Add(new Comment
            {
                id = reader.GetInt32(0),
                VideoId = reader.GetInt32(1),
                Author = reader.GetString(2),
                Text = reader.GetString(3),
                UploadDate = DateTime.Parse(reader.GetString(4)),
                IsPrivate = reader.GetInt32(5) == 1
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
