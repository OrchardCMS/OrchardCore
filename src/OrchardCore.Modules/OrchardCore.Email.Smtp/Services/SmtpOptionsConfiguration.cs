using Fluid;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;

namespace OrchardCore.Email.Smtp.Services;

public sealed class SmtpOptionsConfiguration : IConfigureOptions<SmtpOptions>
{
    public const string ProtectorName = "SmtpSettingsConfiguration";

    private readonly ISiteService _siteService;
    private readonly FluidParser _fluidParser;
    private readonly ShellOptions _shellOptions;
    private readonly ShellSettings _shellSettings;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger _logger;

    public SmtpOptionsConfiguration(
        ISiteService siteService,
        FluidParser fluidParser,
        IOptions<ShellOptions> shellOptions,
        ShellSettings shellSettings,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<SmtpOptionsConfiguration> logger)
    {
        _siteService = siteService;
        _fluidParser = fluidParser;
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
        options.PickupDirectoryLocation = ResolvePickupDirectoryLocation(settings.PickupDirectoryLocation);
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

        options.IsEnabled = settings.IsEnabled ?? options.ConfigurationExists();
    }

    private string ResolvePickupDirectoryLocation(string pickupDirectoryLocation)
    {
        if (string.IsNullOrWhiteSpace(pickupDirectoryLocation))
        {
            return pickupDirectoryLocation;
        }

        try
        {
            pickupDirectoryLocation = ParseAndFormat(pickupDirectoryLocation);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Unable to parse SMTP pickup directory location.");

            return null;
        }

        if (!pickupDirectoryLocation.StartsWith("~/", StringComparison.Ordinal) &&
            !pickupDirectoryLocation.StartsWith("~\\", StringComparison.Ordinal))
        {
            return pickupDirectoryLocation;
        }

        var relativePath = pickupDirectoryLocation[2..]
            .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

        return string.IsNullOrEmpty(relativePath)
            ? _shellOptions.ShellsApplicationDataPath
            : Path.Combine(_shellOptions.ShellsApplicationDataPath, relativePath);
    }

    private string ParseAndFormat(string template)
    {
        var templateOptions = new TemplateOptions();
        templateOptions.MemberAccessStrategy.Register<ShellSettings>();

        if (!_fluidParser.TryParse(template, out var parsedTemplate, out var errors))
        {
            throw new InvalidOperationException($"Failed to parse SMTP pickup directory location: {string.Join(System.Environment.NewLine, errors)}");
        }

        var templateContext = new TemplateContext(templateOptions);
        templateContext.SetValue("ShellSettings", _shellSettings);

        return parsedTemplate.Render(templateContext, NullEncoder.Default)
            .ReplaceLineEndings(string.Empty)
            .Trim();
    }
}
