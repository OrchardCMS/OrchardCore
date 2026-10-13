using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Navigation;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.OptionsMonitoring;

public class DependentShellOptionsTests
{
    [Fact]
    public async Task RequestUpdate_TenantWithDependentTenant_InvalidatesDependentTenantOptionsWithoutReleasingIt()
    {
        using var context = new SiteContext().WithRecipe("SaaS");
        await context.InitializeAsync();

        using var dependentContext = new SiteContext().WithRecipe("SaaS");
        await dependentContext.InitializeAsync();

        Assert.True(SiteContext.ShellHost.TryGetShellContext(context.TenantName, out var shellContext));
        Assert.True(SiteContext.ShellHost.TryGetShellContext(dependentContext.TenantName, out var dependentShellContext));

        await shellContext.AddDependentShellAsync(dependentShellContext);

        var dependentTicks = await TenantOptionsTestHelper.GetShellContextTicksAsync(dependentContext);

        var token = dependentShellContext.ServiceProvider.GetRequiredService<ISignal>().GetToken(OptionsUpdateSignal.AllKey);

        Assert.False(token.HasChanged);

        await TenantOptionsTestHelper.UpdateSiteAsync(context, (site, _) => site.PageSize = 7, notifier => notifier.RequestUpdate<PagerOptions>());
        await dependentContext.WaitForDeferredTasksAsync(CancellationToken.None);

        Assert.True(token.HasChanged);
        Assert.Equal(dependentTicks, await TenantOptionsTestHelper.GetShellContextTicksAsync(dependentContext));
    }
}
