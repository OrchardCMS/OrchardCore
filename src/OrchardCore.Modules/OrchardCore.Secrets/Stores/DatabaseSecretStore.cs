using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using OrchardCore.Secrets.Services;

namespace OrchardCore.Secrets.Stores;

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
    public string DisplayName => S["Database Secret Store"];
    public bool IsReadOnly => false;

    public async Task<Models.SecretBase> GetSecretAsync(string name, Type type)
    {
        if (!typeof(Models.SecretBase).IsAssignableFrom(type))
        {
            throw new ArgumentException($"The type must implement '{nameof(Models.SecretBase)}'.");
        }

        var secretsDocument = await _manager.GetSecretsDocumentAsync();
        if (secretsDocument.Secrets.TryGetValue(name, out var protectedData))
        {
            var plainText = _protector.Unprotect(protectedData);
            return JsonConvert.DeserializeObject(plainText, type) as Models.SecretBase;
        }

        return null;
    }

    public Task UpdateSecretAsync(string name, Models.SecretBase secret) =>
        _manager.UpdateSecretAsync(name, _protector.Protect(JsonConvert.SerializeObject(secret)));

    public Task RemoveSecretAsync(string name) => _manager.RemoveSecretAsync(name);
}
