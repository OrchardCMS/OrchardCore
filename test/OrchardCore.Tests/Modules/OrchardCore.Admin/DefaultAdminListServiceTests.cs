using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
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
        var service = CreateService(new SiteSettings());

        Assert.Equal(AdminListLayouts.List, await service.GetLayoutAsync("Contents"));
    }

    [Fact]
    public async Task GetLayoutAsync_ReturnsConfiguredSiteDefault()
    {
        var site = new SiteSettings();
        site.Put(new AdminSettings { ListLayout = AdminListLayouts.Table });

        var service = CreateService(site);

        Assert.Equal(AdminListLayouts.Table, await service.GetLayoutAsync("Contents"));
    }

    [Fact]
    public async Task GetLayoutAsync_RequestedLayoutWinsOverSiteDefault()
    {
        var site = new SiteSettings();
        site.Put(new AdminSettings { ListLayout = AdminListLayouts.Table });

        var service = CreateService(site);

        Assert.Equal("Cards", await service.GetLayoutAsync("Contents", " Cards "));
        Assert.Equal(AdminListLayouts.Table, await service.GetLayoutAsync("Contents", "   "));
    }

    [Fact]
    public async Task GetActionsLayoutAsync_ReturnsButtons_WhenNothingIsConfigured()
    {
        var service = CreateService(new SiteSettings());

        Assert.Equal(AdminListActionsLayouts.Buttons, await service.GetActionsLayoutAsync());
        Assert.Equal(AdminListActionsLayouts.Buttons, await service.GetActionsLayoutAsync("Contents"));
    }

    [Fact]
    public async Task GetActionsLayoutAsync_RequestedLayoutWinsOverSiteDefault()
    {
        var site = new SiteSettings();
        site.Put(new AdminSettings { ListActionsLayout = AdminListActionsLayouts.Menu });

        var service = CreateService(site);

        Assert.Equal(AdminListActionsLayouts.Menu, await service.GetActionsLayoutAsync("Contents"));
        Assert.Equal("Icons", await service.GetActionsLayoutAsync("Contents", " Icons "));
    }

    [Fact]
    public async Task GetColumnsAsync_ReturnsDefaults_WhenThereAreNoProviders()
    {
        var service = CreateService(new SiteSettings());
        var defaults = new[] { new AdminListColumn { Name = "Title" }, new AdminListColumn { Name = "Actions" } };

        var columns = await service.GetColumnsAsync("Contents", defaults);

        Assert.Equal(["Title", "Actions"], columns.Select(c => c.Name));
        Assert.NotSame(defaults, columns);
    }

    [Fact]
    public async Task GetColumnsAsync_LetsProvidersAlterTheColumnsOfTheirList()
    {
        var service = CreateService(new SiteSettings(), new CultureColumnProvider());
        var defaults = new[] { new AdminListColumn { Name = "Title" }, new AdminListColumn { Name = "Actions" } };

        var contents = await service.GetColumnsAsync("Contents", defaults);
        var users = await service.GetColumnsAsync("Users", defaults);

        Assert.Equal(["Title", "Culture", "Actions"], contents.Select(c => c.Name));
        Assert.Equal(["Title", "Actions"], users.Select(c => c.Name));
    }

    [Fact]
    public async Task GetColumnsAsync_AssignsPositionsToDefaults_AndSortsByPosition_WhateverTheProviderOrder()
    {
        // Two features insert columns between the defaults; the result does not depend on the order they run in.
        var defaults = new[] { new AdminListColumn { Name = "Select" }, new AdminListColumn { Name = "Title" }, new AdminListColumn { Name = "Actions", Position = "end" } };

        var forward = await CreateService(new SiteSettings(), new PositionedColumnProvider("Owner", "15"), new PositionedColumnProvider("Culture", "25"), new PositionedColumnProvider("Unpositioned", null))
            .GetColumnsAsync("Contents", defaults);

        var backward = await CreateService(new SiteSettings(), new PositionedColumnProvider("Unpositioned", null), new PositionedColumnProvider("Culture", "25"), new PositionedColumnProvider("Owner", "15"))
            .GetColumnsAsync("Contents", defaults.Select(c => new AdminListColumn { Name = c.Name, Position = c.Position }));

        Assert.Equal(["Select", "Owner", "Title", "Culture", "Unpositioned", "Actions"], forward.Select(c => c.Name));
        Assert.Equal(forward.Select(c => c.Name), backward.Select(c => c.Name));
        Assert.Equal(["10", "15", "20", "25", null, "end"], forward.Select(c => c.Position));
    }

    [Fact]
    public async Task GetColumnsAsync_ProvidersCanFindAndRemoveColumns()
    {
        var service = CreateService(new SiteSettings(), new RemovingColumnProvider("Title"));
        var defaults = new[] { new AdminListColumn { Name = "Select" }, new AdminListColumn { Name = "Title" }, new AdminListColumn { Name = "Actions" } };

        var columns = await service.GetColumnsAsync("Contents", defaults);

        Assert.Equal(["Select", "Actions"], columns.Select(c => c.Name));
    }

    private sealed class PositionedColumnProvider(string name, string position) : IAdminListColumnProvider
    {
        public Task BuildAsync(AdminListColumnsContext context)
        {
            context.Columns.Add(new AdminListColumn { Name = name, Position = position, Zones = [name] });

            return Task.CompletedTask;
        }
    }

    private sealed class RemovingColumnProvider(string name) : IAdminListColumnProvider
    {
        public Task BuildAsync(AdminListColumnsContext context)
        {
            Assert.NotNull(context.Find(name));
            Assert.True(context.Remove(name));
            Assert.Null(context.Find(name));

            return Task.CompletedTask;
        }
    }

    private static DefaultAdminListService CreateService(ISite site, params IAdminListColumnProvider[] providers)
    {
        var siteService = new Mock<ISiteService>();
        siteService.Setup(s => s.GetSiteSettingsAsync()).ReturnsAsync(site);

        return new DefaultAdminListService(siteService.Object, providers, NullLogger<DefaultAdminListService>.Instance);
    }

    private sealed class CultureColumnProvider : IAdminListColumnProvider
    {
        public Task BuildAsync(AdminListColumnsContext context)
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
