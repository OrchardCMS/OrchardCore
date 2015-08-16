using OrchardVNext.ContentManagement.MetaData.Models;
using OrchardVNext.DependencyInjection;

namespace OrchardVNext.ContentManagement.FieldStorage {
    public interface IFieldStorageProvider : IDependency {
        string ProviderName { get; }
        
        IFieldStorage BindStorage(
            ContentPart contentPart, 
            ContentPartFieldDefinition partFieldDefinition);
    }
}