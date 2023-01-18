using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.GitHub.Services;
using OrchardCore.GitHub.Settings;

namespace OrchardCore.GitHub.Configuration;

public class GitHubAuthenticationSettingsConfiguration : IConfigureOptions<GitHubAuthenticationSettings>
{
    private readonly IGitHubAuthenticationService _gitHubAuthenticationService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger _logger;

    public GitHubAuthenticationSettingsConfiguration(
        IGitHubAuthenticationService gitHubAuthenticationService,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<GitHubAuthenticationSettingsConfiguration> logger)
    {
        _gitHubAuthenticationService = gitHubAuthenticationService;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
    }

    public void Configure(GitHubAuthenticationSettings options)
    {
        var settings = _gitHubAuthenticationService
            .GetSettingsAsync()
            .GetAwaiter()
            .GetResult();

        options.CallbackPath = settings.CallbackPath;
        options.ClientID = settings.ClientID;
        options.SaveTokens = settings.SaveTokens;

        if (!String.IsNullOrWhiteSpace(settings.ClientSecret))
        {
            try
            {
                var protector = _dataProtectionProvider.CreateProtector(nameof(GitHubAuthenticationSettingsConfiguration));

                options.ClientSecret = protector.Unprotect(settings.ClientSecret);
            }
            catch
            {
                _logger.LogError("The GitHub app secret could not be decrypted. It may have been encrypted using a different key.");
            }
        }
    }
}
