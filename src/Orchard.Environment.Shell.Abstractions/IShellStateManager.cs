using System.Threading.Tasks;
using Orchard.Environment.Shell.State;

namespace Orchard.Environment.Shell
{
    public interface IShellStateManager
    {
        Task<ShellState> GetShellStateAsync();
        Task UpdateEnabledStateAsync(ShellFeatureState featureState, ShellFeatureState.State value);
        Task UpdateInstalledStateAsync(ShellFeatureState featureState, ShellFeatureState.State value);
    }
}
