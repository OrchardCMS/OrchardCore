using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Environment.Shell.Builders
{
    public class ShellContextOptionsSetup : IConfigureOptions<ShellContextOptions>
    {
        private readonly IShellConfiguration _shellConfiguration;

        public ShellContextOptionsSetup(IShellConfiguration shellConfiguration)
        {
            _shellConfiguration = shellConfiguration;
        }

        public void Configure(ShellContextOptions options)
        {
            _shellConfiguration.Bind(options);

            // Only used if the current distributed lock implementation is not a local lock.
            if (options.ShellActivateLockTimeout <= 0)
            {
                options.ShellActivateLockTimeout = 30_000;
            }

            if (options.ShellActivateLockExpiration <= 0)
            {
                options.ShellActivateLockExpiration = 30_000;
            }

            if (options.ShellRemovingLockTimeout <= 0)
            {
                options.ShellRemovingLockTimeout = 1_000;
            }

            if (options.ShellRemovingLockExpiration <= 0)
            {
                options.ShellRemovingLockExpiration = 60_000;
            }
        }
    }
}
