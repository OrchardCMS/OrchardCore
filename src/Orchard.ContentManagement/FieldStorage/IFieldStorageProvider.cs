using Orchard.ContentManagement.MetaData.Models;
using Orchard.DependencyInjection;

namespace Orchard.ContentManagement.FieldStorage {
    public interface IFieldStorageProvider : IDependency {
        string ProviderName { get; }
        
        IFieldStorage BindStorage(
            ContentPart contentPart, 
            ContentPartFieldDefinition partFieldDefinition);
    }
}