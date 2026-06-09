# Rate Limits (`OrchardCore.RateLimits`)

The Rate Limits module centralizes ASP.NET Core request rate limiting for Orchard Core tenants.

When the feature is enabled, it can:

- apply tenant-specific published rate-limit policies;
- let administrators keep draft changes without reloading the tenant;
- publish one or many policies with a single tenant reload;
- keep feature-contributed named-route limits active as read-only built-in limits.

Static assets are intentionally excluded because Orchard registers static-file middleware before `UseRateLimiter()`. Scripts, stylesheets, and images therefore do not consume request budgets.

## Tenant policies

The module adds **Tools** -> **Rate Limits**, where administrators can manage tenant policies.

Each policy has:

- a **draft** version used for editing;
- an optional **published** version enforced at runtime;
- one target type: **Global**, **Route**, or **Endpoint**;
- an optional description;
- owner and author metadata;
- one or more limiters.

Saving edits updates only the draft. Publishing copies the draft into the published version and reloads the tenant shell once.

The admin UI includes:

- policy search on the index page;
- inline **Publish** actions for policies that currently have a draft;
- a draft editor with **Save Draft** and **Save & Publish** actions;
- an accordion that shows the published version when a published snapshot exists;
- a warning that publishing restarts the website.

### Policy types

| Type | Matches |
|------|---------|
| `Global` | Every tenant request |
| `Route` | A named route from `IRouteNameMetadata` or `IEndpointNameMetadata` |
| `Endpoint` | Requests whose path starts with the configured path |

### Limiter types

The admin UI currently supports these limiter sources:

- `FixedWindow`
- `SlidingWindow`
- `Concurrency`
- `TokenBucket`

Multiple limiters can be added to the same policy. When a published policy matches a request, every limiter on that policy is applied.

## Built-in route limits

Features can continue to contribute named-route limits in code through `RateLimitsOptions`. These built-in limits stay active and are shown in the admin UI as read-only entries.

```csharp
services.Configure<RateLimitsOptions>(options =>
{
    options.AddRouteRateLimit(
        routeName: "Login",
        httpMethod: HttpMethods.Post,
        partitioner: RateLimitPartitionHelpers.CreateSlidingWindowPerIpPolicy(
            policyName: "password-authentication",
            permitLimit: 10));
});
```

Built-in route limits are matched by:

- route name; and
- optional HTTP method.

Matching by both values prevents `GET` and `POST` actions that share the same route name from unintentionally sharing the same limiter.

## Seeding the default global policy

When the feature runs with this policy-based model for the first time, Orchard seeds a published **Default Global Policy** with a fixed-window limiter using these defaults:

- permit limit: `150`
- window seconds: `60`
- queue limit: `0`

After the seed runs, tenant administrators can edit, publish, or replace that policy in the UI.

## Recipes and deployment plans

Rate limit policies can also be moved between tenants with recipes and deployment plans.

- The recipe step name is `CreateOrUpdateRateLimitPolicies`.
- The deployment UI exposes **All Rate Limit Policies**, which exports every stored policy entry.
- Setup-style recipe exports stamp policy owner and author values with the tenant admin parameters.

The initial default global policy is seeded through a migration recipe stored under the module's `Migrations` folder.

## Route names

Route policies can target:

- MVC route names from `IRouteNameMetadata`; and
- endpoint names from `IEndpointNameMetadata`, such as minimal API routes defined with `.WithName(...)`.

The admin editor lists the route names currently exposed by enabled features in the tenant, and route policies include a **Select a route** option for an empty initial selection.

## Security considerations

Rate limiting is a defensive control that helps reduce abuse on high-risk endpoints such as:

- username/password sign-in;
- registration;
- password reset;
- token issuance;
- two-factor code submission or delivery.

This makes automated credential stuffing, brute-force attempts, and repetitive token or code requests more expensive for an attacker.

Rate limiting is not a replacement for authentication, authorization, captcha, lockout, or monitoring. Use it together with the other security features that Orchard Core and ASP.NET Core provide.
