// TenantContext.cs  ← implémentation

using System.Security.Claims;

public sealed class TenantContext : ITenantContext
{
    public TenantContext(IHttpContextAccessor accessor)
    {
        var user = accessor.HttpContext?.User
                   ?? throw new InvalidOperationException("No authenticated user");

        Account = user.FindFirst("account")?.Value
                   ?? throw new UnauthorizedAccessException("Missing account claim");

        Database = user.FindFirst("db")?.Value
                   ?? throw new UnauthorizedAccessException("Missing db claim");

        Email = user.FindFirst(ClaimTypes.Email)?.Value;
    }

    public string Account { get; }
    public string Database { get; }
    public string? Email { get; }
}
