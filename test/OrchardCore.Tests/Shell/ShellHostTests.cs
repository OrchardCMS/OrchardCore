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

    [Fact]
    public static async Task FindCaseInsensitiveShellSettings()
    {
        const string exactName = "Test";

        var testSettings = new ShellSettings()
        {
            Name = exactName,
            State = TenantState.Uninitialized,
        };

        await ShellHost.GetOrCreateShellContextAsync(testSettings);

        ShellHost.TryGetSettings(exactName, out var foundExactShellSettings);
        ShellHost.TryGetSettings("test", out var foundMixedShellSettings);
        ShellHost.TryGetSettings("missing", out var missingShellSettings);

        Assert.NotNull(foundExactShellSettings);
        Assert.NotNull(foundMixedShellSettings);
        Assert.Null(missingShellSettings);
    }

    [Fact]
    public static async Task FindCaseInsensitiveDefaultShellSettings()
    {
        var defaultSettings = new ShellSettings()
        {
            Name = ShellHelper.DefaultShellName,
            State = TenantState.Uninitialized,
        };

        await ShellHost.GetOrCreateShellContextAsync(defaultSettings);

        ShellHost.TryGetSettings("dEFaUlT", out var defaultTenant);

        Assert.NotNull(defaultTenant);
    }



    [Fact]
    public static async Task FindCaseInsensitiveShellContext()
    {
        const string exactName = "Test";

        var testSettings = new ShellSettings()
        {
            Name = exactName,
            State = TenantState.Uninitialized,
        };

        await ShellHost.GetOrCreateShellContextAsync(testSettings);

        ShellHost.TryGetShellContext(exactName, out var foundExactShellShellContext);
        ShellHost.TryGetShellContext("test", out var foundMixedShellShellContext);
        ShellHost.TryGetShellContext("missing", out var missingShellShellContext);

        Assert.NotNull(foundExactShellShellContext);
        Assert.NotNull(foundMixedShellShellContext);
        Assert.Null(missingShellShellContext);
    }

    [Fact]
    public static async Task FindCaseInsensitiveDefaultShellContext()
    {
        var defaultSettings = new ShellSettings()
        {
            Name = ShellHelper.DefaultShellName,
            State = TenantState.Uninitialized,
        };

        await ShellHost.GetOrCreateShellContextAsync(defaultSettings);

        ShellHost.TryGetShellContext("dEFaUlT", out var defaultTenant);

        Assert.NotNull(defaultTenant);
    }
}
