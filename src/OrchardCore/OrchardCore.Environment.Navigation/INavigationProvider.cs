using System.Threading.Tasks;

namespace OrchardCore.Environment.Navigation
{
    public interface INavigationProvider
    {
        Task BuildNavigationAsync(string name, NavigationBuilder builder);
    }
}
