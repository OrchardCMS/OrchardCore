using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OrchardCore.Infrastructure.Security;

namespace OrchardCore.Tests.Modules.OrchardCore.SignalR;

public sealed class AuthorizeWithSchemesMiddlewareTests
{
    [Fact]
    public void TryGetAuthentication_WithAuthorizeWithSchemesOnHub_ReturnsHubTypeName()
    {
        // Arrange
        var context = CreateHubContext(new AuthorizeWithSchemesAttribute
        {
            AuthenticationSchemes = OrchardCoreConstants.AuthenticationSchemes.Api,
        });

        // Act
        var result = AuthorizeWithSchemesMiddleware.TryGetAuthentication(context, out var authentication, out var endpointDisplayName);

        // Assert
        Assert.True(result);
        Assert.Equal(OrchardCoreConstants.AuthenticationSchemes.Api, authentication.AuthenticationSchemes);
        Assert.Equal(typeof(TestHub).FullName, endpointDisplayName);
    }

    [Fact]
    public void TryGetAuthentication_WithAuthorizeWithSchemesOnRoute_ReturnsRouteDisplayName()
    {
        // Arrange
        var context = CreateRouteContext(new AuthorizeWithSchemesAttribute
        {
            AuthenticationSchemes = OrchardCoreConstants.AuthenticationSchemes.Api,
        });

        // Act
        var result = AuthorizeWithSchemesMiddleware.TryGetAuthentication(context, out var authentication, out var endpointDisplayName);

        // Assert
        Assert.True(result);
        Assert.Equal(OrchardCoreConstants.AuthenticationSchemes.Api, authentication.AuthenticationSchemes);
        Assert.Equal("Test route", endpointDisplayName);
    }

    [Fact]
    public async Task InvokeAsync_WithAccessToken_AuthenticatesConfiguredScheme()
    {
        // Arrange
        var context = CreateHubContext(new AuthorizeWithSchemesAttribute
        {
            AuthenticationSchemes = OrchardCoreConstants.AuthenticationSchemes.Api,
        });
        context.Request.QueryString = new QueryString("?access_token=test-token");

        // Act
        await CreateMiddleware().InvokeAsync(context);

        // Assert
        Assert.True(context.User.Identity?.IsAuthenticated);
        Assert.Equal(OrchardCoreConstants.AuthenticationSchemes.Api, context.User.Identity?.AuthenticationType);
    }

    [Fact]
    public async Task InvokeAsync_IncludeDefaultAuthenticateSchemeTrue_LeavesDefaultUserUntouched()
    {
        // Arrange
        var context = CreateHubContext(new AuthorizeWithSchemesAttribute
        {
            AuthenticationSchemes = OrchardCoreConstants.AuthenticationSchemes.Api,
            IncludeDefaultAuthenticateScheme = true,
        });
        context.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "admin")], "CustomDefault"));

        // Act
        await CreateMiddleware().InvokeAsync(context);

        // Assert
        Assert.True(context.User.Identity?.IsAuthenticated);
        Assert.Equal("CustomDefault", context.User.Identity?.AuthenticationType);
    }

    [Fact]
    public async Task InvokeAsync_IncludeDefaultAuthenticateSchemeFalse_ClearsDefaultUser()
    {
        // Arrange
        var context = CreateHubContext(new AuthorizeWithSchemesAttribute
        {
            AuthenticationSchemes = OrchardCoreConstants.AuthenticationSchemes.Api,
            IncludeDefaultAuthenticateScheme = false,
        });
        context.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "admin")], "CustomDefault"));

        // Act
        await CreateMiddleware().InvokeAsync(context);

        // Assert
        Assert.False(context.User.Identity?.IsAuthenticated);
    }

    [Fact]
    public async Task InvokeAsync_IncludeDefaultAuthenticateSchemeFalse_AccessTokenReplacesDefaultUser()
    {
        // Arrange
        var context = CreateHubContext(new AuthorizeWithSchemesAttribute
        {
            AuthenticationSchemes = OrchardCoreConstants.AuthenticationSchemes.Api,
            IncludeDefaultAuthenticateScheme = false,
        });
        context.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "admin")], "CustomDefault"));
        context.Request.QueryString = new QueryString("?access_token=test-token");

        // Act
        await CreateMiddleware().InvokeAsync(context);

        // Assert
        Assert.True(context.User.Identity?.IsAuthenticated);
        Assert.Equal(OrchardCoreConstants.AuthenticationSchemes.Api, context.User.Identity?.AuthenticationType);
    }

    [Fact]
    public async Task InvokeAsync_WithMultipleAuthenticationSchemes_TriesEachSchemeUntilSuccess()
    {
        // Arrange
        var context = CreateHubContext(new AuthorizeWithSchemesAttribute
        {
            AuthenticationSchemes = "MissingScheme, AnotherScheme",
            IncludeDefaultAuthenticateScheme = false,
        });
        context.Request.QueryString = new QueryString("?access_token=test-token");

        // Act
        await CreateMiddleware().InvokeAsync(context);

        // Assert
        Assert.True(context.User.Identity?.IsAuthenticated);
        Assert.Equal("AnotherScheme", context.User.Identity?.AuthenticationType);
    }

    [Fact]
    public async Task InvokeAsync_WithAccessTokenOnRoute_AuthenticatesConfiguredScheme()
    {
        // Arrange
        var context = CreateRouteContext(new AuthorizeWithSchemesAttribute
        {
            AuthenticationSchemes = OrchardCoreConstants.AuthenticationSchemes.Api,
            IncludeDefaultAuthenticateScheme = false,
        });
        context.Request.QueryString = new QueryString("?access_token=test-token");

        // Act
        await CreateMiddleware().InvokeAsync(context);

        // Assert
        Assert.True(context.User.Identity?.IsAuthenticated);
        Assert.Equal(OrchardCoreConstants.AuthenticationSchemes.Api, context.User.Identity?.AuthenticationType);
    }

    private static AuthorizeWithSchemesMiddleware CreateMiddleware()
        => new(_ => Task.CompletedTask, NullLogger<AuthorizeWithSchemesMiddleware>.Instance);

    private static DefaultHttpContext CreateHubContext(AuthorizeWithSchemesAttribute authentication = null)
    {
        var metadata = authentication is null
            ? new EndpointMetadataCollection(new HubMetadata(typeof(TestHub)))
            : new EndpointMetadataCollection(new HubMetadata(typeof(TestHub)), authentication);

        return CreateContext(authentication, metadata);
    }

    private static DefaultHttpContext CreateRouteContext(AuthorizeWithSchemesAttribute authentication = null)
    {
        var metadata = authentication is null
            ? new EndpointMetadataCollection()
            : new EndpointMetadataCollection(authentication);

        return CreateContext(authentication, metadata);
    }

    private static DefaultHttpContext CreateContext(AuthorizeWithSchemesAttribute authentication, EndpointMetadataCollection metadata)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var authenticationBuilder = services.AddAuthentication(options => options.DefaultAuthenticateScheme = "CustomDefault");

        foreach (var scheme in new[]
        {
            OrchardCoreConstants.AuthenticationSchemes.Api,
            "AnotherScheme",
            "CustomDefault",
        })
        {
            authenticationBuilder.AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(scheme, _ => { });
        }

        services.AddAuthorization(options =>
        {
            options.AddPolicy("HubPolicy", policy => policy.RequireAuthenticatedUser());
        });

        var context = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider(),
        };

        context.SetEndpoint(new Endpoint(
            _ => Task.CompletedTask,
            metadata,
            authentication is null ? "Anonymous endpoint" : metadata.GetMetadata<HubMetadata>() is null ? "Test route" : "Test hub"));

        return context;
    }

    private sealed class TestHub : Hub;

    private sealed class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Scheme.Name == "CustomDefault")
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            if (!Request.Headers.Authorization.ToString().StartsWith("Bearer ", StringComparison.Ordinal))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            if (Scheme.Name == "AnotherScheme" || Scheme.Name == OrchardCoreConstants.AuthenticationSchemes.Api)
            {
                var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "api-user")], Scheme.Name));

                return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
            }

            return Task.FromResult(AuthenticateResult.NoResult());
        }
    }
}
