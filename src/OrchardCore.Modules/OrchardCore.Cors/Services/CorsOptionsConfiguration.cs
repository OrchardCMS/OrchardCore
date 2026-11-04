using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrchardCore.Cors.Services;

public sealed class CorsOptionsConfiguration : IConfigureOptions<CorsOptions>
{
    private readonly CorsService _corsService;
    private readonly ILogger _logger;

    public CorsOptionsConfiguration(CorsService corsService, ILogger<CorsOptionsConfiguration> logger)
    {
        _corsService = corsService;
        _logger = logger;
    }

    public void Configure(CorsOptions options)
    {
        var corsSettings = _corsService.GetSettingsAsync().GetAwaiter().GetResult();
        if (corsSettings?.Policies == null || !corsSettings.Policies.Any())
        {
            return;
        }

        var names = new HashSet<string>(StringComparer.Ordinal);
        string firstPolicy = null;
        string defaultPolicy = null;
        foreach (var corsPolicy in corsSettings.Policies)
        {
            if (_corsService.ValidatePolicy(corsPolicy).Count > 0 || !names.Add(corsPolicy.Name))
            {
                _logger.LogWarning("The invalid or duplicate CORS policy {PolicyName} will not be loaded.", corsPolicy?.Name);
                continue;
            }

            options.AddPolicy(corsPolicy.Name, configurePolicy =>
            {
                if (corsPolicy.AllowAnyHeader)
                {
                    configurePolicy.AllowAnyHeader();
                }
                else
                {
                    configurePolicy.WithHeaders(corsPolicy.AllowedHeaders ?? []);
                }

                if (corsPolicy.AllowAnyMethod)
                {
                    configurePolicy.AllowAnyMethod();
                }
                else
                {
                    configurePolicy.WithMethods(corsPolicy.AllowedMethods ?? []);
                }

                if (corsPolicy.AllowAnyOrigin)
                {
                    configurePolicy.AllowAnyOrigin();
                }
                else
                {
                    configurePolicy.WithOrigins(corsPolicy.AllowedOrigins ?? []);
                }

                if (corsPolicy.AllowCredentials)
                {
                    configurePolicy.AllowCredentials();
                }
                else
                {
                    configurePolicy.DisallowCredentials();
                }

                if (corsPolicy.ExposedHeaders?.Length > 0)
                {
                    configurePolicy.WithExposedHeaders(corsPolicy.ExposedHeaders);
                }
            });

            firstPolicy ??= corsPolicy.Name;
            if (corsPolicy.IsDefaultPolicy)
            {
                defaultPolicy ??= corsPolicy.Name;
            }
        }

        if (firstPolicy is not null)
        {
            options.DefaultPolicyName = defaultPolicy ?? firstPolicy;
        }
    }
}
