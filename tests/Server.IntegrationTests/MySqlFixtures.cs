using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using MySql.Data.MySqlClient;

namespace Server.IntegrationTests;

public class MySqlFixture : IAsyncLifetime
{
    private readonly MySqlTestcontainer _container;

    public string RootPassword { get; } = "root";
    public MySqlFixture()
    {
        _container = new TestcontainersBuilder<MySqlTestcontainer>()
            .WithImage("mysql:8.0")
            // on passe uniquement ROOT password et création de la base
            .WithEnvironment("MYSQL_ROOT_PASSWORD", RootPassword)
            .WithEnvironment("MYSQL_DATABASE", "TimeStockDB")
            // expose toujours le port 3306 à l'intérieur du conteneur
            .WithExposedPort(3306)
            // mappe le port hôte 3307 vers le port conteneur 3306
            .WithPortBinding(3307, 3306)

            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(3306))

            .WithCleanUp(true)
            .Build();
    }

    public string ConnectionString =>
        // on récupère le port mappé sur le host pour le port conteneur 3306
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
    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}
