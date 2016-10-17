using Orchard.DependencyInjection;

namespace Orchard.DisplayManagement.Layout
{
    public interface ILayoutAccessor
    {
        dynamic GetLayout();
    }
}
