// ITenantConnectionFactory.cs
using MySql.Data.MySqlClient;

public interface ITenantConnectionFactory
{
    /// <summary>Retourne une connexion MySql ouverte vers la DB du tenant</summary>
    ValueTask<MySqlConnection> GetOpenConnectionAsync(CancellationToken ct = default);
}
