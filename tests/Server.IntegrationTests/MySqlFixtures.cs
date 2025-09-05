using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Configurations;
using MySql.Data.MySqlClient;

namespace Server.IntegrationTests;

public class MySqlFixture : IAsyncLifetime
{
    private readonly MySqlTestcontainer _container;

    public string RootPassword { get; } = "root";

    public MySqlFixture()
    {
        _container = new TestcontainersBuilder<MySqlTestcontainer>()
            .WithDatabase(new MySqlTestcontainerConfiguration
            {
                Database = "TimeStockDB",
                Username = "root",
                Password = RootPassword
            })
            .WithCleanUp(true)
            .Build();
    }

    public string ConnectionString =>
        $"Server=localhost;Port={_container.GetMappedPublicPort(3306)};" +
        $"Uid=root;Pwd={RootPassword};Database=TimeStockDB;";

    public int Port => _container.GetMappedPublicPort(3306);

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var conn = new MySqlConnection(ConnectionString);
        await conn.OpenAsync();

        const string schema = """
            CREATE TABLE IF NOT EXISTS Users (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                AccountName VARCHAR(255) NOT NULL UNIQUE,
                Name VARCHAR(255) NOT NULL,
                FirstName VARCHAR(255) NOT NULL,
                Email VARCHAR(255) NOT NULL UNIQUE,
                PasswordHash VARCHAR(255) NOT NULL,
                DatabaseName VARCHAR(255) NOT NULL,
                DatabasePassword VARCHAR(255) NOT NULL
            );
            """;

        await using var cmd = new MySqlCommand(schema, conn);
        await cmd.ExecuteNonQueryAsync();
    }

    public Task DisposeAsync()
    {
        return _container != null
            ? _container.DisposeAsync().AsTask()
            : Task.CompletedTask;
    }
}
