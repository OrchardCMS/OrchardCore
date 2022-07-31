using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Tests.Apis.Context;
using Xunit;

namespace OrchardCore.Tests.Shell;

public class ShellHostTests : SiteContext
{
    private readonly IShellHost _shellHost;

    public ShellHostTests()
    {
        _shellHost = Site.Services.GetRequiredService<IShellHost>();
    }

    [Theory]
    [InlineData("Test", "test")]
    [InlineData("OrchardCore", "orchardcore")]
    [InlineData(ShellHelper.DefaultShellName, "DeFauLt")]
    public async Task FindCaseInsensitiveShellSettings(string shellName, string anotherVariantOfShellName)
    {
        await GetOrCreateShellContextAsync(shellName);
        _shellHost.TryGetSettings(shellName, out var shellSettings);
        _shellHost.TryGetSettings(anotherVariantOfShellName, out var anotherVariantOfShellSettings);

        Assert.NotEqual(shellName, anotherVariantOfShellName);
        Assert.NotNull(shellSettings);
        Assert.NotNull(anotherVariantOfShellSettings);
        Assert.Same(shellSettings, anotherVariantOfShellSettings);
    }

    [Theory]
    [InlineData("Test", "test")]
    [InlineData("OrchardCore", "orchardcore")]
    [InlineData(ShellHelper.DefaultShellName, "dEfAut")]
    public async Task FindCaseInsensitiveShellContext(string shellName, string anotherVariantOfShellName)
    {
        await GetOrCreateShellContextAsync(shellName);

        _shellHost.TryGetShellContext(shellName, out var shellContext);
        _shellHost.TryGetShellContext(anotherVariantOfShellName, out var anotherVariantOfShellContext);

        Assert.NotEqual(shellName, anotherVariantOfShellName);
        Assert.NotNull(shellContext);
        Assert.NotNull(anotherVariantOfShellContext);
        Assert.Same(shellContext, anotherVariantOfShellContext);
    }

    private async Task<ShellContext> GetOrCreateShellContextAsync(string name)
    {
        var shellSettings = new ShellSettings()
        {
            Name = name,
            State = TenantState.Uninitialized,
        };

        return await _shellHost.GetOrCreateShellContextAsync(shellSettings);
    }
}
