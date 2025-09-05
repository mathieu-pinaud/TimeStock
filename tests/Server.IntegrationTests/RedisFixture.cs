using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Server.IntegrationTests;
public class RedisFixture : IAsyncLifetime
{
    public ITestcontainersContainer? Container { get; private set; }
    public string Connection { get; private set; } = "";

    public async Task InitializeAsync()
    {
        Container = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("redis:7-alpine")
            .WithCleanUp(true)
            .WithPortBinding(0, 6379) // map sur un port libre
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(6379))
            .Build();

        await Container.StartAsync();

        var hostPort = Container.GetMappedPublicPort(6379);
        Connection = $"localhost:{hostPort}";
    }

    public Task DisposeAsync()
    {
        return Container != null
            ? Container.DisposeAsync().AsTask()
            : Task.CompletedTask;
    }
}
