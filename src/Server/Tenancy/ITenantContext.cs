// ITenantContext.cs  ← contrat
public interface ITenantContext
{
    /// <summary>Nom du compte (tenant), ex.: "AcmeCorp"</summary>
    string Account  { get; }
    /// <summary>Nom de la base MySQL, ex.: "db_AcmeCorp"</summary>
    string Database { get; }
    /// <summary>Email de l'utilisateur courant (facultatif)</summary>
    string? Email   { get; }
}
