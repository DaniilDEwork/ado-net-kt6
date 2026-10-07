using Kt6UpdateApi.Models;
using Microsoft.Data.Sqlite;

namespace Kt6UpdateApi.Data;

public class UserDatabase
{
    private readonly string _connectionString;
    private readonly string _schemaPath;

    public UserDatabase(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var connection = new SqliteConnectionStringBuilder(configuration.GetConnectionString("DefaultConnection"));
        connection.DataSource = Path.GetFullPath(connection.DataSource, environment.ContentRootPath);
        Directory.CreateDirectory(Path.GetDirectoryName(connection.DataSource)!);
        _connectionString = connection.ConnectionString;
        _schemaPath = Path.Combine(environment.ContentRootPath, "Data", "schema.sql");
    }

    public async Task InitializeAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = await File.ReadAllTextAsync(_schemaPath);
        await command.ExecuteNonQueryAsync();
        command.CommandText = "SELECT COUNT(*) FROM Users";
        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        if (count == 0)
        {
            command.CommandText = """
                INSERT INTO Users (Id, Name, Email, Age) VALUES
                    (1, 'Даниил', 'daniil@example.com', 19),
                    (2, 'Анна', 'anna@example.com', 22),
                    (3, 'Иван', 'ivan@example.com', 20);
                """;
            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task<List<User>> GetAllAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Email, Age FROM Users ORDER BY Id";
        using var reader = await command.ExecuteReaderAsync();
        var users = new List<User>();
        while (await reader.ReadAsync())
        {
            users.Add(ReadUser(reader));
        }
        return users;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        return await FindUserAsync(connection, id);
    }

    public async Task<User?> UpdateAsync(int id, UpdateUserRequest request)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Users SET Name = @Name, Email = @Email, Age = @Age WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@Name", request.Name.Trim());
        command.Parameters.AddWithValue("@Email", request.Email.Trim());
        command.Parameters.AddWithValue("@Age", request.Age.GetValueOrDefault());
        var updated = await command.ExecuteNonQueryAsync();
        return updated == 0 ? null : await FindUserAsync(connection, id);
    }

    public async Task<User?> UpdateEmailAsync(int id, UpdateEmailRequest request)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Users SET Email = @Email WHERE Id = @Id AND Name = @Name";
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@Name", request.Name);
        command.Parameters.AddWithValue("@Email", request.Email.Trim());
        var updated = await command.ExecuteNonQueryAsync();
        return updated == 0 ? null : await FindUserAsync(connection, id);
    }

    public async Task<User?> UpdateWithTransactionAsync(int id, UpdateUserRequest request)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();
        try
        {
            using var details = connection.CreateCommand();
            details.Transaction = transaction;
            details.CommandText = "UPDATE Users SET Name = @Name, Age = @Age WHERE Id = @Id";
            details.Parameters.AddWithValue("@Id", id);
            details.Parameters.AddWithValue("@Name", request.Name.Trim());
            details.Parameters.AddWithValue("@Age", request.Age.GetValueOrDefault());
            if (await details.ExecuteNonQueryAsync() == 0)
            {
                transaction.Rollback();
                return null;
            }

            using var email = connection.CreateCommand();
            email.Transaction = transaction;
            email.CommandText = "UPDATE Users SET Email = @Email WHERE Id = @Id";
            email.Parameters.AddWithValue("@Id", id);
            email.Parameters.AddWithValue("@Email", request.Email.Trim());
            await email.ExecuteNonQueryAsync();
            var user = await FindUserAsync(connection, id, transaction);
            transaction.Commit();
            return user;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static async Task<User?> FindUserAsync(SqliteConnection connection, int id, SqliteTransaction? transaction = null)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT Id, Name, Email, Age FROM Users WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id);
        using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ReadUser(reader) : null;
    }

    private static User ReadUser(SqliteDataReader reader)
    {
        return new User
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Email = reader.GetString(2),
            Age = reader.GetInt32(3)
        };
    }
}
