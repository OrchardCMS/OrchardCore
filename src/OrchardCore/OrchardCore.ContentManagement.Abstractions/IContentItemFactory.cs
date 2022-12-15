using System.Threading.Tasks;

namespace OrchardCore.ContentManagement;

public interface IContentItemFactory
{
    Task<ContentItem> CreateAsync(string contentTypeId, string ownerId = null);
}
