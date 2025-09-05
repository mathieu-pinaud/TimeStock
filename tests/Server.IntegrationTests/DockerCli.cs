using System.Diagnostics;

namespace Server.IntegrationTests;

internal static class DockerCli
{
    public static void Run(string args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "docker",
            ArgumentList = { },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        foreach (var part in SplitArgs(args)) psi.ArgumentList.Add(part);

        using var p = Process.Start(psi)!;
        p.WaitForExit();

        if (p.ExitCode != 0)
        {
            var err = p.StandardError.ReadToEnd();
            throw new InvalidOperationException($"docker {args} failed: {err}");
        }
    }

    public static string RunAndRead(string args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "docker",
            ArgumentList = { },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        foreach (var part in SplitArgs(args)) psi.ArgumentList.Add(part);

        using var p = Process.Start(psi)!;
        var output = p.StandardOutput.ReadToEnd();
        var err = p.StandardError.ReadToEnd();
        p.WaitForExit();

        if (p.ExitCode != 0)
        {
            throw new InvalidOperationException($"docker {args} failed: {err}");
        }

        return output.Trim();
    }

    // Récupère le port hôte mappé sur un port conteneur donné (ex: 3306/tcp)
    public static int InspectHostPort(string containerName, int containerPort)
    {
        // Exemple de sorties possibles :
        //  "0.0.0.0:49153"
        //  "0.0.0.0:49153\n:::49153"
        //  ":::49153"
        var output = RunAndRead($"port {containerName} {containerPort}/tcp");

        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var idx = line.LastIndexOf(':');
            if (idx >= 0 && int.TryParse(line[(idx + 1)..].Trim(), out var port))
                return port;
        }

        throw new InvalidOperationException(
            $"Cannot parse mapped host port from docker output: {output}");
    }


    private static IEnumerable<string> SplitArgs(string cmdLine)
    {
        // split simple, suffisant pour nos commandes docker
        bool inQuote = false;
        var current = new List<char>();
        foreach (var ch in cmdLine)
        {
            if (ch == '"') { inQuote = !inQuote; continue; }
            if (char.IsWhiteSpace(ch) && !inQuote)
            {
                if (current.Count > 0) { yield return new string(current.ToArray()); current.Clear(); }
            }
            else current.Add(ch);
        }
        if (current.Count > 0) yield return new string(current.ToArray());
    }
}
