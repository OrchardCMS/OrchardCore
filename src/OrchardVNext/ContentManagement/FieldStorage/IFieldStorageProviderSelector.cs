using OrchardVNext.ContentManagement.MetaData.Models;

namespace OrchardVNext.ContentManagement.FieldStorage {
    public interface IFieldStorageProviderSelector : IDependency {
        IFieldStorageProvider GetProvider(ContentPartFieldDefinition partFieldDefinition);
    }
}