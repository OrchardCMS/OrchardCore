using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Liquid.Abstractions;

namespace OrchardCore.FileStorage.AzureBlob;

public abstract class BlobStorageOptionsConfiguration<TOptions> : IConfigureOptions<TOptions>
    where TOptions : BlobStorageOptions
{
    private readonly ShellSettings _sellSettings;
    private readonly ILogger _logger;

    public BlobStorageOptionsConfiguration(
        ShellSettings sellSettings,
        ILogger logger)
    {
        _sellSettings = sellSettings;
        _logger = logger;
    }

    protected abstract TOptions GetRawOptions();

    public void Configure(TOptions options)
    {
        var rawOptions = GetRawOptions();

        if (rawOptions == null || !rawOptions.IsConfigured())
        {
            return;
        }

        var parser = new FluidOptionsParser<TOptions>(_sellSettings);

        options.ConnectionString = rawOptions.ConnectionString;
        options.CredentialName = rawOptions.CredentialName;

        if (rawOptions.StorageAccountUri is null)
        {
            var accountName = GetAccountName(rawOptions.ConnectionString);

            if (string.IsNullOrEmpty(accountName))
            {
                throw new InvalidOperationException($"Unable to determine the storage account name for {typeof(TOptions).Name}.");
            }

            rawOptions.StorageAccountUri = new Uri($"https://{accountName}.blob.core.windows.net");
        }
        else
        {
            options.StorageAccountUri = rawOptions.StorageAccountUri;
        }

        if (!string.IsNullOrEmpty(rawOptions.ContainerName))
        {
            try
            {
                // Container name must be lowercase.
                options.ContainerName = parser.ParseAndFormat(rawOptions.ContainerName).ToLowerInvariant();
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Unable to parse container name for {OptionName}.", typeof(TOptions).Name);
                throw;
            }
        }

        if (!string.IsNullOrWhiteSpace(rawOptions.BasePath))
        {
            try
            {
                options.BasePath = parser.ParseAndFormat(rawOptions.BasePath);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Unable to parse base path for {OptionName}.", typeof(TOptions).Name);
                throw;
            }
        }

        FurtherConfigure(rawOptions, options);
    }

    /// <summary>
    /// Allows you to configure additional options in an inherited class.
    /// </summary>
    /// <param name="rawOptions">The options as returned by <see cref="GetRawOptions"/>.</param>
    /// <param name="options">The options to configure.</param>
    protected virtual void FurtherConfigure(TOptions rawOptions, TOptions options)
    {
    }

    private static string GetAccountName(string connectionString)
    {
        // Split by ';' and parse key=value pairs
        var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var part in parts)
        {
            var kv = part.Split('=', 2);

            if (kv.Length == 2)
            {
                dict[kv[0].Trim()] = kv[1].Trim();
            }
        }

        dict.TryGetValue("AccountName", out var accountName);

        return accountName;
    }
}
