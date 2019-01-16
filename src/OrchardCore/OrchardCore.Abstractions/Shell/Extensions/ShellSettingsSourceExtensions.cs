using Microsoft.Extensions.Configuration;

namespace OrchardCore.Environment.Shell
{
    public static class ShellSettingsSourceExtensions
    {
        public static IConfigurationBuilder AddSources(this IConfigurationBuilder builder, IShellSettingsSources sources)
        {
            sources?.AddSources(builder);
            return builder;
        }
    }
}