using Fluid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OrchardCore.DataProtection.Azure;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Tests.Modules.OrchardCore.DataProtection.Azure;

public class StartupTests
{
    [Fact]
    public async Task ConfigureServices_CreateContainerDisabled_RunsAsyncStartupValidationWithoutAzureAccess()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["OrchardCore_DataProtection_Azure:ConnectionString"] = "UseDevelopmentStorage=true",
                ["OrchardCore_DataProtection_Azure:CreateContainer"] = "false",
            })
            .Build();
        var shellConfiguration = new ShellConfiguration(configuration);
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IShellConfiguration>(shellConfiguration);
        services.AddSingleton(new ShellSettings { Name = "Default" });
        services.AddSingleton(new FluidParser());
        services.AddOptions<ShellOptions>();

        var startup = new Startup(shellConfiguration, NullLogger<Startup>.Instance);
        startup.ConfigureServices(services);

        await using var serviceProvider = services.BuildServiceProvider();
        await serviceProvider.GetRequiredService<IAsyncStartupValidator>().ValidateAsync(TestContext.Current.CancellationToken);

        var options = serviceProvider.GetRequiredService<IOptions<BlobOptions>>().Value;
        Assert.False(options.CreateContainer);
    }
}
