using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;

namespace PrefixKeyVault;
public class PrefixKeyVaultSecretManager : KeyVaultSecretManager
{
    private readonly string _prefix;

    public PrefixKeyVaultSecretManager(string environment)
    {
        // Example prefix: "prod-"
        _prefix = $"{environment.ToLower()}-";
    }

    // 1. Only load secrets that match our current environment prefix
    public override bool Load(SecretProperties properties)
    {
        return properties.Name.StartsWith(_prefix, StringComparison.OrdinalIgnoreCase);
    }

    // 2. Strip the environment prefix out so configuration lookups remain uniform
    public override string GetKey(KeyVaultSecret secret)
    {
        // First strip the prefix, then let the base method handle the '--' to ':' conversion
        string normalizedName = secret.Name.Substring(_prefix.Length);
        
        return base.GetKey(new KeyVaultSecret(normalizedName, secret.Value));
    }
}