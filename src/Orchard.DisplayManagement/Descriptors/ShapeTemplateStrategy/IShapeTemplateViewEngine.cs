using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Orchard.DisplayManagement.Implementation;

namespace Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy
{
    public interface IShapeTemplateViewEngine
    {
        IEnumerable<string> TemplateFileExtensions { get; }
        Task<IHtmlContent> RenderAsync(string relativePath, DisplayContext displayContext);
    }
}