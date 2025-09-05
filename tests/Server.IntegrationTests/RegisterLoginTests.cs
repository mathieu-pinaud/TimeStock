using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using TimeStock.Shared.Dtos;
using TimeStock.Server;

namespace Server.IntegrationTests;

[Collection("integration")]                                     // ← partage MySqlFixture
public class RegisterLoginTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly IServiceProvider _services;

    public RegisterLoginTests(MySqlFixture db, RedisFixture redis, WebApplicationFactory<Program> factory)
    {
        var webAppFactory = factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((ctx, cfg) =>
                {
                    var port = db.Port;        // port mappé dynamique
                    cfg.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] =
                            db.ConnectionString,
                        ["ConnectionStrings:TenantTemplate"] =
                            $"Server=localhost;Port={port};Uid={{0}};Pwd={{1}};Database={{2}};",

                        ["Jwt:Key"] = "TestKey-ForIntegration-0123456789",
                        ["Jwt:Issuer"] = "TimeStock",
                        ["Jwt:Audience"] = "TimeStockClient",
                        ["Jwt:ExpireMinutes"] = "60",
                        ["Redis:Configuration"] = redis.Connection,
                        ["Redis:InstanceName"] = "timestock:test:"
                    });
                });

                builder.ConfigureServices(services =>
                {
                    var existing = services.FirstOrDefault(d => d.ServiceType == typeof(IDistributedCache));
                    if (existing is not null) services.Remove(existing);

                    services.AddStackExchangeRedisCache(o =>
                    {
                        o.Configuration = redis.Connection;    // <-- clé : on force la vraie connexion
                        o.InstanceName = "timestock:test:";
                    });
                });
            });

        _client = webAppFactory.CreateClient();     // HttpClient adressé sur l’API in-memory
        _services = webAppFactory.Services;
    }

    [Fact]
    public async Task Register_Then_Login_Returns_Jwt()
    {
        // ▶ 2. Prépare le DTO d'inscription
        var register = new UserDto
        {
            AccountName = "Acme",
            Name = "Doe",
            FirstName = "John",
            Email = "john@acme.io",
            Password = "Pa$$w0rd!"
        };

        // ▶ 3. POST /api/auth/register
        var regRes = await _client.PostAsJsonAsync("/api/auth/register", register);
        regRes.StatusCode.Should().Be(HttpStatusCode.OK);

        // ▶ 4. POST /api/auth/login
        var login = new LoginDto { Email = register.Email, Password = register.Password };
        var logRes = await _client.PostAsJsonAsync("/api/auth/login", login);
        logRes.StatusCode.Should().Be(HttpStatusCode.OK);

        // ▶ 5. Vérifie le body
        var body = await logRes.Content.ReadFromJsonAsync<LoginResponseDto>();
        body!.Token.Should().NotBeNullOrEmpty();
        body.DatabaseName.Should().Be("db_Acme");
    }

    [Fact]
    public async Task Me_WarmsUpRedisCache()
    {
        // 1) Register
        var regRes = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            AccountName = "azerty",
            Name = "Doe",
            FirstName = "Jane",
            Email = "azerty@test.com",
            Password = "Passw0rd!"
        });
        regRes.EnsureSuccessStatusCode();

        // 2) Login
        var loginRes = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "azerty@test.com",
            Password = "Passw0rd!"
        });
        loginRes.EnsureSuccessStatusCode();

        var login = await loginRes.Content.ReadFromJsonAsync<LoginResponseDto>();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login!.Token);

        // 3) Call /me (warm cache)
        var meRes = await _client.GetAsync("/api/auth/me");
        meRes.EnsureSuccessStatusCode();

        // 4) Check Redis cache (clé LOGIQUE, sans InstanceName)
        using var scope = _services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
        var key = "tenant:azerty:creds"; // <- NE PAS inclure "timestock:test:"

        string? cachedJson = null;
        for (int i = 0; i < 10; i++) // ~5s max
        {
            cachedJson = await cache.GetStringAsync(key);
            if (!string.IsNullOrEmpty(cachedJson))
                break;
            await Task.Delay(500);
        }
        Assert.False(string.IsNullOrEmpty(cachedJson));
        Assert.Contains("Username", cachedJson); // JSON sérialisé des creds
    }
}
