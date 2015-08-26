using Orchard.ContentManagement.MetaData.Models;
using Orchard.DependencyInjection;

namespace Orchard.ContentManagement.FieldStorage {
    public interface IFieldStorageProviderSelector : IDependency {
        IFieldStorageProvider GetProvider(ContentPartFieldDefinition partFieldDefinition);
    }
}