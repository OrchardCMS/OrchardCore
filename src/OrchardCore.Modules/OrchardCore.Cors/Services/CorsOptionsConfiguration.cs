using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Cors.Settings;

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

        string? firstAddedPolicyName = null;

        foreach (var corsPolicy in corsSettings.Policies)
        {
            var allowAnyOrigin = CorsSettingsHelper.IsAnyOriginAllowed(corsPolicy.AllowAnyOrigin, corsPolicy.AllowedOrigins);

            if (corsPolicy.AllowCredentials && allowAnyOrigin)
            {
                _logger.LogWarning(
                    "Using AllowCredentials with any origin (including '*') is considered a security risk, the {PolicyName} policy will not be loaded.",
                    corsPolicy.Name);
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
                    configurePolicy.WithHeaders(corsPolicy.AllowedHeaders);
                }

                if (corsPolicy.AllowAnyMethod)
                {
                    configurePolicy.AllowAnyMethod();
                }
                else
                {
                    configurePolicy.WithMethods(corsPolicy.AllowedMethods);
                }

                if (allowAnyOrigin)
                {
                    configurePolicy.AllowAnyOrigin();
                }
                else
                {
                    configurePolicy.WithOrigins(corsPolicy.AllowedOrigins);
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

            firstAddedPolicyName ??= corsPolicy.Name;

            if (corsPolicy.IsDefaultPolicy)
            {
                options.DefaultPolicyName = corsPolicy.Name;
            }
        }

        // Only fall back to the first *successfully added* policy, never a skipped/invalid one.
        options.DefaultPolicyName ??= firstAddedPolicyName;
    }
}
