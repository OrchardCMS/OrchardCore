using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets.Services
{
    public class DatabaseSecretStore : ISecretStore
    {
        private readonly SecretsDocumentManager _manager;
        private readonly DatabaseSecretDataProtector _databaseSecretDataProtector;
        private readonly IStringLocalizer S;

        public DatabaseSecretStore(
            SecretsDocumentManager manager,
            DatabaseSecretDataProtector databaseSecretDataProtector,
            IStringLocalizer<DatabaseSecretStore> stringLocalizer)
        {
            _manager = manager;
            _databaseSecretDataProtector = databaseSecretDataProtector;
            S = stringLocalizer;
        }

        public string Name => nameof(DatabaseSecretStore);
        public string DisplayName => S["Database Secret Store"];
        public bool IsReadOnly => false;

        public async Task<Secret> GetSecretAsync(string key, Type type)
        {
            var secretsDocument = await _manager.GetSecretsDocumentAsync();
            if (secretsDocument.Secrets.TryGetValue(key, out var documentSecret))
            {
                // TODO Disabled for dev purposes.
                // var value = _databaseSecretDataProtector.Unprotect(documentSecret.Value);
                // var secret = JsonConvert.DeserializeObject(value, type) as Secret;

                var secret = JsonConvert.DeserializeObject(documentSecret.Value, type) as Secret;

                return secret;
            }

            return null;
        }

        public async Task<TSecret> GetSecretAsync<TSecret>(string key) where TSecret : Secret, new()
        {
            var secretsDocument = await _manager.GetSecretsDocumentAsync();
            if (secretsDocument.Secrets.TryGetValue(key, out var documentSecret))
            {
                // var value = _databaseSecretDataProtector.Unprotect(documentSecret.Value);
                // var secret = JsonConvert.DeserializeObject<TSecret>(value);

                // TODO Disabled for dev purposes.
                var secret = JsonConvert.DeserializeObject<TSecret>(documentSecret.Value);

                return secret;
            }

            return null;
        }
        public Task UpdateSecretAsync(string key, Secret secret)
        {
            var documentSecret = new DocumentSecret();
            // TODO Disabled for dev purposes.
            // documentSecret.Value = _databaseSecretDataProtector.Protect(JsonConvert.SerializeObject(secret));
            documentSecret.Value = JsonConvert.SerializeObject(secret);

            return _manager.UpdateSecretAsync(key, documentSecret);
        }

        public Task RemoveSecretAsync(string key) => _manager.RemoveSecretAsync(key);
    }
}
