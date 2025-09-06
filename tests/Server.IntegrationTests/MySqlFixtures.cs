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

    private string BuildConnectionString(bool includeDatabase = true)
    {
        var csb = new MySqlConnectionStringBuilder
        {
            Server = "localhost",
            Port = (uint)Port,
            UserID = "root",
            Password = RootPassword,
            SslMode = MySqlSslMode.Disabled,       // évite la négo SSL
            ConnectionTimeout = 15,                // s
            DefaultCommandTimeout = 15,            // s
            AllowPublicKeyRetrieval = true,
        };
        if (includeDatabase)
            csb.Database = "TimeStockDB";
        return csb.ConnectionString;
    }

    public string ConnectionString => BuildConnectionString(includeDatabase: true);

    public async Task InitializeAsync()
    {
        // 1) Démarrage via Testcontainers (fallback CLI si nécessaire)
        try
        {
            _container = new TestcontainersBuilder<MySqlTestcontainer>()
                .WithDatabase(new MySqlTestcontainerConfiguration
                {
                    Database = "TimeStockDB",
                    Username = "root",
                    Password = RootPassword
                })
                // Port ouvert ≠ serveur prêt : on ajoute notre propre attente plus bas
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(3306))
                .WithCleanUp(true)
                .Build();

            await _container.StartAsync();
            _port = _container.GetMappedPublicPort(3306);
        }
        catch
        {
            _cliContainerName = "it_mysql_tests";
            try { DockerCli.Run($"rm -f {_cliContainerName}"); } catch { /* ignore */ }

            DockerCli.Run(
                $"run -d --name {_cliContainerName} -p 0:3306 " +
                $"-e MYSQL_ROOT_PASSWORD={RootPassword} -e MYSQL_DATABASE=TimeStockDB mysql:8.0");

            _port = DockerCli.InspectHostPort(_cliContainerName, 3306);
        }

        // 2) Attendre la « vraie » readiness (SELECT 1)
        await WaitForMySqlAsync();

        // 3) Créer/valider le schéma
        await EnsureSchemaAsync();
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

    private async Task WaitForMySqlAsync(int attempts = 80, int delayMs = 500)
    {
        Exception? last = null;

        for (var i = 0; i < attempts; i++)
        {
            try
            {
                // on se connecte sans DB d’abord (certains init scripts créent la DB à la volée)
                await using var conn = new MySqlConnection(BuildConnectionString(includeDatabase: false));
                await conn.OpenAsync();

                // puis on vérifie que la DB cible est accessible
                await using (var cmdDb = new MySqlCommand("CREATE DATABASE IF NOT EXISTS TimeStockDB;", conn))
                    await cmdDb.ExecuteNonQueryAsync();

                await using (var cmd = new MySqlCommand("SELECT 1;", conn))
                    await cmd.ExecuteScalarAsync();

                return; // prêt
            }
            catch (Exception ex)
            {
                last = ex;
                await Task.Delay(delayMs);
            }
        }

        throw new InvalidOperationException(
            $"MySQL not ready with connection string '{ConnectionString}'", last);
    }

    private async Task EnsureSchemaAsync()
    {
        await using var conn = new MySqlConnection(ConnectionString);
        await conn.OpenAsync();

        const string schema = """
            CREATE TABLE IF NOT EXISTS Users (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                AccountName      VARCHAR(255) NOT NULL UNIQUE,
                Name             VARCHAR(255) NOT NULL,
                FirstName        VARCHAR(255) NOT NULL,
                Email            VARCHAR(255) NOT NULL UNIQUE,
                PasswordHash     VARCHAR(255) NOT NULL,
                DatabaseName     VARCHAR(255) NOT NULL,
                DatabasePassword VARCHAR(255) NOT NULL
            );
        """;

        await using var cmd = new MySqlCommand(schema, conn);
        await cmd.ExecuteNonQueryAsync();
    }
}
