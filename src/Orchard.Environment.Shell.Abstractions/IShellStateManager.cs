using System.Threading.Tasks;
using Orchard.DependencyInjection;
using Orchard.Environment.Shell.State;

namespace Orchard.Environment.Shell
{
    public interface IShellStateManager : IDependency
    {
        Task<ShellState> GetShellStateAsync();
        void UpdateEnabledState(ShellFeatureState featureState, ShellFeatureState.State value);
        void UpdateInstalledState(ShellFeatureState featureState, ShellFeatureState.State value);
    }
}
