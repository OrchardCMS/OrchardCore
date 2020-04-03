using System.Threading.Tasks;
using OrchardCore.Environment.Shell.State;

namespace OrchardCore.Environment.Shell
{
    public interface IShellStateManager
    {
        Task<ShellState> GetShellStateAsync();
        Task UpdateEnabledStateAsync(ShellFeatureState featureState, ShellFeatureState.State value);
        Task UpdateInstalledStateAsync(ShellFeatureState featureState, ShellFeatureState.State value);
    }
}
