using MySql.Data.MySqlClient;
using TimeStock.Shared.Models;
using Dapper;

public class DatabaseService
{
    private readonly string _masterConnectionString;

    public DatabaseService(IConfiguration config)
    {
        _masterConnectionString = config.GetConnectionString("DefaultConnection")!;
    }

    public async Task<bool> UserExistsAsync(string email, string accountName)
    {
        using var connection = new MySqlConnection(_masterConnectionString);
        await connection.OpenAsync();

        string query = "SELECT COUNT(*) FROM Users WHERE Email = @Email OR AccountName = @AccountName;";
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@Email", email);
        command.Parameters.AddWithValue("@AccountName", accountName);

        return Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
    }

    public async Task SaveUserAsync(User user)
    {
        await using var connection = new MySqlConnection(_masterConnectionString);
        await connection.OpenAsync();

        const string insertSql = @"
        INSERT INTO Users
              (AccountName, Name, FirstName, Email, PasswordHash, DatabaseName, DatabasePassword)
        VALUES (@AccountName, @Name, @FirstName, @Email, @PasswordHash, @DatabaseName, @DatabasePassword);";

        await connection.ExecuteAsync(insertSql, user);

        var createDbSql = $"CREATE DATABASE `{user.DatabaseName}`;";
        var createUserSql = $"CREATE USER `{user.AccountName}`@'%' IDENTIFIED BY '{user.DatabasePassword}';";
        var grantSql = $"GRANT ALL PRIVILEGES ON `{user.DatabaseName}`.* TO `{user.AccountName}`@'%';";

        await connection.ExecuteAsync(createDbSql);
        await connection.ExecuteAsync(createUserSql);
        await connection.ExecuteAsync(grantSql);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        using var connection = new MySqlConnection(_masterConnectionString);
        await connection.OpenAsync();

        string query = "SELECT Id, AccountName, Name, FirstName, Email, PasswordHash, DatabaseName, DatabasePassword FROM Users WHERE Email = @Email";
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@Email", email);

        using var reader = await command.ExecuteReaderAsync();
        if (!reader.Read()) return null;

        return new User
        {
            Id = reader.GetInt32(0),
            AccountName = reader.GetString(1),
            Name = reader.GetString(2),
            FirstName = reader.GetString(3),
            Email = reader.GetString(4),
            PasswordHash = reader.GetString(5),
            DatabaseName = reader.GetString(6),
            DatabasePassword = reader.GetString(7)
        };
    }
}
