using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace OrchardCore.Secrets.Services;

public class DatabaseSecretStore : ISecretStore
{
    private readonly SecretsDocumentManager _manager;
    private readonly IDataProtector _protector;
    protected readonly IStringLocalizer S;

    public DatabaseSecretStore(
        SecretsDocumentManager manager,
        IDataProtectionProvider dataProtectionProvider,
        IStringLocalizer<DatabaseSecretStore> localizer)
    {
        _manager = manager;
        _protector = dataProtectionProvider.CreateProtector(nameof(DatabaseSecretStore));
        S = localizer;
    }

    public string Name => nameof(DatabaseSecretStore);
    public string DisplayName => S["Database Secret Store."];
    public bool IsReadOnly => false;

    public async Task<Models.Secret> GetSecretAsync(string key, Type type)
    {
        if (!typeof(Models.Secret).IsAssignableFrom(type))
        {
            throw new ArgumentException($"The type must implement '{nameof(Models.Secret)}'.");
        }

        var secretsDocument = await _manager.GetSecretsDocumentAsync();
        if (secretsDocument.Secrets.TryGetValue(key, out var protectedData))
        {
            var plainText = _protector.Unprotect(protectedData);
            return JsonConvert.DeserializeObject(plainText, type) as Models.Secret;
        }

        return null;
    }

    public Task UpdateSecretAsync(string key, Models.Secret secret) =>
        _manager.UpdateSecretAsync(key, _protector.Protect(JsonConvert.SerializeObject(secret)));

    public Task RemoveSecretAsync(string key) => _manager.RemoveSecretAsync(key);
}
