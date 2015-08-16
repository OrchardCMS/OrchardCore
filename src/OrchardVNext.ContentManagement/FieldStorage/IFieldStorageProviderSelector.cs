using OrchardVNext.ContentManagement.MetaData.Models;
using OrchardVNext.DependencyInjection;

namespace OrchardVNext.ContentManagement.FieldStorage {
    public interface IFieldStorageProviderSelector : IDependency {
        IFieldStorageProvider GetProvider(ContentPartFieldDefinition partFieldDefinition);
    }
}