using Newtonsoft.Json.Linq;

namespace Orchard.ContentManagement
{
    public interface IContent
    {
        ContentItem ContentItem { get; }
    }
}