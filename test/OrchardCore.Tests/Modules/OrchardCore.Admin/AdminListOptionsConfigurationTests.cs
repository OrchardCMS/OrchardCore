using OrchardCore.Admin;
using OrchardCore.Admin.Configuration;
using OrchardCore.Admin.Models;
using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.Tests.Modules.OrchardCore.Admin;

public class AdminListOptionsConfigurationTests
{
    [Fact]
    public void Configure_SiteSettings_WinOverTheConfiguredValues()
    {
        var site = new SiteSettings();
        site.Put(new AdminSettings
        {
            ListLayout = AdminListConstants.Table,
            ListActionsLayout = AdminListActionsLayouts.Menu,
            AllowUserListLayoutSelection = true,
        });

        var options = Configure(new AdminListOptions { DefaultLayout = AdminListConstants.Grid }, site);

        Assert.Equal(AdminListConstants.Table, options.DefaultLayout);
        Assert.Equal(AdminListActionsLayouts.Menu, options.DefaultActionsLayout);
        Assert.True(options.AllowUserSelection);
    }

    [Fact]
    public void Configure_SiteWithoutChoice_KeepsTheConfiguredValues()
    {
        var options = Configure(
            new AdminListOptions { DefaultLayout = AdminListConstants.Grid, DefaultActionsLayout = AdminListActionsLayouts.Menu },
            new SiteSettings());

        Assert.Equal(AdminListConstants.Grid, options.DefaultLayout);
        Assert.Equal(AdminListActionsLayouts.Menu, options.DefaultActionsLayout);
        Assert.False(options.AllowUserSelection);
    }

    [Fact]
    public void Configure_BlankConfiguredValues_FallBackToTheShippedLayouts()
    {
        var options = Configure(new AdminListOptions { DefaultLayout = "  ", DefaultActionsLayout = "" }, new SiteSettings());

        Assert.Equal(AdminListConstants.List, options.DefaultLayout);
        Assert.Equal(AdminListActionsLayouts.Buttons, options.DefaultActionsLayout);
    }

    [Fact]
    public void Configure_PaddedValues_AreTrimmed()
    {
        var site = new SiteSettings();
        site.Put(new AdminSettings { ListActionsLayout = " Menu " });

        var options = Configure(new AdminListOptions { DefaultLayout = " Table " }, site);

        Assert.Equal(AdminListConstants.Table, options.DefaultLayout);
        Assert.Equal(AdminListActionsLayouts.Menu, options.DefaultActionsLayout);
    }

    private static AdminListOptions Configure(AdminListOptions options, ISite site)
    {
        var siteService = new Mock<ISiteService>();
        siteService.Setup(s => s.GetSiteSettingsAsync()).ReturnsAsync(site);

        new AdminListOptionsConfiguration(siteService.Object).Configure(options);

        return options;
    }
}
