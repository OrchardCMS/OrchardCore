using System.Threading.Tasks;

namespace OrchardCore.Navigation

{
    public interface INavigationProvider
    {
        Task BuildNavigationAsync(string name, NavigationBuilder builder);
    }
}
