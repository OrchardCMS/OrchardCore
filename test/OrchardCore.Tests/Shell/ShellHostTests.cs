using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
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

    [Fact]
    public static async Task FindCaseInsensitiveShellSettings()
    {
        const string exactName = "Test";

        var testSettings = new ShellSettings()
        {
            Name = exactName,
        };

        await ShellHost.GetOrCreateShellContextAsync(testSettings);

        ShellHost.TryGetSettings(exactName, out var foundExactShellSettings);
        ShellHost.TryGetSettings(exactName, out var foundExactShellShellContext);

        ShellHost.TryGetSettings("test", out var foundMixedShellSettings);
        ShellHost.TryGetSettings("test", out var foundMixedShellShellContext);

        ShellHost.TryGetSettings("missing", out var missingShellSettings);
        ShellHost.TryGetSettings("missing", out var missingShellShellContext);

        Assert.NotNull(foundExactShellSettings);
        Assert.NotNull(foundExactShellShellContext);

        Assert.NotNull(foundMixedShellSettings);
        Assert.NotNull(foundMixedShellShellContext);

        Assert.Null(missingShellSettings);
        Assert.Null(missingShellShellContext);
    }

    [Fact]
    public static async Task FindCaseInsensitiveDefaultShellSettings()
    {
        var defaultSettings = new ShellSettings()
        {
            Name = ShellHelper.DefaultShellName,
        };

        await ShellHost.GetOrCreateShellContextAsync(defaultSettings);

        ShellHost.TryGetSettings("dEFaUlT", out var defaultTenant);

        Assert.NotNull(defaultTenant);
        Assert.True(defaultTenant.IsDefaultShell());
    }
}
