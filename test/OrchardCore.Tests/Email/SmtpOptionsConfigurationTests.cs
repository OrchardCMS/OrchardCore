using Fluid;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.Email;
using OrchardCore.Email.Smtp.Services;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Settings;

namespace OrchardCore.Tests.Email;

public class SmtpOptionsConfigurationTests
{
    [Theory]
    [InlineData("/", null)]
    [InlineData("/Outbound", "Outbound")]
    [InlineData("Outbound", "Outbound")]
    [InlineData("Email/Outbound", "Email\\Outbound")]
    public void Configure_ResolvesPickupDirectoryLocationUnderDefaultTenantBase(string pickupDirectoryLocation, string expectedRelativePath)
    {
        var appDataPath = GetRootedPath("App", "App_Data");
        var shellSettings = new ShellSettings
        {
            Name = "Default",
        };
        var sut = CreateSut(
            appDataPath,
            shellSettings,
            pickupDirectoryLocation: pickupDirectoryLocation);

        var options = new SmtpOptions();

        sut.Configure(options);

        var expectedBasePath = Path.GetFullPath(Path.Combine(appDataPath, "Sites", shellSettings.Name, "Emails"));
        var expectedPath = string.IsNullOrEmpty(expectedRelativePath)
            ? expectedBasePath
            : Path.Combine(expectedBasePath, expectedRelativePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar));

        Assert.Equal(expectedBasePath, options.PickupDirectoryLocationBase);
        Assert.Equal(expectedPath, options.PickupDirectoryLocation);
    }

    [Fact]
    public void Configure_UsesConfiguredPickupDirectoryLocationBaseWithFluidTokens()
    {
        var appDataPath = GetRootedPath("App", "App_Data");
        var shellSettings = new ShellSettings
        {
            Name = "TenantA",
        };
        var sut = CreateSut(
            appDataPath,
            shellSettings,
            pickupDirectoryLocation: "/Outbound",
            shellConfigurationValues: new Dictionary<string, string>
            {
                ["OrchardCore_Email_Smtp:PickupDirectoryLocationBase"] = @"{{ AppData }}\Drops\{{ ShellSettings.Name }}",
            });

        var options = new SmtpOptions();

        sut.Configure(options);

        var expectedBasePath = Path.GetFullPath(Path.Combine(appDataPath, "Drops", shellSettings.Name));
        Assert.Equal(expectedBasePath, options.PickupDirectoryLocationBase);
        Assert.Equal(Path.Combine(expectedBasePath, "Outbound"), options.PickupDirectoryLocation);
    }

    [Theory]
    [InlineData("..\\..\\Shared")]
    [InlineData("~/Emails")]
    [InlineData(".\\Drafts")]
    [InlineData("Email\\..\\Shared")]
    [InlineData("C:\\Emails")]
    [InlineData("\\\\server\\share\\Emails")]
    public void Configure_DisablesPickupDirectoryOutsideConfiguredBase(string pickupDirectoryLocation)
    {
        var appDataPath = GetRootedPath("App", "App_Data");
        var shellSettings = new ShellSettings
        {
            Name = "Default",
        };
        var sut = CreateSut(
            appDataPath,
            shellSettings,
            pickupDirectoryLocation: pickupDirectoryLocation);

        var options = new SmtpOptions();

        sut.Configure(options);

        Assert.Null(options.PickupDirectoryLocationBase);
        Assert.Null(options.PickupDirectoryLocation);
        Assert.False(options.IsEnabled);
    }

    [Fact]
    public void PostConfigure_ResolvesDefaultPickupDirectoryLocationBase()
    {
        var appDataPath = GetRootedPath("App", "App_Data");
        var shellSettings = new ShellSettings
        {
            Name = "Default",
        };
        var sut = new DefaultSmtpOptionsConfiguration(
            new FluidParser(),
            Options.Create(new ShellOptions
            {
                ShellsApplicationDataPath = appDataPath,
            }),
            shellSettings,
            NullLogger<DefaultSmtpOptionsConfiguration>.Instance);
        var options = new DefaultSmtpOptions
        {
            DefaultSender = "admin@example.com",
            DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
        };

        sut.PostConfigure(Options.DefaultName, options);

        var expectedBasePath = Path.GetFullPath(Path.Combine(appDataPath, "Sites", shellSettings.Name, "Emails"));
        Assert.Equal(expectedBasePath, options.PickupDirectoryLocationBase);
        Assert.Equal(expectedBasePath, options.PickupDirectoryLocation);
        Assert.True(options.IsEnabled);
    }

    [Fact]
    public void PostConfigure_DisablesDefaultPickupDirectoryLocationOutsideConfiguredBase()
    {
        var appDataPath = GetRootedPath("App", "App_Data");
        var shellSettings = new ShellSettings
        {
            Name = "Default",
        };
        var sut = new DefaultSmtpOptionsConfiguration(
            new FluidParser(),
            Options.Create(new ShellOptions
            {
                ShellsApplicationDataPath = appDataPath,
            }),
            shellSettings,
            NullLogger<DefaultSmtpOptionsConfiguration>.Instance);
        var options = new DefaultSmtpOptions
        {
            DefaultSender = "admin@example.com",
            DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
            PickupDirectoryLocation = "..\\..\\Shared",
        };

        sut.PostConfigure(Options.DefaultName, options);

        Assert.Null(options.PickupDirectoryLocationBase);
        Assert.Null(options.PickupDirectoryLocation);
        Assert.False(options.IsEnabled);
    }

    [Fact]
    public void PostConfigure_UsesSlashRootForConfiguredPickupDirectoryLocation()
    {
        var appDataPath = GetRootedPath("App", "App_Data");
        var shellSettings = new ShellSettings
        {
            Name = "Default",
        };
        var sut = new DefaultSmtpOptionsConfiguration(
            new FluidParser(),
            Options.Create(new ShellOptions
            {
                ShellsApplicationDataPath = appDataPath,
            }),
            shellSettings,
            NullLogger<DefaultSmtpOptionsConfiguration>.Instance);
        var options = new DefaultSmtpOptions
        {
            DefaultSender = "admin@example.com",
            DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
            PickupDirectoryLocationBase = @"{{ AppData }}\Drops\{{ ShellSettings.Name }}",
            PickupDirectoryLocation = "/",
        };

        sut.PostConfigure(Options.DefaultName, options);

        var expectedBasePath = Path.GetFullPath(Path.Combine(appDataPath, "Drops", shellSettings.Name));
        Assert.Equal(expectedBasePath, options.PickupDirectoryLocationBase);
        Assert.Equal(expectedBasePath, options.PickupDirectoryLocation);
        Assert.True(options.IsEnabled);
    }

    private static SmtpOptionsConfiguration CreateSut(
        string appDataPath,
        ShellSettings shellSettings,
        string pickupDirectoryLocation,
        IDictionary<string, string> shellConfigurationValues = null)
    {
        var site = new SiteSettings();
        site.Put(new SmtpSettings
        {
            DefaultSender = "admin@example.com",
            DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
            PickupDirectoryLocation = pickupDirectoryLocation,
        });

        var siteService = new Mock<ISiteService>();
        siteService
            .Setup(x => x.GetSiteSettingsAsync())
            .ReturnsAsync(site);

        var shellConfiguration = new ShellConfiguration(new ConfigurationBuilder()
            .AddInMemoryCollection(shellConfigurationValues ?? new Dictionary<string, string>())
            .Build());

        return new SmtpOptionsConfiguration(
            siteService.Object,
            new FluidParser(),
            shellConfiguration,
            Options.Create(new ShellOptions
            {
                ShellsApplicationDataPath = appDataPath,
            }),
            shellSettings,
            new EphemeralDataProtectionProvider(),
            NullLogger<SmtpOptionsConfiguration>.Instance);
    }

    private static string GetRootedPath(params string[] segments)
        => Path.GetFullPath(Path.Combine(Path.DirectorySeparatorChar.ToString(), Path.Combine(segments)));
}
