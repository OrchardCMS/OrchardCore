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
            ListLayout = AdminListLayout.Grid,
            ListActionsLayout = AdminListActionsLayouts.Menu,
            AllowUserListLayoutSelection = true,
        });

        var options = Configure(new AdminListOptions { DefaultLayout = AdminListLayout.List }, site);

        Assert.Equal(AdminListLayout.Grid, options.DefaultLayout);
        Assert.Equal(AdminListActionsLayouts.Menu, options.DefaultActionsLayout);
        Assert.True(options.AllowUserSelection);
    }

    [Fact]
    public void Configure_SiteWithoutChoice_KeepsTheConfiguredValues()
    {
        var options = Configure(
            new AdminListOptions { DefaultLayout = AdminListLayout.Grid, DefaultActionsLayout = AdminListActionsLayouts.Menu },
            new SiteSettings());

        Assert.Equal(AdminListLayout.Grid, options.DefaultLayout);
        Assert.Equal(AdminListActionsLayouts.Menu, options.DefaultActionsLayout);
        Assert.False(options.AllowUserSelection);
    }

    [Fact]
    public void Configure_BlankActionsLayout_FallsBackToButtons()
    {
        var options = Configure(new AdminListOptions { DefaultActionsLayout = "" }, new SiteSettings());

        Assert.Equal(AdminListLayout.List, options.DefaultLayout);
        Assert.Equal(AdminListActionsLayouts.Buttons, options.DefaultActionsLayout);
    }

    [Fact]
    public void Configure_PaddedValues_AreTrimmed()
    {
        var site = new SiteSettings();
        site.Put(new AdminSettings { ListActionsLayout = " Menu " });

        var options = Configure(new AdminListOptions { DefaultLayout = AdminListLayout.Grid }, site);

        Assert.Equal(AdminListLayout.Grid, options.DefaultLayout);
        Assert.Equal(AdminListActionsLayouts.Menu, options.DefaultActionsLayout);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Configure_UndefinedLayout_ThrowsValidationError(bool siteOverride)
    {
        var site = new SiteSettings();
        var options = new AdminListOptions();
        if (siteOverride)
        {
            site.Put(new AdminSettings { ListLayout = (AdminListLayout)42 });
        }
        else
        {
            options.DefaultLayout = (AdminListLayout)42;
        }

        Assert.Throws<OptionsValidationException>(() => Configure(options, site));
    }

    private static AdminListOptions Configure(AdminListOptions options, ISite site)
    {
        var siteService = new Mock<ISiteService>();
        siteService.Setup(s => s.GetSiteSettingsAsync()).ReturnsAsync(site);

        new AdminListOptionsConfiguration(siteService.Object).Configure(options);

        return options;
    }
}
