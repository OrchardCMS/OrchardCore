using Fluid;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.Email;
using OrchardCore.Email.Smtp.Services;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;

namespace OrchardCore.Tests.Email;

public class SmtpOptionsConfigurationTests
{
    [Theory]
    [InlineData("~/Emails")]
    [InlineData("~\\Emails")]
    [InlineData("~/MyEmails/Etc")]
    [InlineData("~/Sites/{{ ShellSettings.Name }}/Emails")]
    public void Configure_ResolvesPickupDirectoryLocationFromAppDataRoot(string pickupDirectoryLocation)
    {
        var appDataPath = Path.Combine("C:\\App", "App_Data");
        var shellSettings = new ShellSettings
        {
            Name = "Default",
        };
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

        var shellOptions = Options.Create(new ShellOptions
        {
            ShellsApplicationDataPath = appDataPath,
        });

        var sut = new SmtpOptionsConfiguration(
            siteService.Object,
            new FluidParser(),
            shellOptions,
            shellSettings,
            new EphemeralDataProtectionProvider(),
            NullLogger<SmtpOptionsConfiguration>.Instance);

        var options = new SmtpOptions();

        sut.Configure(options);

        var expectedPath = pickupDirectoryLocation switch
        {
            "~\\Emails" => Path.Combine(appDataPath, "Emails"),
            "~/MyEmails/Etc" => Path.Combine(appDataPath, "MyEmails", "Etc"),
            "~/Sites/{{ ShellSettings.Name }}/Emails" => Path.Combine(appDataPath, "Sites", shellSettings.Name, "Emails"),
            _ => Path.Combine(appDataPath, "Emails"),
        };

        Assert.Equal(expectedPath, options.PickupDirectoryLocation);
    }
}
