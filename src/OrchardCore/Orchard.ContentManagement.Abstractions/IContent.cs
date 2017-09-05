using Newtonsoft.Json.Linq;

namespace OrchardCore.ContentManagement
{
    public interface IContent
    {
        ContentItem ContentItem { get; }
    }
}