using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Admin.Configuration;
using OrchardCore.Admin.Models;
using OrchardCore.Admin.Services;
using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.Tests.Modules.OrchardCore.Admin;

public class DefaultAdminListServiceTests
{
    [Fact]
    public async Task GetLayoutAsync_ReturnsList_WhenNothingIsConfigured()
    {
        var service = CreateService();

        Assert.Equal(AdminListConstants.List, await service.GetLayoutAsync("Contents", cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetLayoutAsync_ReturnsConfiguredSiteDefault()
    {
        var site = new SiteSettings();
        site.Put(new AdminSettings { ListLayout = AdminListConstants.Table });

        var service = CreateService(Configure(new AdminListOptions(), site));

        Assert.Equal(AdminListConstants.Table, await service.GetLayoutAsync("Contents", cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetLayoutAsync_RequestedLayoutWinsOverSiteDefault()
    {
        var site = new SiteSettings();
        site.Put(new AdminSettings { ListLayout = AdminListConstants.Table });

        var service = CreateService(Configure(new AdminListOptions(), site));

        Assert.Equal("Cards", await service.GetLayoutAsync("Contents", " Cards ", TestContext.Current.CancellationToken));
        Assert.Equal(AdminListConstants.Table, await service.GetLayoutAsync("Contents", "   ", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetActionsLayoutAsync_ReturnsButtons_WhenNothingIsConfigured()
    {
        var service = CreateService();

        Assert.Equal(AdminListActionsLayouts.Buttons, await service.GetActionsLayoutAsync(cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(AdminListActionsLayouts.Buttons, await service.GetActionsLayoutAsync("Contents", cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetActionsLayoutAsync_RequestedLayoutWinsOverSiteDefault()
    {
        var site = new SiteSettings();
        site.Put(new AdminSettings { ListActionsLayout = AdminListActionsLayouts.Menu });

        var service = CreateService(Configure(new AdminListOptions(), site));

        Assert.Equal(AdminListActionsLayouts.Menu, await service.GetActionsLayoutAsync("Contents", cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal("Icons", await service.GetActionsLayoutAsync("Contents", " Icons ", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetLayoutAsync_UsesTheConfiguredDefault_WhenTheSiteHasNoChoice()
    {
        var service = CreateService(new AdminListOptions { DefaultLayout = AdminListConstants.Grid });

        Assert.Equal(AdminListConstants.Grid, await service.GetLayoutAsync("Contents", cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetLayoutAsync_SiteSettingWinsOverTheConfiguredDefault()
    {
        var site = new SiteSettings();
        site.Put(new AdminSettings { ListLayout = AdminListConstants.Table });

        var service = CreateService(Configure(new AdminListOptions { DefaultLayout = AdminListConstants.Grid }, site));

        Assert.Equal(AdminListConstants.Table, await service.GetLayoutAsync("Contents", cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetActionsLayoutAsync_UsesTheConfiguredDefault_WhenTheSiteHasNoChoice()
    {
        var service = CreateService(new AdminListOptions { DefaultActionsLayout = AdminListActionsLayouts.Menu });

        Assert.Equal(AdminListActionsLayouts.Menu, await service.GetActionsLayoutAsync("Contents", cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Defaults_FallBackToTheShippedLayouts_WhenTheConfiguredValuesAreBlank()
    {
        var service = CreateService(new AdminListOptions { DefaultLayout = "  ", DefaultActionsLayout = "" });

        Assert.Equal(AdminListConstants.List, await service.GetLayoutAsync("Contents", cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(AdminListActionsLayouts.Buttons, await service.GetActionsLayoutAsync("Contents", cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetColumnsAsync_ReturnsDefaults_WhenThereAreNoProviders()
    {
        var service = CreateService();
        var defaults = new[] { new AdminListColumn { Name = "Title" }, new AdminListColumn { Name = "Actions" } };

        var columns = await service.GetColumnsAsync("Contents", defaults, TestContext.Current.CancellationToken);

        Assert.Equal(["Title", "Actions"], columns.Select(c => c.Name));
        Assert.NotSame(defaults, columns);
    }

    [Fact]
    public async Task GetColumnsAsync_LetsProvidersAlterTheColumnsOfTheirList()
    {
        var service = CreateService(new CultureColumnProvider());
        var defaults = new[] { new AdminListColumn { Name = "Title" }, new AdminListColumn { Name = "Actions" } };

        var contents = await service.GetColumnsAsync("Contents", defaults, TestContext.Current.CancellationToken);
        var users = await service.GetColumnsAsync("Users", defaults, TestContext.Current.CancellationToken);

        Assert.Equal(["Title", "Culture", "Actions"], contents.Select(c => c.Name));
        Assert.Equal(["Title", "Actions"], users.Select(c => c.Name));
    }

    [Fact]
    public async Task GetColumnsAsync_AssignsPositionsToDefaults_AndSortsByPosition_WhateverTheProviderOrder()
    {
        // Two features insert columns between the defaults; the result does not depend on the order they run in.
        var defaults = new[] { new AdminListColumn { Name = "Select" }, new AdminListColumn { Name = "Title" }, new AdminListColumn { Name = "Actions", Position = "end" } };

        var forward = await CreateService(new PositionedColumnProvider("Owner", "15"), new PositionedColumnProvider("Culture", "25"), new PositionedColumnProvider("Unpositioned", null))
            .GetColumnsAsync("Contents", defaults, TestContext.Current.CancellationToken);

        var backward = await CreateService(new PositionedColumnProvider("Unpositioned", null), new PositionedColumnProvider("Culture", "25"), new PositionedColumnProvider("Owner", "15"))
            .GetColumnsAsync("Contents", defaults.Select(c => new AdminListColumn { Name = c.Name, Position = c.Position }), TestContext.Current.CancellationToken);

        Assert.Equal(["Select", "Owner", "Title", "Culture", "Unpositioned", "Actions"], forward.Select(c => c.Name));
        Assert.Equal(forward.Select(c => c.Name), backward.Select(c => c.Name));
        Assert.Equal(["10", "15", "20", "25", null, "end"], forward.Select(c => c.Position));
    }

    [Fact]
    public async Task GetColumnsAsync_ProvidersCanFindAndRemoveColumns()
    {
        var service = CreateService(new RemovingColumnProvider("Title"));
        var defaults = new[] { new AdminListColumn { Name = "Select" }, new AdminListColumn { Name = "Title" }, new AdminListColumn { Name = "Actions" } };

        var columns = await service.GetColumnsAsync("Contents", defaults, TestContext.Current.CancellationToken);

        Assert.Equal(["Select", "Actions"], columns.Select(c => c.Name));
    }

    private sealed class PositionedColumnProvider(string name, string position) : IAdminListColumnProvider
    {
        public Task BuildAsync(AdminListColumnsContext context, CancellationToken cancellationToken = default)
        {
            context.Columns.Add(new AdminListColumn { Name = name, Position = position, Zones = [name] });

            return Task.CompletedTask;
        }
    }

    private sealed class RemovingColumnProvider(string name) : IAdminListColumnProvider
    {
        public Task BuildAsync(AdminListColumnsContext context, CancellationToken cancellationToken = default)
        {
            Assert.NotNull(context.Find(name));
            Assert.True(context.Remove(name));
            Assert.Null(context.Find(name));

            return Task.CompletedTask;
        }
    }

    private static DefaultAdminListService CreateService(params IAdminListColumnProvider[] providers)
        => CreateService(new AdminListOptions(), providers);

    private static DefaultAdminListService CreateService(AdminListOptions options, params IAdminListColumnProvider[] providers)
        => new(providers, Mock.Of<IOptionsMonitor<AdminListOptions>>(m => m.CurrentValue == options), NullLogger<DefaultAdminListService>.Instance);

    // The effective defaults are produced by AdminListOptionsConfiguration, which layers the site settings on
    // top of the values bound from appsettings.json.
    private static AdminListOptions Configure(AdminListOptions options, ISite site)
    {
        var siteService = new Mock<ISiteService>();
        siteService.Setup(s => s.GetSiteSettingsAsync()).ReturnsAsync(site);

        new AdminListOptionsConfiguration(siteService.Object).Configure(options);

        return options;
    }

    private sealed class CultureColumnProvider : IAdminListColumnProvider
    {
        public Task BuildAsync(AdminListColumnsContext context, CancellationToken cancellationToken = default)
        {
            if (context.ListName == "Contents")
            {
                context.Columns.Add(new AdminListColumn
                {
                    Name = "Culture",
                    Position = "15",
                    Title = new LocalizedString("Culture", "Culture"),
                    Zones = ["Culture"],
                });
            }

            return Task.CompletedTask;
        }
    }
}
