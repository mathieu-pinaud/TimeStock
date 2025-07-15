public interface ITenantCredentialsStore
{
    ValueTask<(string User, string Password)> GetAsync(string account, CancellationToken ct = default);
}
