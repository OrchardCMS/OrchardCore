using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.SignalR.Azure;

internal static class AzureSignalROptionsExtension
{
    private const string ConfigurationSection = "SignalR:Azure";

    public static IEnumerable<ValidationResult> Validate(this AzureSignalROptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            yield return new ValidationResult($"'{ConfigurationSection}:ConnectionString' is not configured.");
        }
    }

    public static bool TryBindConfiguration(this IShellConfiguration shellConfiguration, out AzureSignalROptions options)
    {
        options = new AzureSignalROptions();

        var section = shellConfiguration.GetSection(ConfigurationSection);

        options.ConnectionString = section.GetValue<string>(nameof(options.ConnectionString));
        options.ApplicationName = section.GetValue<string>(nameof(options.ApplicationName));

        return true;
    }
}
