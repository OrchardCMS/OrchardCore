using Microsoft.AspNetCore.Html;
using System.Threading.Tasks;

namespace OrchardCore.DisplayManagement.Implementation
{
    /// <summary>
    /// Coordinates the rendering of shapes.
    /// </summary>
    public interface IHtmlDisplay
    {
        Task<IHtmlContent> ExecuteAsync(DisplayContext context);
    }
}