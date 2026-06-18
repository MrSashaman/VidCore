using System.Security.Cryptography;
using Microsoft.Data.Sqlite;

namespace VidCore;

public static class UserDatabase
{
    private const string ConnectionString = "Data Source=accounts.db";

    public static void Initialize()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
        CREATE TABLE IF NOT EXISTS Users (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Username TEXT NOT NULL UNIQUE,
            PasswordHash TEXT NOT NULL,
            PasswordSalt TEXT NOT NULL,
            CreatedAt TEXT NOT NULL
        );";

        command.ExecuteNonQuery();
    }

    public static bool CreateUser(string username, string password)
    {
        if (GetUserByUsername(username) != null)
        {
            return false;
        }

        CreatePasswordHash(password, out var hash, out var salt);

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
        INSERT INTO Users (Username, PasswordHash, PasswordSalt, CreatedAt)
        VALUES (@Username, @PasswordHash, @PasswordSalt, @CreatedAt);";

        command.Parameters.AddWithValue("@Username", username);
        command.Parameters.AddWithValue("@PasswordHash", Convert.ToBase64String(hash));
        command.Parameters.AddWithValue("@PasswordSalt", Convert.ToBase64String(salt));
        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow.ToString("o"));

        return command.ExecuteNonQuery() > 0;
    }

    public static bool ValidateUser(string username, string password)
    {
        var user = GetUserByUsername(username);
        if (user == null)
        {
            return false;
        }

        var hash = Convert.FromBase64String(user.PasswordHash);
        var salt = Convert.FromBase64String(user.PasswordSalt);

        return VerifyPasswordHash(password, hash, salt);
    }

    public static User? GetUserByUsername(string username)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
        SELECT Id, Username, PasswordHash, PasswordSalt, CreatedAt
        FROM Users
        WHERE Username = @Username;";

        command.Parameters.AddWithValue("@Username", username);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                PasswordSalt = reader.GetString(3),
                CreatedAt = DateTime.Parse(reader.GetString(4))
            };
        }

        return null;
    }

    private static void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
    {
        salt = RandomNumberGenerator.GetBytes(16);
        using var deriveBytes = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
        hash = deriveBytes.GetBytes(32);
    }

    private static bool VerifyPasswordHash(string password, byte[] hash, byte[] salt)
    {
        using var deriveBytes = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
        var computedHash = deriveBytes.GetBytes(32);
        return CryptographicOperations.FixedTimeEquals(hash, computedHash);
    }
}
