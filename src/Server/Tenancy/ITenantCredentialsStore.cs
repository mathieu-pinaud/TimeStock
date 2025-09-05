public interface ITenantCredentialsStore
{
    ValueTask<TenantSqlCredentials> GetAsync(string account, CancellationToken ct = default);
}
