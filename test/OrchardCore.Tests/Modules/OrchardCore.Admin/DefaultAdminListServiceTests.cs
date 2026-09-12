using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin.Services;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Environment.Extensions;
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

        var columns = await service.GetColumnsAsync("Contents", defaults, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["Title", "Actions"], columns.Select(c => c.Name));
        Assert.NotSame(defaults, columns);
    }

    [Fact]
    public async Task GetColumnsAsync_LetsProvidersAlterTheColumnsOfTheirList()
    {
        var service = CreateService(new CultureColumnProvider());
        var defaults = new[] { new AdminListColumn { Name = "Title" }, new AdminListColumn { Name = "Actions" } };

        var contents = await service.GetColumnsAsync("Contents", defaults, cancellationToken: TestContext.Current.CancellationToken);
        var users = await service.GetColumnsAsync("Users", defaults, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["Title", "Culture", "Actions"], contents.Select(c => c.Name));
        Assert.Equal(["Title", "Actions"], users.Select(c => c.Name));
    }

    [Fact]
    public async Task GetColumnsAsync_AssignsPositionsToDefaults_AndSortsByPosition_WhateverTheProviderOrder()
    {
        // Two features insert columns between the defaults; the result does not depend on the order they run in.
        var defaults = new[] { new AdminListColumn { Name = "Select" }, new AdminListColumn { Name = "Title" }, new AdminListColumn { Name = "Actions", Position = "end" } };

        var forward = await CreateService(new PositionedColumnProvider("Owner", "15"), new PositionedColumnProvider("Culture", "25"), new PositionedColumnProvider("Unpositioned", null))
            .GetColumnsAsync("Contents", defaults, cancellationToken: TestContext.Current.CancellationToken);

        var backward = await CreateService(new PositionedColumnProvider("Unpositioned", null), new PositionedColumnProvider("Culture", "25"), new PositionedColumnProvider("Owner", "15"))
            .GetColumnsAsync("Contents", defaults.Select(c => new AdminListColumn { Name = c.Name, Position = c.Position }), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["Select", "Owner", "Title", "Culture", "Unpositioned", "Actions"], forward.Select(c => c.Name));
        Assert.Equal(forward.Select(c => c.Name), backward.Select(c => c.Name));
        Assert.Equal(["10", "15", "20", "25", null, "end"], forward.Select(c => c.Position));
    }

    [Fact]
    public async Task GetColumnsAsync_ProvidersCanFindAndRemoveColumns()
    {
        var service = CreateService(new RemovingColumnProvider("Title"));
        var defaults = new[] { new AdminListColumn { Name = "Select" }, new AdminListColumn { Name = "Title" }, new AdminListColumn { Name = "Actions" } };

        var columns = await service.GetColumnsAsync("Contents", defaults, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["Select", "Actions"], columns.Select(c => c.Name));
    }

    [Fact]
    public async Task GetColumnsAsync_PassesTheDataOfThePageToTheProviders()
    {
        var provider = new DataColumnProvider();
        var service = CreateService(provider);
        var data = new Dictionary<string, object>
        {
            ["ContentTypes"] = new[] { "BlogPost" },
            ["Count"] = 3,
        };

        var columns = await service.GetColumnsAsync("Contents", [new AdminListColumn { Name = "Title" }], data, TestContext.Current.CancellationToken);

        // The provider added a column for the content type the page is filtered by.
        Assert.Equal(["Title", "BlogPost"], columns.Select(column => column.Name));

        // A key that is not there, or that holds another type, falls back instead of throwing.
        Assert.Equal(3, provider.Count);
        Assert.Null(provider.MissingKey);
        Assert.Null(provider.WrongType);
    }

    [Fact]
    public async Task GetColumnsAsync_GivesProvidersEmptyData_WhenThePagePassesNone()
    {
        var provider = new DataColumnProvider();
        var service = CreateService(provider);

        var columns = await service.GetColumnsAsync("Contents", [new AdminListColumn { Name = "Title" }], cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["Title"], columns.Select(column => column.Name));
        Assert.Empty(provider.Data);
    }

    private sealed class DataColumnProvider : IAdminListColumnProvider
    {
        public IReadOnlyDictionary<string, object> Data { get; private set; }

        public int Count { get; private set; }

        public string MissingKey { get; private set; }

        public string WrongType { get; private set; }

        public Task BuildAsync(AdminListColumnsContext context, CancellationToken cancellationToken = default)
        {
            Data = context.Data;
            Count = context.GetData<int>("Count");
            MissingKey = context.GetData<string>("NotThere");
            WrongType = context.GetData<string>("Count");

            if (context.TryGetData<string[]>("ContentTypes", out var contentTypes))
            {
                foreach (var contentType in contentTypes)
                {
                    context.Columns.Add(new AdminListColumn { Name = contentType, Zones = [contentType] });
                }
            }

            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task GetLayoutAsync_IgnoresTheQueryString_WhenTheSiteKeepsTheChoice()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString("?layout=Grid");

        var service = CreateService(new AdminListOptions { DefaultLayout = AdminListConstants.Table }, httpContext, [], AdminListConstants.List, AdminListConstants.Table, AdminListConstants.Grid);

        Assert.Equal(AdminListConstants.Table, await service.GetLayoutAsync("Contents", cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetLayoutAsync_TakesTheLayoutFromTheQueryString_AndRemembersIt()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString("?layout=Grid");

        var options = new AdminListOptions { DefaultLayout = AdminListConstants.Table, AllowUserSelection = true };
        var service = CreateService(options, httpContext, [], AdminListConstants.List, AdminListConstants.Table, AdminListConstants.Grid);

        Assert.Equal(AdminListConstants.Grid, await service.GetLayoutAsync("Contents", cancellationToken: TestContext.Current.CancellationToken));

        // The choice went to the cookie, so the next request opens the list the same way.
        var cookie = httpContext.Response.Headers.SetCookie.ToString();

        Assert.Contains(AdminListLayoutPreference.CookieName, cookie);

        // The value is url encoded on the way out, so the separator reads as %3A here.
        Assert.Contains("Contents%3AGrid", cookie);
    }

    [Fact]
    public async Task GetLayoutAsync_TakesTheLayoutTheUserPickedBefore()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Cookie = $"{AdminListLayoutPreference.CookieName}=Contents:Grid|Users:Table";

        var options = new AdminListOptions { DefaultLayout = AdminListConstants.List, AllowUserSelection = true };
        var service = CreateService(options, httpContext, [], AdminListConstants.List, AdminListConstants.Table, AdminListConstants.Grid);

        Assert.Equal(AdminListConstants.Grid, await service.GetLayoutAsync("Contents", cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(AdminListConstants.Table, await service.GetLayoutAsync("Users", cancellationToken: TestContext.Current.CancellationToken));

        // A list the user never chose for keeps the default of the site.
        Assert.Equal(AdminListConstants.List, await service.GetLayoutAsync("Queries", cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetLayoutAsync_IgnoresALayoutThisSiteCannotRender()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString("?layout=Cards");
        httpContext.Request.Headers.Cookie = $"{AdminListLayoutPreference.CookieName}=Users:Cards";

        var options = new AdminListOptions { DefaultLayout = AdminListConstants.List, AllowUserSelection = true };
        var service = CreateService(options, httpContext, [], AdminListConstants.List, AdminListConstants.Table, AdminListConstants.Grid);

        Assert.Equal(AdminListConstants.List, await service.GetLayoutAsync("Contents", cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(AdminListConstants.List, await service.GetLayoutAsync("Users", cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetLayoutAsync_TheLayoutOfThePageWinsOverTheChoiceOfTheUser()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Cookie = $"{AdminListLayoutPreference.CookieName}=Contents:Grid";

        var options = new AdminListOptions { DefaultLayout = AdminListConstants.List, AllowUserSelection = true };
        var service = CreateService(options, httpContext, [], AdminListConstants.List, AdminListConstants.Table, AdminListConstants.Grid);

        Assert.Equal(AdminListConstants.Table, await service.GetLayoutAsync("Contents", AdminListConstants.Table, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetAvailableLayoutsAsync_ReturnsTheLayoutsOfTheShapeTable()
    {
        var service = CreateService(new AdminListOptions(), new DefaultHttpContext(), [], AdminListConstants.Table, AdminListConstants.List);

        Assert.Equal([AdminListConstants.List, AdminListConstants.Table], await service.GetAvailableLayoutsAsync(TestContext.Current.CancellationToken));
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
        => CreateService(options, new DefaultHttpContext(), providers);

    // The layouts a site can render come from its shape table, so a test says which ones exist.
    private static DefaultAdminListService CreateService(
        AdminListOptions options,
        HttpContext httpContext,
        IAdminListColumnProvider[] providers,
        params string[] availableLayouts)
    {
        var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };

        var theme = new Mock<IExtensionInfo>();
        theme.Setup(t => t.Id).Returns("TheAdmin");

        var themeManager = new Mock<IThemeManager>();
        themeManager.Setup(t => t.GetThemeAsync()).ReturnsAsync(theme.Object);

        var shapeTable = new ShapeTable(
            new Dictionary<string, ShapeDescriptor>(StringComparer.OrdinalIgnoreCase),
            availableLayouts.ToDictionary(
                layout => AdminListConstants.OptionShapePrefix + layout,
                layout => new ShapeBinding { BindingName = AdminListConstants.OptionShapePrefix + layout },
                StringComparer.OrdinalIgnoreCase));

        var shapeTableManager = new Mock<IShapeTableManager>();
        shapeTableManager.Setup(s => s.GetShapeTableAsync(It.IsAny<string>())).ReturnsAsync(shapeTable);

        return new DefaultAdminListService(
            providers,
            Mock.Of<IOptionsMonitor<AdminListOptions>>(m => m.CurrentValue == options),
            NullLogger<DefaultAdminListService>.Instance,
            httpContextAccessor,
            new AdminListLayoutPreference(httpContextAccessor),
            themeManager.Object,
            shapeTableManager.Object);
    }

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
