using OrchardCore.Environment.Shell;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Shell;

public class ShellHostTests : SiteContext
{
    static ShellHostTests()
    {
    }

    [Theory]
    [InlineData("Tenant1", "tenant1", "tEnAnT1")]
    [InlineData(ShellSettings.DefaultShellName, "", "dEfAuLt")]
    public static async Task CanGetShellByCaseInsensitiveName(string name, string urlPrefix, string searchName)
    {
        await ShellHost.InitializeAsync();

        var shellContext = await ShellHost.GetOrCreateShellContextAsync(
            new ShellSettings()
            {
                Name = name,
                RequestUrlPrefix = urlPrefix,
            }
            .AsUninitialized());

        ShellHost.TryGetSettings(searchName, out var foundShellSettings);
        ShellHost.TryGetShellContext(searchName, out var foundShellContext);

        Assert.NotNull(shellContext);
        Assert.NotEqual(name, searchName);

        Assert.Same(foundShellSettings, shellContext.Settings);
        Assert.Same(foundShellContext, shellContext);
    }
}
