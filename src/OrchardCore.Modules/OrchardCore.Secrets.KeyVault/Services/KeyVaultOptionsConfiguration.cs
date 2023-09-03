using System;
using Fluid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Secrets.KeyVault.Models;

namespace OrchardCore.Secrets.KeyVault.Services;

public class KeyVaultOptionsConfiguration : IConfigureOptions<SecretsKeyVaultOptions>
{
    private readonly IShellConfiguration _shellConfiguration;
    private readonly ShellSettings _shellSettings;
    private readonly ILogger _logger;

    // Local instance since it can be discarded once the startup is over.
    private readonly FluidParser _fluidParser = new();

    public KeyVaultOptionsConfiguration(
        IShellConfiguration shellConfiguration,
        ShellSettings shellSettings,
        ILogger<KeyVaultOptionsConfiguration> logger)
    {
        _shellConfiguration = shellConfiguration;
        _shellSettings = shellSettings;
        _logger = logger;
    }

    public void Configure(SecretsKeyVaultOptions options)
    {
        var section = _shellConfiguration.GetSection("OrchardCore_Secrets_KeyVault");

        options.KeyVaultName = section.GetValue(nameof(options.KeyVaultName), String.Empty);
        options.Prefix = section.GetValue(nameof(options.Prefix), String.Empty);

        var templateOptions = new TemplateOptions();
        var templateContext = new TemplateContext(templateOptions);
        templateOptions.MemberAccessStrategy.Register<ShellSettings>();
        templateOptions.MemberAccessStrategy.Register<SecretsKeyVaultOptions>();
        templateContext.SetValue("ShellSettings", _shellSettings);

        ParseKeyVaultName(options, templateContext);
        ParsePrefix(options, templateContext);
    }

    private void ParseKeyVaultName(SecretsKeyVaultOptions options, TemplateContext templateContext)
    {
        // Use Fluid directly as this is transient and cannot invoke _liquidTemplateManager.
        try
        {
            var template = _fluidParser.Parse(options.KeyVaultName);

            // Container name must be lowercase.
            options.KeyVaultName = template.Render(templateContext, NullEncoder.Default).ToLower();
            options.KeyVaultName = options.KeyVaultName.Replace("\r", String.Empty).Replace("\n", String.Empty);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Unable to parse Azure KeyVault Secrets KeyVaultName.");
            throw;
        }
    }

    private void ParsePrefix(SecretsKeyVaultOptions options, TemplateContext templateContext)
    {
        try
        {
            var template = _fluidParser.Parse(options.Prefix);
            options.Prefix = template.Render(templateContext, NullEncoder.Default);
            options.Prefix = options.Prefix.Replace("\r", String.Empty).Replace("\n", String.Empty);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Unable to parse Azure KeyVault Secrets KeyVault Prefix.");
            throw;
        }
    }
}
