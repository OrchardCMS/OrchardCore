# Rate Limits (`OrchardCore.RateLimits`)

The Rate Limits module centralizes ASP.NET Core request rate limiting for Orchard Core tenants.

When the feature is enabled, it can:

- apply a global per-IP rate limit to tenant requests;
- apply stricter limits to specific named routes;
- let administrators disable the global limiter from site settings while keeping route-specific limits;
- let individual Orchard Core features contribute their own route-specific limits through `RateLimitsOptions`.

Static assets are intentionally excluded from the global limiter because Orchard registers static-file middleware before `UseRateLimiter()`. This keeps page assets such as scripts, stylesheets, and images from consuming request budgets.

## What it does

`OrchardCore.RateLimits` configures `Microsoft.AspNetCore.RateLimiting` once for the tenant and builds a chained global limiter with two stages:

1. A route-specific limiter that checks the current endpoint's route name and optional HTTP method.
2. A global fixed-window limiter that applies to all remaining tenant requests by remote IP address.

If no route-specific rule matches, the request falls through to the global limiter.

## Site settings

The module also adds a **Settings** -> **Security** -> **Rate Limits** page with an **Enable the global rate limiter** toggle.

- Enabled by default, which preserves the existing behavior.
- When disabled, Orchard Core stops applying the tenant-wide fixed-window limiter and keeps only route-specific limits.
- Saving a change requests a tenant shell reload so the middleware is rebuilt with the updated setting.

## Global configuration

The global limiter is configured from the `RateLimits:Global` section under the Orchard Core configuration root:

```json
{
  "OrchardCore": {
    "RateLimits": {
      "Global": {
        "PermitLimit": 150,
        "Window": "00:01:00",
        "QueueLimit": 0
      }
    }
  }
}
```

### Defaults

| Property | Default | Description |
|----------|---------|-------------|
| `PermitLimit` | `150` | Maximum requests allowed during each window for a single remote IP address. |
| `Window` | `00:01:00` | The fixed-window duration used by the global limiter. |
| `QueueLimit` | `0` | Number of queued requests allowed after the limit is reached. |

### Environment variables

The same settings can be overridden with environment variables:

```text
OrchardCore__RateLimits__Global__PermitLimit=150
OrchardCore__RateLimits__Global__Window=00:01:00
OrchardCore__RateLimits__Global__QueueLimit=0
```

As with other Orchard Core configuration, tenant-specific overrides may also be provided through shell configuration sources. See the [Configuration](../Configuration/README.md) documentation for details.

## Route-specific limits

Features can contribute route-specific limits by configuring `RateLimitsOptions`.

```csharp
using Microsoft.AspNetCore.Http;
using OrchardCore.Modules;
using OrchardCore.RateLimits;

[RequireFeatures("OrchardCore.Users", "OrchardCore.RateLimits")]
public sealed class RateLimitsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<RateLimitsOptions>(options =>
        {
            options.AddRouteRateLimit(
                routeName: "Login",
                httpMethod: HttpMethods.Post,
                partitioner: RateLimitPartitionHelpers.CreateSlidingWindowPerIpPolicy(
                    policyName: "password-authentication",
                    permitLimit: 10));
        });
    }
}
```

### Route matching

Route-specific limits are matched by:

- route name; and
- optional HTTP method.

Matching by both values prevents `GET` and `POST` actions that share the same route name from unintentionally sharing the same limiter.

The module supports both:

- MVC route names from `IRouteNameMetadata`; and
- endpoint names from `IEndpointNameMetadata`, such as minimal API routes using `.WithName(...)`.

## Security considerations

Rate limiting is a defensive control that helps reduce abuse, especially on high-risk endpoints such as:

- username/password sign-in;
- registration;
- password reset;
- token issuance;
- two-factor authentication code submission or delivery.

This improves security by making automated credential stuffing, brute-force attempts, and repetitive token or code requests more expensive for an attacker.

Rate limiting is not a replacement for authentication, authorization, captcha, lockout, or monitoring. It should be used together with the other security features that Orchard Core and ASP.NET Core provide.

## Orchard Core integration

The module is designed so that Orchard Core features can add their own limits without directly registering rate-limiter middleware. For example, the Users and OpenID features contribute limits for named routes such as login, registration, password reset, two-factor flows, and token endpoints when `OrchardCore.RateLimits` is enabled.

This keeps rate-limit policy registration centralized while still allowing features to protect their own endpoints.
