using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using StackExchange.Redis;

namespace Server.IntegrationTests;

public class RedisFixture : IAsyncLifetime
{
    private ITestcontainersContainer? _container;
    private string? _cliContainerName;

    public string Connection { get; private set; } = "";

    public async Task InitializeAsync()
    {
        // 1) Tentative Testcontainers
        try
        {
            _container = new TestcontainersBuilder<TestcontainersContainer>()
                .WithImage("redis:7-alpine")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(6379))
                .WithCleanUp(true)
                .Build();

            await _container.StartAsync();
            var hostPort = _container.GetMappedPublicPort(6379);
            Connection = $"localhost:{hostPort}";

            using var muxer = await ConnectionMultiplexer.ConnectAsync(Connection);
            await muxer.GetDatabase().PingAsync();
        }
        catch (Exception)
        {
            // 2) Fallback CLI (no attach)
            _cliContainerName = "it_redis_tests";
            try { DockerCli.Run($"rm -f {_cliContainerName}"); } catch { /* ignore */ }

            DockerCli.Run(
                $"run -d --name {_cliContainerName} -p 0:6379 redis:7-alpine");

            var port = DockerCli.InspectHostPort(_cliContainerName, 6379);
            Connection = $"localhost:{port}";

            using var muxer = await ConnectionMultiplexer.ConnectAsync(Connection);
            await muxer.GetDatabase().PingAsync();
        }
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
