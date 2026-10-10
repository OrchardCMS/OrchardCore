using Fluid;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Secrets;
using OrchardCore.Settings;

namespace OrchardCore.Email.Smtp.Services;

public sealed class SmtpOptionsConfiguration : IConfigureOptions<SmtpOptions>
{
    public const string ProtectorName = "SmtpSettingsConfiguration";
    public const string PasswordSecretName = "Smtp.Password";
    private const string _pickupDirectoryLocationBaseKey = nameof(SmtpOptions.PickupDirectoryLocationBase);

    private readonly ISiteService _siteService;
    private readonly FluidParser _fluidParser;
    private readonly IShellConfiguration _shellConfiguration;
    private readonly ShellOptions _shellOptions;
    private readonly ShellSettings _shellSettings;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    public SmtpOptionsConfiguration(
        ISiteService siteService,
        FluidParser fluidParser,
        IShellConfiguration shellConfiguration,
        IOptions<ShellOptions> shellOptions,
        ShellSettings shellSettings,
        IDataProtectionProvider dataProtectionProvider,
        IServiceProvider serviceProvider,
        ILogger<SmtpOptionsConfiguration> logger)
    {
        _siteService = siteService;
        _fluidParser = fluidParser;
        _shellConfiguration = shellConfiguration;
        _shellOptions = shellOptions.Value;
        _shellSettings = shellSettings;
        _dataProtectionProvider = dataProtectionProvider;
        _serviceProvider = serviceProvider;
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
        options.IgnoreInvalidSslCertificate = settings.IgnoreInvalidSslCertificate;

        options.Password = _serviceProvider.GetSecretValueAsync(
            settings.PasswordSecretName,
            settings.Password,
            _dataProtectionProvider.CreateProtector(ProtectorName),
            _logger)
            .GetAwaiter()
            .GetResult();

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

    // The 'OrchardCore_Email_Smtp' and 'OrchardCore_Email' sections are deprecated and will be removed in a future major version, use 'Email:Smtp' instead.
    private string GetConfiguredPickupDirectoryLocationBase()
        => _shellConfiguration.GetSectionCompat("Email:Smtp", "OrchardCore_Email_Smtp", "OrchardCore_Email")[_pickupDirectoryLocationBaseKey];
}
