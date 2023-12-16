using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using OrchardCore.Secrets.Models;
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

    public async Task<SecretBase> GetSecretAsync(string name, Type type)
    {
        if (!typeof(SecretBase).IsAssignableFrom(type))
        {
            throw new ArgumentException($"The type must implement '{nameof(SecretBase)}'.");
        }

        var document = await _manager.GetSecretsAsync();
        if (!document.Secrets.TryGetValue(name, out var protectedData))
        {
            return null;
        }

        var plaintext = _protector.Unprotect(protectedData);
        return JsonConvert.DeserializeObject(plaintext, type) as SecretBase;
    }

    public Task UpdateSecretAsync(string name, SecretBase secret) =>
        _manager.UpdateSecretAsync(name, _protector.Protect(JsonConvert.SerializeObject(secret)));

    public Task RemoveSecretAsync(string name) => _manager.RemoveSecretAsync(name);
}
