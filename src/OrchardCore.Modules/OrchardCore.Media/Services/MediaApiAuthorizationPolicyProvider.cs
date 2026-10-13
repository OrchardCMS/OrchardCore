using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Settings;

namespace OrchardCore.Media.Services;

/// <summary>
/// Decorates the <see cref="IAuthorizationPolicyProvider"/> to build the "MediaApi" policy from the current
/// <see cref="MediaApiSettings"/>: it requires an authenticated user against exactly one scheme, the admin cookie
/// by default, or the bearer "Api" scheme when configured. The other policies are provided by the decorated provider.
/// </summary>
/// <remarks>
/// As the policy depends on the site settings, the authorization middleware must not cache the policies it combines
/// per endpoint, the cache being only invalidated when the endpoints change. This cache can't be disabled per policy.
/// </remarks>
internal sealed class MediaApiAuthorizationPolicyProvider : IAuthorizationPolicyProvider
{
    private static readonly AuthorizationPolicy s_cookiePolicy = new AuthorizationPolicyBuilder(IdentityConstants.ApplicationScheme)
        .RequireAuthenticatedUser()
        .Build();

    private static readonly AuthorizationPolicy s_bearerPolicy = new AuthorizationPolicyBuilder(OrchardCoreConstants.AuthenticationSchemes.Api)
        .RequireAuthenticatedUser()
        .Build();

    private readonly IAuthorizationPolicyProvider _innerProvider;
    private readonly ISiteService _siteService;

    public MediaApiAuthorizationPolicyProvider(IAuthorizationPolicyProvider innerProvider, ISiteService siteService)
    {
        _innerProvider = innerProvider;
        _siteService = siteService;
    }

    public bool AllowsCachingPolicies => false;

    public async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
    {
        if (string.Equals(policyName, MediaApiConstants.AuthorizationPolicyName, StringComparison.OrdinalIgnoreCase))
        {
            var settings = await _siteService.GetSettingsAsync<MediaApiSettings>();

            return settings.AuthenticationScheme == MediaApiAuthenticationScheme.Bearer
                ? s_bearerPolicy
                : s_cookiePolicy;
        }

        return await _innerProvider.GetPolicyAsync(policyName);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        => _innerProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy> GetFallbackPolicyAsync()
        => _innerProvider.GetFallbackPolicyAsync();

    /// <summary>
    /// Decorates the registered <see cref="IAuthorizationPolicyProvider"/>, or the default one if none is registered.
    /// </summary>
    public static void Decorate(IServiceCollection services)
    {
        var descriptor = services.LastOrDefault(descriptor => descriptor.ServiceType == typeof(IAuthorizationPolicyProvider) && !descriptor.IsKeyedService);

        var lifetime = descriptor?.Lifetime ?? ServiceLifetime.Transient;

        Func<IServiceProvider, IAuthorizationPolicyProvider> createInnerProvider = descriptor switch
        {
            null => serviceProvider => ActivatorUtilities.CreateInstance<DefaultAuthorizationPolicyProvider>(serviceProvider),
            { ImplementationInstance: IAuthorizationPolicyProvider instance } => _ => instance,
            { ImplementationFactory: not null } => serviceProvider => (IAuthorizationPolicyProvider)descriptor.ImplementationFactory(serviceProvider),
            _ => serviceProvider => (IAuthorizationPolicyProvider)ActivatorUtilities.CreateInstance(serviceProvider, descriptor.ImplementationType),
        };

        if (descriptor is not null)
        {
            services.Remove(descriptor);
        }

        services.Add(ServiceDescriptor.Describe(
            typeof(IAuthorizationPolicyProvider),
            serviceProvider => new MediaApiAuthorizationPolicyProvider(
                createInnerProvider(serviceProvider),
                serviceProvider.GetRequiredService<ISiteService>()),
            lifetime));
    }
}
