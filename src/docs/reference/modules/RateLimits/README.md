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
- one target type: **Global** or **Endpoint**;
- an optional description;
- owner and author metadata;
- one or more limiters.

Saving **Save Draft** updates only the draft. **Save & Publish** updates the draft, publishes it immediately, clears the draft snapshot, and reloads the tenant shell once. Publishing a draft from the policy list also clears the draft after it becomes the published version.

The admin UI includes:

- policy search on the index page;
- inline **Publish Draft** actions for policies that currently have a draft;
- a draft editor with **Save Draft** and **Save & Publish** actions;
- a published comparison section when a published snapshot exists;
- a warning that publishing restarts the tenant shell.

### Policy types

| Type | Matches | Good for |
|------|---------|----------|
| `Global` | Every tenant request | Applying a baseline budget to all dynamic requests |
| `Endpoint` | Requests whose path starts with the configured path | Protecting login, token, registration, API, or webhook paths |

### Endpoint matching

Endpoint policies use a request path prefix. For example:

- `/api` matches `/api`, `/api/users`, and `/api/orders/42`;
- `/users/login` matches `/users/login` and `/users/login/reset`;
- `/graphql` is a good fit when you want a single policy for all GraphQL traffic.

Endpoint policies must start with `/`.

## Limiter sources

Each policy can contain one or more limiter sources. When a published policy matches a request, every limiter on that policy is applied.

## Choosing a limiter source

The four limiter sources solve different problems:

| Limiter source | Best for | Think of it as |
|---|---|---|
| `FixedWindow` | Simple per-period caps | "Only X requests every Y seconds" |
| `SlidingWindow` | Smoother throttling without sharp window boundaries | "X requests across a rolling time span" |
| `Concurrency` | Limiting in-flight work | "Only X requests can run at the same time" |
| `TokenBucket` | Burst-friendly APIs with steady refill | "A bucket that fills over time and is spent by requests" |

### Fixed window

Use **Fixed window** when you want a straightforward hard cap for a known time period.

Example uses:

- allow `5` password reset submissions every `60` seconds;
- allow `20` login attempts every `5` minutes;
- apply a simple tenant-wide cap such as `150` requests every `60` seconds.

Example behavior:

- if the limit is `10 requests / 60 seconds`, the counter resets at the next window boundary;
- clients can use the full limit again as soon as the next window starts.

Choose **Fixed window** when simplicity matters more than smoothing.

### Sliding window

Use **Sliding window** when you want the same kind of rate cap as a fixed window, but with smoother enforcement near the boundary between windows.

Example uses:

- login or verification endpoints where burstiness around the reset boundary is undesirable;
- public APIs where you want less "spiky" throttling behavior.

Example behavior:

- instead of resetting all at once, the window is split into segments;
- request volume expires gradually as old segments fall out of the rolling window.

Choose **Sliding window** when a fixed window feels too abrupt.

### Concurrency

Use **Concurrency** when the real concern is not how many requests arrive over time, but how many expensive requests are running at once.

Example uses:

- export, report, or image-processing endpoints;
- expensive search endpoints;
- operations that call slow downstream services.

Example behavior:

- if the permit limit is `3`, only `3` matching requests can execute concurrently;
- the rest are queued or rejected depending on the queue settings.

Choose **Concurrency** when you need to protect server capacity or downstream dependencies from long-running requests.

### Token bucket

Use **Token bucket** when you want to allow short bursts while still enforcing a sustainable average rate over time.

Example uses:

- API clients that naturally send brief bursts;
- webhook receivers that can tolerate short spikes but need a steady long-term cap;
- interactive experiences where occasional bursts should not be blocked immediately.

Example behavior:

- the bucket might hold `20` tokens;
- each request consumes a token;
- tokens are replenished over time, for example `5` tokens every `10` seconds.

Choose **Token bucket** when burst tolerance matters.

!!! tip
    If you are unsure, start with **Fixed window**. Move to **Sliding window** when boundary effects matter, to **Concurrency** when work duration is the real bottleneck, and to **Token bucket** when you need burst tolerance.

## Example policies

### Protect a login endpoint

Use an endpoint policy that matches `/Login` or your custom sign-in path, then attach a fixed-window or sliding-window limiter.

Typical choice:

- **Fixed window** for a simple cap;
- **Sliding window** if you want smoother throttling across the full period.

### Protect an API area

Use an endpoint policy for `/api` and attach:

- **Token bucket** if short bursts are acceptable;
- **Concurrency** if each request is expensive;
- optionally a second limiter when you want both burst control and concurrency control.

### Add a tenant-wide baseline

Use a **Global** policy with a fixed-window limiter so every dynamic tenant request shares the same baseline budget.

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
- Recipe payloads use `draftPolicies` and `publishedPolicies` arrays.
- The deployment UI exposes **All Rate Limit Policies**, which exports every stored policy.
- Setup-style recipe exports stamp policy owner and author values with the tenant admin parameters.

The initial default global policy is seeded through a migration recipe stored under the module's `Migrations` folder.

## Security considerations

Rate limiting is a defensive control that helps reduce abuse on high-risk endpoints such as:

- username/password sign-in;
- registration;
- password reset;
- token issuance;
- two-factor code submission or delivery;
- public APIs and webhooks.

This makes automated credential stuffing, brute-force attempts, and repetitive token or code requests more expensive for an attacker.

Rate limiting is not a replacement for authentication, authorization, captcha, lockout, or monitoring. Use it together with the other security features that Orchard Core and ASP.NET Core provide.
