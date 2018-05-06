using System.IO;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace OrchardCore.DataProtection
{
    public class Startup : StartupBase
    {
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IOptions<ShellOptions> _shellOptions;
        private readonly ShellSettings _shellSettings;

        public Startup(
            IDataProtectionProvider dataProtectionProvider,
            IOptions<ShellOptions> shellOptions,
            ShellSettings shellSettings)
        {
            _dataProtectionProvider = dataProtectionProvider;
            _shellOptions = shellOptions;
            _shellSettings = shellSettings;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            //var directory = Directory.CreateDirectory(Path.Combine(
            //    _shellOptions.Value.ShellsApplicationDataPath,
            //    _shellOptions.Value.ShellsContainerName,
            //    _shellSettings.Name, "DataProtection-Keys"));

            // Re-register the data protection services to be tenant-aware so that modules that internally
            // rely on IDataProtector/IDataProtectionProvider automatically get an isolated instance that
            // manages its own key ring and doesn't allow decrypting payloads encrypted by another tenant.
            // By default, the key ring is stored in the tenant directory of the configured App_Data path.
            //services.Add(new ServiceCollection()
            //    .AddDataProtection()
            //    .PersistKeysToFileSystem(directory)
            //    .SetApplicationName(_shellSettings.Name)
            //    .Services);
        }
    }
}
