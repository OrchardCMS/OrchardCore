using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Setup.Services;

namespace OrchardCore.Setup.Core
{
    /// <summary>
    /// The provider that bound SetupOptions to Shell's configuration.
    /// </summary>
    public class SetupOptionsProvider : IConfigureOptions<SetupOptions>
    {
        /// <summary>
        /// The shell settings.
        /// </summary>
        private readonly ShellSettings _shellsettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupOptionsProvider"/> class.
        /// </summary>
        /// <param name="shellSettings">
        /// The shell configuration.
        /// </param>
        public SetupOptionsProvider(ShellSettings shellSettings)
        {
            _shellsettings = shellSettings;
        }

        /// <summary>
        /// Configure <see cref="SetupOptions"/> .
        /// </summary>
        /// <param name="options">
        /// The options.
        /// </param>
        public void Configure(SetupOptions options)
        {
            _shellsettings.ShellConfiguration.Bind(options);

            // Only used if the current distributed lock implementation is not a local lock.
            if (options.SetupLockExpiration <= 0)
            {
                options.SetupLockExpiration = 30_000;
            }

            if (options.SetupLockTimeout <= 0)
            {
                options.SetupLockTimeout = 30_000;
            }

            if (string.IsNullOrEmpty(options.SetupLockName))
            {
                options.SetupLockName = _shellsettings.Name;
            }
        }
    }
}
