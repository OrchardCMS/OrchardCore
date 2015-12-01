using Orchard.DependencyInjection;

namespace Orchard.DisplayManagement.Layout
{
    public interface ILayoutAccessor : IDependency
    {
        dynamic GetLayout();
    }
}
