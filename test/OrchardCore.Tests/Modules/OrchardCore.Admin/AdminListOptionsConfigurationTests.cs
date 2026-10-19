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
            ListActionsLayout = AdminListActionsLayout.Menu,
            AllowUserListLayoutSelection = true,
        });

        var options = Configure(new AdminListOptions { DefaultLayout = AdminListLayout.List }, site);

        Assert.Equal(AdminListLayout.Grid, options.DefaultLayout);
        Assert.Equal(AdminListActionsLayout.Menu, options.DefaultActionsLayout);
        Assert.True(options.AllowUserSelection);
    }

    [Fact]
    public void Configure_SiteWithoutChoice_KeepsTheConfiguredValues()
    {
        var options = Configure(
            new AdminListOptions { DefaultLayout = AdminListLayout.Grid, DefaultActionsLayout = AdminListActionsLayout.Menu },
            new SiteSettings());

        Assert.Equal(AdminListLayout.Grid, options.DefaultLayout);
        Assert.Equal(AdminListActionsLayout.Menu, options.DefaultActionsLayout);
        Assert.False(options.AllowUserSelection);
    }

    [Fact]
    public void Configure_NoActionsChoice_DefaultsToButtons()
    {
        var options = Configure(new AdminListOptions(), new SiteSettings());

        Assert.Equal(AdminListLayout.List, options.DefaultLayout);
        Assert.Equal(AdminListActionsLayout.Buttons, options.DefaultActionsLayout);
    }

    [Fact]
    public void Configure_ButtonsOverride_WinsOverConfiguredMenu()
    {
        var site = new SiteSettings();
        site.Put(new AdminSettings { ListActionsLayout = AdminListActionsLayout.Buttons });

        var options = Configure(new AdminListOptions
        {
            DefaultLayout = AdminListLayout.Grid,
            DefaultActionsLayout = AdminListActionsLayout.Menu,
        }, site);

        Assert.Equal(AdminListLayout.Grid, options.DefaultLayout);
        Assert.Equal(AdminListActionsLayout.Buttons, options.DefaultActionsLayout);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Configure_UndefinedActionsLayout_ThrowsValidationError(bool siteOverride)
    {
        var site = new SiteSettings();
        var options = new AdminListOptions();
        if (siteOverride)
        {
            site.Put(new AdminSettings { ListActionsLayout = (AdminListActionsLayout)42 });
        }
        else
        {
            options.DefaultActionsLayout = (AdminListActionsLayout)42;
        }

        var error = Assert.Throws<OptionsValidationException>(() => Configure(options, site));

        Assert.Contains("DefaultActionsLayout must be Buttons or Menu.", error.Failures);
    }

    [Theory]
    [InlineData("Buttons", AdminListActionsLayout.Buttons)]
    [InlineData("menu", AdminListActionsLayout.Menu)]
    public void Bind_ActionsLayoutName_UsesEnum(string value, AdminListActionsLayout expected)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["DefaultActionsLayout"] = value })
            .Build();
        var options = new AdminListOptions();

        configuration.Bind(options);

        Assert.Equal(expected, options.DefaultActionsLayout);
    }

    [Fact]
    public void Bind_UnknownActionsLayout_Throws()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["DefaultActionsLayout"] = "Icons" })
            .Build();

        Assert.Throws<InvalidOperationException>(() => configuration.Bind(new AdminListOptions()));
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
