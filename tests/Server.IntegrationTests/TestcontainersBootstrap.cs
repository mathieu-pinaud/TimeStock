using System.Runtime.CompilerServices;
using DotNet.Testcontainers.Configurations;

namespace Server.IntegrationTests;

internal static class TestcontainersBoostrap
{
    // S'exécute une fois au chargement de l'assembly de tests.
    [ModuleInitializer]
    public static void Init()
    {
        // 1) Forcer l’endpoint Docker (utile en WSL & CI).
        //    Pas d’API centrale en 1.6.0, on pose juste DOCKER_HOST si absent.
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOCKER_HOST")))
        {
            Environment.SetEnvironmentVariable("DOCKER_HOST", "unix:///var/run/docker.sock");
        }

        // 2) Neutraliser d’éventuelles variables proxy qui cassent l’hijack.
        foreach (var k in new[] {
            "HTTP_PROXY","HTTPS_PROXY","ALL_PROXY","NO_PROXY",
            "http_proxy","https_proxy","all_proxy","no_proxy"
        })
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(k)))
                Environment.SetEnvironmentVariable(k, null);
        }

        // 3) Désactiver Ryuk partout (local ET CI) pour éliminer la cause de l'hijack.
        //    Le nettoyage sera assuré par DisposeAsync() dans tes fixtures.
        TestcontainersSettings.ResourceReaperEnabled = false;

        // (Optionnel) Si tu veux tracer plus tard :
        // TestcontainersSettings.Logger = new TestcontainersConsoleLogger();
    }
}
