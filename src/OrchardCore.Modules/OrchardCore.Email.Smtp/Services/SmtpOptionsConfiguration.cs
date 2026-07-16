using Fluid;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Settings;

namespace OrchardCore.Email.Smtp.Services;

public sealed class SmtpOptionsConfiguration : IConfigureOptions<SmtpOptions>
{
    public const string ProtectorName = "SmtpSettingsConfiguration";
    private const string _pickupDirectoryLocationBaseKey = nameof(SmtpOptions.PickupDirectoryLocationBase);

    private readonly ISiteService _siteService;
    private readonly FluidParser _fluidParser;
    private readonly IShellConfiguration _shellConfiguration;
    private readonly ShellOptions _shellOptions;
    private readonly ShellSettings _shellSettings;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger _logger;

    public SmtpOptionsConfiguration(
        ISiteService siteService,
        FluidParser fluidParser,
        IShellConfiguration shellConfiguration,
        IOptions<ShellOptions> shellOptions,
        ShellSettings shellSettings,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<SmtpOptionsConfiguration> logger)
    {
        _siteService = siteService;
        _fluidParser = fluidParser;
        _shellConfiguration = shellConfiguration;
        _shellOptions = shellOptions.Value;
        _shellSettings = shellSettings;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
    }

    public void Configure(SmtpOptions options)
    {
        var settings = _siteService.GetSettings<SmtpSettings>();

        options.DefaultSender = settings.DefaultSender;
        options.DeliveryMethod = settings.DeliveryMethod;
        options.PickupDirectoryLocation = settings.PickupDirectoryLocation;
        options.Host = settings.Host;
        options.Port = settings.Port;
        options.ProxyHost = settings.ProxyHost;
        options.ProxyPort = settings.ProxyPort;
        options.EncryptionMethod = settings.EncryptionMethod;
        options.AutoSelectEncryption = settings.AutoSelectEncryption;
        options.RequireCredentials = settings.RequireCredentials;
        options.UseDefaultCredentials = settings.UseDefaultCredentials;
        options.UserName = settings.UserName;
        options.Password = settings.Password;
        options.IgnoreInvalidSslCertificate = settings.IgnoreInvalidSslCertificate;

        if (!string.IsNullOrWhiteSpace(settings.Password))
        {
            try
            {
                var protector = _dataProtectionProvider.CreateProtector(ProtectorName);
                options.Password = protector.Unprotect(settings.Password);
            }
            catch
            {
                _logger.LogError("The Smtp password could not be decrypted. It may have been encrypted using a different key.");
            }
        }

        if (settings.DeliveryMethod == SmtpDeliveryMethod.SpecifiedPickupDirectory)
        {
            ConfigurePickupDirectory(options, settings.PickupDirectoryLocation);
        }

        options.IsEnabled = settings.IsEnabled ?? (options.PickupDirectoryLocation is not null && options.ConfigurationExists());
    }

    private void ConfigurePickupDirectory(SmtpOptions options, string pickupDirectoryLocation)
    {
        try
        {
            options.PickupDirectoryLocation = pickupDirectoryLocation;

            SmtpPickupDirectoryResolver.ConfigurePickupDirectory(
                options,
                GetConfiguredPickupDirectoryLocationBase(),
                _fluidParser,
                _shellOptions,
                _shellSettings);
        }
        catch (Exception e)
        {
            options.PickupDirectoryLocationBase = null;
            options.PickupDirectoryLocation = null;
            _logger.LogCritical(e, "Unable to resolve SMTP pickup directory location.");
        }
    }

    private string GetConfiguredPickupDirectoryLocationBase()
        => _shellConfiguration.GetSection("OrchardCore_Email_Smtp")[_pickupDirectoryLocationBaseKey]
            ?? _shellConfiguration.GetSection("OrchardCore_Email")[_pickupDirectoryLocationBaseKey];
}
