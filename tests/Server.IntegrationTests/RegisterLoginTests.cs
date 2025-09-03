using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using TimeStock.Shared.Dtos;
using TimeStock.Server;

namespace Server.IntegrationTests;

[Collection("integration")]                                     // ← partage MySqlFixture
public class RegisterLoginTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public RegisterLoginTests(MySqlFixture db, WebApplicationFactory<Program> factory)
    {
        // ⚙️ 1. Override de configuration pour pointer l’API vers le MySQL du conteneur
        _client = factory
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

                        ["Jwt:Key"]           = "TestKey-ForIntegration-0123456789", 
                        ["Jwt:Issuer"]        = "TimeStock",
                        ["Jwt:Audience"]      = "TimeStockClient",
                        ["Jwt:ExpireMinutes"] = "60"
                    });
                });
            })
            .CreateClient();     // HttpClient adressé sur l’API in-memory
    }

    [Fact]
    public async Task Register_Then_Login_Returns_Jwt()
    {
        // ▶ 2. Prépare le DTO d'inscription
        var register = new UserDto
        {
            AccountName = "Acme",
            Name        = "Doe",
            FirstName   = "John",
            Email       = "john@acme.io",
            Password    = "Pa$$w0rd!"
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
}
