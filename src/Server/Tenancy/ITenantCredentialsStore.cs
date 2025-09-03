public interface ITenantCredentialsStore
{
    ValueTask<(string Username, string Password)> GetAsync(string account, CancellationToken ct = default);
}
