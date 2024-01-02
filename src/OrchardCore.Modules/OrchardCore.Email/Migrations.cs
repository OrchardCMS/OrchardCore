using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using OrchardCore.Data.Migration;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Secrets;
using OrchardCore.Secrets.Models;
using OrchardCore.Settings;

namespace OrchardCore.Email;

public class Migrations : DataMigration
{
    private readonly ISiteService _siteService;
    private readonly ISecretService _secretService;
    private readonly ShellDescriptor _shellDescriptor;
    private readonly IDataProtector _dataProtector;
    private readonly ILogger _logger;

    public Migrations(
        ISiteService siteService,
        ISecretService secretService,
        ShellDescriptor shellDescriptor,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<Migrations> logger)
    {
        _siteService = siteService;
        _secretService = secretService;
        _shellDescriptor = shellDescriptor;

        _dataProtector = dataProtectionProvider.CreateProtector("OrchardCore.Email").ToTimeLimitedDataProtector();

        _logger = logger;
    }

    // New installations don't need to be upgraded, but because there is no initial migration record,
    // 'UpgradeAsync' is called in a new 'CreateAsync' but only if the feature was already installed.
    public async Task<int> CreateAsync()
    {
        if (_shellDescriptor.WasFeatureAlreadyInstalled("OrchardCore.Email"))
        {
            await UpgradeAsync();
        }

        // Shortcut other migration steps on new content definition schemas.
        return 1;
    }

    // Upgrade an existing installation.
#pragma warning disable CS0618 // Type or member is obsolete
    private async Task UpgradeAsync()
    {
        var settings = (await _siteService.GetSiteSettingsAsync()).As<SmtpSettings>();
        if (settings?.Password is null)
        {
            return;
        }

        await _secretService.AddSecretAsync<TextSecret>(
            EmailSecret.Password,
            (secret, info) =>
            {
                secret.Text = settings.Password;
                info.Description = "Email Secret holding a Password.";
            });

        var siteSettings = await _siteService.LoadSiteSettingsAsync();
        siteSettings.Alter<SmtpSettings>(nameof(SmtpSettings), settings =>
        {
            settings.Password = null;
        });

    }
#pragma warning restore CS0618 // Type or member is obsolete
}
