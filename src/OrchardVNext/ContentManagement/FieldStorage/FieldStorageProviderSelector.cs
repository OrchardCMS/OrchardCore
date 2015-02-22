using System.Collections.Generic;
using System.Linq;
using OrchardVNext.ContentManagement.MetaData.Models;

namespace OrchardVNext.ContentManagement.FieldStorage {
    public class FieldStorageProviderSelector : IFieldStorageProviderSelector {
        public const string Storage = "Storage";
        public const string DefaultProviderName = "Infoset";

        private readonly IEnumerable<IFieldStorageProvider> _storageProviders;

        public FieldStorageProviderSelector(IEnumerable<IFieldStorageProvider> storageProviders) {
            _storageProviders = storageProviders;
        }

        public IFieldStorageProvider GetProvider(ContentPartFieldDefinition partFieldDefinition) {

            IFieldStorageProvider provider = null;

            string storage;
            if (partFieldDefinition.Settings.TryGetValue(Storage, out storage))
                provider = Locate(storage);

            return provider ?? Locate(DefaultProviderName);
        }

        private IFieldStorageProvider Locate(string providerName) {
            return _storageProviders.FirstOrDefault(provider => provider.ProviderName == providerName);
        }
    }
}