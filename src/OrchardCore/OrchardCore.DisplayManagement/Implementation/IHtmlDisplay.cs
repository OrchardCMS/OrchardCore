using Microsoft.AspNetCore.Html;
using System.Threading.Tasks;

namespace OrchardCore.DisplayManagement.Implementation
{
    /// <summary>
    /// Coordinates the rendering of shapes.
    /// This interface isn't used directly - instead you would call it
    /// through a IDisplayHelper interface.
    /// </summary>
    public interface IHtmlDisplay
    {
        Task<IHtmlContent> ExecuteAsync(DisplayContext context);
    }
}