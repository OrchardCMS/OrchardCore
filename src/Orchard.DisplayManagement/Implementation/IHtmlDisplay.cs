using Microsoft.AspNetCore.Html;
using System.Threading.Tasks;

namespace Orchard.DisplayManagement.Implementation
{
    /// <summary>
    /// Coordinates the rendering of shapes.
    /// This interface isn't used directly - instead you would call through a
    /// DisplayHelper created by the IDisplayHelperFactory interface
    /// </summary>
    public interface IHtmlDisplay
    {
        Task<IHtmlContent> ExecuteAsync(DisplayContext context);
    }
}