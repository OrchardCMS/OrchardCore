using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace OrchardCore.Environment.Shell.Configuration
{
    public interface IShellsConfigurationSources
    {
        Task AddSourcesAsync(IConfigurationBuilder builder);
    }

    public static class ShellsConfigurationSourcesExtensions
    {
        public static async Task<IConfigurationBuilder> AddSourcesAsync(this IConfigurationBuilder builder, IShellsConfigurationSources sources)
        {
            await sources.AddSourcesAsync(builder);
            return builder;
        }
    }
}
