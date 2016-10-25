using Microsoft.AspNetCore.Html;

namespace Orchard.ContentManagement.Models
{
    public interface IBody : IContent
    {
        IHtmlContent BodyHtml { get; }
    }
}
