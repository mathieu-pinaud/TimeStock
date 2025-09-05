using System.Text.Json;
using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

public sealed class TenantCredentialsStore : ITenantCredentialsStore
{
    private readonly IDistributedCache _cache;
    private readonly string _masterCs;
    private readonly ILogger<TenantCredentialsStore>? _logger;

    private static readonly DistributedCacheEntryOptions Entry15Min =
        new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) };

    public TenantCredentialsStore(IDistributedCache cache, IConfiguration cfg, ILogger<TenantCredentialsStore>? logger = null)
    {
        _cache = cache;
        _logger = logger;
        _masterCs = cfg.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("Missing DefaultConnection");
    }

    private static string Key(string account) => $"tenant:{account}:creds";

    public async ValueTask<TenantSqlCredentials> GetAsync(string account, CancellationToken ct = default)
    {
        var key = Key(account);

        // 1) Try cache (best-effort)
        try
        {
            var cached = await _cache.GetStringAsync(key, ct);
            if (!string.IsNullOrEmpty(cached))
                return JsonSerializer.Deserialize<TenantSqlCredentials>(cached)!;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Redis GetStringAsync failed for key {Key}", key);
            // continue: we’ll fallback to MySQL
        }

        // 2) DB fallback (source of truth)
        await using var conn = new MySqlConnection(_masterCs);
        await conn.OpenAsync(ct);

        const string sql = @"
            SELECT
                AccountName       AS Username,
                DatabasePassword  AS Password
            FROM Users
            WHERE AccountName = @acc
            LIMIT 1;
        ";

        var creds = await conn.QuerySingleOrDefaultAsync<TenantSqlCredentials>(sql, new { acc = account });
        if (creds is null || string.IsNullOrEmpty(creds.Username))
            throw new InvalidOperationException($"No SQL credentials found for tenant '{account}'.");

        // 3) Warm cache (best-effort) — never fail the request on cache write
        try
        {
            var json = JsonSerializer.Serialize(creds);
            await _cache.SetStringAsync(key, json, Entry15Min, ct);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Redis SetStringAsync failed for key {Key}", key);
        }

        return creds;
    }
}
