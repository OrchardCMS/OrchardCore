using Microsoft.AspNetCore.Html;

namespace Orchard.ContentManagement.Models
{
    public interface IBodyAspect
    {
        IHtmlContent Body { get; set; }
    }
}
