// TenantConnectionFactory.cs
using MySql.Data.MySqlClient;
using System.Data;

public sealed class TenantConnectionFactory : ITenantConnectionFactory
{
    private readonly IConfiguration _cfg;
    private readonly ITenantContext _tenant;
    private readonly ITenantCredentialsStore _store;

    private MySqlConnection? _cached;   // 1 connexion par requête (scope)

    public TenantConnectionFactory(IConfiguration cfg, ITenantContext tenant, ITenantCredentialsStore store)
    {
        _cfg = cfg;
        _tenant = tenant;
        _store = store;
    }

    public async ValueTask<MySqlConnection> GetOpenConnectionAsync(CancellationToken ct = default)
    {
        if (_cached is { State: ConnectionState.Open })
            return _cached;

        var (user, pwd) = await _store.GetAsync(_tenant.Account, ct);

        var template = _cfg.GetConnectionString("TenantTemplate")!;
        var cs = string.Format(template, user, pwd, _tenant.Database);

        _cached = new MySqlConnection(cs);
        await _cached.OpenAsync(ct);
        return _cached;
    }
}

