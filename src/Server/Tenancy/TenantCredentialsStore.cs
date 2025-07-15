
using Microsoft.Extensions.Caching.Memory;
using MySql.Data.MySqlClient;

public sealed class TenantCredentialsStore : ITenantCredentialsStore
{
    private readonly string      _masterCs;
    private readonly IMemoryCache _cache;

    public TenantCredentialsStore(IConfiguration cfg, IMemoryCache cache)
    {
        _masterCs = cfg.GetConnectionString("DefaultConnection")!;
        _cache    = cache;
    }

    public async ValueTask<(string,string)> GetAsync(string account, CancellationToken ct = default)
    {
        if (_cache.TryGetValue(account, out (string,string) creds))
            return creds;

        using var conn = new MySqlConnection(_masterCs);
        await conn.OpenAsync(ct);

        const string sql = """
            SELECT AccountName, DatabasePassword
            FROM Users WHERE AccountName = @acc
            """;
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@acc", account);

        using var rdr = await cmd.ExecuteReaderAsync(ct);
        if (!await rdr.ReadAsync(ct))
            throw new InvalidOperationException($"Tenant '{account}' introuvable.");

        creds = (rdr.GetString(0), rdr.GetString(1));
        _cache.Set(account, creds, TimeSpan.FromMinutes(15));   // cache 15 min
        return creds;
    }
}
