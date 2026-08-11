using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.Options;
using OrchardCore.SignalR.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.SignalR;

public sealed class AuthorizationOptionsConfigurationTests
{
    [Fact]
    public void PostConfigure_WithDefaultAuthenticateScheme_AddsDefaultAndApiSchemes()
    {
        var options = new AuthorizationOptions();
        var configuration = CreateConfiguration(new AuthenticationOptions
        {
            DefaultAuthenticateScheme = "CustomDefault",
        });

        configuration.PostConfigure(Options.DefaultName, options);

        var policy = options.GetPolicy("SignalR");
        Assert.NotNull(policy);
        Assert.Equal(["CustomDefault", OrchardCoreConstants.AuthenticationSchemes.Api], policy.AuthenticationSchemes);
        Assert.Contains(policy.Requirements, requirement => requirement is DenyAnonymousAuthorizationRequirement);
    }

    [Fact]
    public void PostConfigure_WithoutDefaultAuthenticateScheme_FallsBackToDefaultScheme()
    {
        var options = new AuthorizationOptions();
        var configuration = CreateConfiguration(new AuthenticationOptions
        {
            DefaultScheme = "FallbackDefault",
        });

        configuration.PostConfigure(Options.DefaultName, options);

        var policy = options.GetPolicy("SignalR");
        Assert.NotNull(policy);
        Assert.Equal(["FallbackDefault", OrchardCoreConstants.AuthenticationSchemes.Api], policy.AuthenticationSchemes);
    }

    [Fact]
    public void PostConfigure_WhenDefaultAuthenticateSchemeIsApi_DoesNotDuplicateApiScheme()
    {
        var options = new AuthorizationOptions();
        var configuration = CreateConfiguration(new AuthenticationOptions
        {
            DefaultAuthenticateScheme = OrchardCoreConstants.AuthenticationSchemes.Api,
        });

        configuration.PostConfigure(Options.DefaultName, options);

        var policy = options.GetPolicy("SignalR");
        Assert.NotNull(policy);
        Assert.Equal([OrchardCoreConstants.AuthenticationSchemes.Api], policy.AuthenticationSchemes);
    }

    private static AuthorizationOptionsConfiguration CreateConfiguration(AuthenticationOptions authenticationOptions)
        => new(Options.Create(authenticationOptions));
}
