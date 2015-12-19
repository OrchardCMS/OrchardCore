using Orchard.DependencyInjection;

namespace Orchard.Environment.Navigation
{
    public interface INavigationProvider : IDependency
    {
        void BuildNavigation(string name, NavigationBuilder builder);
    }
}
