namespace CorsoApi.Infrastructure;

/// <summary>
/// Abstraction for a secret store (e.g. Azure Key Vault, AWS Secrets Manager, KeyRing, lib secret).
/// </summary>
public interface ISecretStore
{
    /// <summary>
    /// Retrieves a secret value by key. Returns null if the secret does not exist.
    /// </summary>
    Task<string> GetRequiredSecretAsync(string key, CancellationToken cancellationToken = default);
}