using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Tests.Apis.Context;
using Xunit;

namespace OrchardCore.Tests.Shell;

public class ShellHostTests : SiteContext
{
    public static IShellHost ShellHost { get; }

    static ShellHostTests()
    {
        ShellHost = Site.Services.GetRequiredService<IShellHost>();
    }

    [Theory]
    [InlineData("Tenant1", "tEnAnT1")]
    [InlineData(ShellHelper.DefaultShellName, "dEfAuLt")]
    public static async Task CanGetShellByCaseInsensitiveName(string name, string searchName)
    {
        await ShellHost.InitializeAsync();

        var shellContext = await ShellHost.GetOrCreateShellContextAsync(
            new ShellSettings()
            {
                Name = name,
                State = TenantState.Uninitialized,
            });

        ShellHost.TryGetSettings(searchName, out var foundShellSettings);
        ShellHost.TryGetShellContext(searchName, out var foundShellContext);

        Assert.NotNull(shellContext);
        Assert.NotEqual(name, searchName);

        Assert.Same(foundShellSettings, shellContext.Settings);
        Assert.Same(foundShellContext, shellContext);
    }
}
