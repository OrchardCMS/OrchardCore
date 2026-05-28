using Fluid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Email.Smtp.Services;

public sealed class DefaultSmtpOptionsConfiguration : IPostConfigureOptions<DefaultSmtpOptions>
{
    private readonly FluidParser _fluidParser;
    private readonly ShellOptions _shellOptions;
    private readonly ShellSettings _shellSettings;
    private readonly ILogger _logger;

    public DefaultSmtpOptionsConfiguration(
        FluidParser fluidParser,
        IOptions<ShellOptions> shellOptions,
        ShellSettings shellSettings,
        ILogger<DefaultSmtpOptionsConfiguration> logger)
    {
        _fluidParser = fluidParser;
        _shellOptions = shellOptions.Value;
        _shellSettings = shellSettings;
        _logger = logger;
    }

    public void PostConfigure(string name, DefaultSmtpOptions options)
    {
        if (options.DeliveryMethod == SmtpDeliveryMethod.SpecifiedPickupDirectory)
        {
            try
            {
                SmtpPickupDirectoryResolver.ConfigurePickupDirectory(
                    options,
                    options.PickupDirectoryLocationBase,
                    _fluidParser,
                    _shellOptions,
                    _shellSettings);
            }
            catch (Exception e)
            {
                options.PickupDirectoryLocationBase = null;
                options.PickupDirectoryLocation = null;
                _logger.LogCritical(e, "Unable to resolve default SMTP pickup directory location.");
            }
        }

        options.IsEnabled = options.PickupDirectoryLocation is not null && options.ConfigurationExists();
    }
}
