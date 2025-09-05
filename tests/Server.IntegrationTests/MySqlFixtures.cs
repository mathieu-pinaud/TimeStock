using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Configurations;
using MySql.Data.MySqlClient;

namespace Server.IntegrationTests;

public class MySqlFixture : IAsyncLifetime
{
    private MySqlTestcontainer? _container;
    private string? _cliContainerName;
    private int _port;

    public string RootPassword { get; } = "root";

    public int Port => _port;

    public string ConnectionString =>
        $"Server=localhost;Port={Port};Uid=root;Pwd={RootPassword};Database=TimeStockDB;";

    public async Task InitializeAsync()
    {
        // 1) Tentative Testcontainers (parfait en CI et souvent OK local)
        try
        {
            _container = new TestcontainersBuilder<MySqlTestcontainer>()
                .WithDatabase(new MySqlTestcontainerConfiguration
                {
                    Database = "TimeStockDB",
                    Username = "root",
                    Password = RootPassword
                })
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(3306))
                .WithCleanUp(true)
                .Build();

            await _container.StartAsync();
            _port = _container.GetMappedPublicPort(3306);
        }
        catch (Exception)
        {
            // 2) Fallback automatique: docker CLI (aucun attach → pas d’hijack)
            // Nettoyage éventuel d’un ancien conteneur
            _cliContainerName = "it_mysql_tests";
            try { DockerCli.Run($"rm -f {_cliContainerName}"); } catch { /* ignore */ }

            DockerCli.Run(
              $"run -d --name {_cliContainerName} -p 0:3306 " +
              $"-e MYSQL_ROOT_PASSWORD={RootPassword} -e MYSQL_DATABASE=TimeStockDB mysql:8.0");

            _port = DockerCli.InspectHostPort(_cliContainerName, 3306);
        }

        // 3) Schéma
        await EnsureSchemaAsync();
    }

    private async Task EnsureSchemaAsync()
    {
        await using var conn = new MySqlConnection(ConnectionString);
        // retry simple au cas où MySQL démarre tout juste
        for (var i = 0; i < 30; i++)
        {
            try
            {
                await conn.OpenAsync();
                break;
            }
            catch
            {
                await Task.Delay(500);
            }
        }

        const string schema = """
            CREATE TABLE IF NOT EXISTS Users (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                AccountName      VARCHAR(255) NOT NULL UNIQUE,
                Name             VARCHAR(255) NOT NULL,
                FirstName       VARCHAR(255) NOT NULL,
                Email            VARCHAR(255) NOT NULL UNIQUE,
                PasswordHash     VARCHAR(255) NOT NULL,
                DatabaseName     VARCHAR(255) NOT NULL,
                DatabasePassword VARCHAR(255) NOT NULL
            );
        """;

        await using var cmd = new MySqlCommand(schema, conn);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
        else if (!string.IsNullOrEmpty(_cliContainerName))
        {
            try { DockerCli.Run($"rm -f {_cliContainerName}"); } catch { /* ignore */ }
        }
    }
}
