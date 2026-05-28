using Fluid;
using OrchardCore.Email;
using OrchardCore.Email.Smtp.Services;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Tests.Email;

public class SmtpPickupDirectoryResolverTests
{
    [Fact]
    public void GetPickupDirectoryLocationBase_UsesDefaultTemplate()
    {
        var appDataPath = GetRootedPath("App", "App_Data");
        var shellSettings = new ShellSettings
        {
            Name = "Default",
        };

        var result = SmtpPickupDirectoryResolver.GetPickupDirectoryLocationBase(
            configuredBasePath: null,
            new FluidParser(),
            new ShellOptions
            {
                ShellsApplicationDataPath = appDataPath,
            },
            shellSettings);

        Assert.Equal(Path.GetFullPath(Path.Combine(appDataPath, "Sites", shellSettings.Name, "Emails")), result);
    }

    [Fact]
    public void GetPickupDirectoryLocationBase_ExpandsFluidTokens()
    {
        var appDataPath = GetRootedPath("App", "App_Data");
        var shellSettings = new ShellSettings
        {
            Name = "TenantA",
        };

        var result = SmtpPickupDirectoryResolver.GetPickupDirectoryLocationBase(
            @"{{ AppData }}\Drops\{{ ShellSettings.Name }}",
            new FluidParser(),
            new ShellOptions
            {
                ShellsApplicationDataPath = appDataPath,
            },
            shellSettings);

        Assert.Equal(Path.GetFullPath(Path.Combine(appDataPath, "Drops", shellSettings.Name)), result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("/")]
    [InlineData("/Outbound")]
    [InlineData("Outbound")]
    [InlineData("Email/Outbound")]
    public void IsValidPickupDirectoryLocation_AllowsSupportedValues(string pickupDirectoryLocation)
    {
        Assert.True(SmtpPickupDirectoryResolver.IsValidPickupDirectoryLocation(pickupDirectoryLocation));
    }

    [Theory]
    [InlineData("~")]
    [InlineData("~/")]
    [InlineData("../")]
    [InlineData("..")]
    [InlineData("~/Emails")]
    [InlineData("~\\Emails")]
    [InlineData("../Emails")]
    [InlineData("..\\Emails")]
    [InlineData("./Emails")]
    [InlineData(".\\Emails")]
    [InlineData("Emails\\..\\Shared")]
    [InlineData("Emails/./Drafts")]
    [InlineData("C:\\Emails")]
    [InlineData("\\\\server\\share\\Emails")]
    [InlineData("{{ ShellSettings.Name }}\\Emails")]
    public void IsValidPickupDirectoryLocation_RejectsUnsafeValues(string pickupDirectoryLocation)
    {
        Assert.False(SmtpPickupDirectoryResolver.IsValidPickupDirectoryLocation(pickupDirectoryLocation));
    }

    [Fact]
    public void ConfigurePickupDirectory_AppliesBaseAndResolvedPath()
    {
        var appDataPath = GetRootedPath("App", "App_Data");
        var shellSettings = new ShellSettings
        {
            Name = "Default",
        };
        var options = new SmtpOptions
        {
            PickupDirectoryLocation = "/Outbound",
        };

        SmtpPickupDirectoryResolver.ConfigurePickupDirectory(
            options,
            @"{{ AppData }}\Drops\{{ ShellSettings.Name }}",
            new FluidParser(),
            new ShellOptions
            {
                ShellsApplicationDataPath = appDataPath,
            },
            shellSettings);

        var expectedBasePath = Path.GetFullPath(Path.Combine(appDataPath, "Drops", shellSettings.Name));
        Assert.Equal(expectedBasePath, options.PickupDirectoryLocationBase);
        Assert.Equal(Path.Combine(expectedBasePath, "Outbound"), options.PickupDirectoryLocation);
    }

    [Theory]
    [InlineData("/", null)]
    [InlineData("/Outbound", "Outbound")]
    [InlineData("Email/Outbound", "Email/Outbound")]
    public void ResolvePickupDirectoryLocation_ResolvesInsideBasePath(string pickupDirectoryLocation, string expectedRelativePath)
    {
        var basePath = GetRootedPath("App", "App_Data", "Sites", "Default", "Emails");

        var result = SmtpPickupDirectoryResolver.ResolvePickupDirectoryLocation(basePath, pickupDirectoryLocation);

        var expectedPath = string.IsNullOrEmpty(expectedRelativePath)
            ? basePath
            : Path.Combine(basePath, expectedRelativePath.Replace('/', Path.DirectorySeparatorChar));

        Assert.Equal(expectedPath, result);
    }

    [Theory]
    [InlineData("~/Emails")]
    [InlineData("../Shared")]
    [InlineData("./Drafts")]
    [InlineData("Emails/../Shared")]
    public void ResolvePickupDirectoryLocation_ThrowsForUnsafeValues(string pickupDirectoryLocation)
    {
        var basePath = GetRootedPath("App", "App_Data", "Sites", "Default", "Emails");

        Assert.Throws<InvalidOperationException>(() =>
            SmtpPickupDirectoryResolver.ResolvePickupDirectoryLocation(basePath, pickupDirectoryLocation));
    }

    private static string GetRootedPath(params string[] segments)
        => Path.GetFullPath(Path.Combine(Path.DirectorySeparatorChar.ToString(), Path.Combine(segments)));
}
