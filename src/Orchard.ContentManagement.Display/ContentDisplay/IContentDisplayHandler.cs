using Orchard.DependencyInjection;
using Orchard.DisplayManagement.Handlers;

namespace Orchard.ContentManagement.Display.ContentDisplay
{
    public interface IContentDisplayHandler : IDisplayHandler<IContent>, IDependency
    {
    }
}
