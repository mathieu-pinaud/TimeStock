
using Microsoft.Extensions.Caching.Memory;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Dapper;

public sealed class TenantCredentialsStore : ITenantCredentialsStore
{
    private readonly string _masterCs;
    private readonly IDistributedCache _cache;

    public TenantCredentialsStore(IConfiguration cfg, IDistributedCache cache)
    {
        _masterCs = cfg.GetConnectionString("DefaultConnection")!;
        _cache = cache;
    }

    public async ValueTask<(string Username, string Password)> GetAsync(string account, CancellationToken ct = default)
    {
        var key = $"tenant:{account}";
        var cached = await _cache.GetStringAsync(key, ct);

        if (cached is not null)
            return JsonSerializer.Deserialize<(string, string)>(cached)!;

        await using var conn = new MySqlConnection(_masterCs);
        await conn.OpenAsync(ct);

        const string sql = "SELECT AccountName, DatabasePassword FROM Users WHERE AccountName = @acc;";
        var creds = await conn.QuerySingleOrDefaultAsync<(string, string)>(sql, new { acc = account });

        var json = JsonSerializer.Serialize(creds);
        await _cache.SetStringAsync(key, json, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
        }, ct);

        return creds;
    }
}
