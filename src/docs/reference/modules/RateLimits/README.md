# Rate Limits (`OrchardCore.RateLimits`)

The Rate Limits module centralizes ASP.NET Core request rate limiting for Orchard Core tenants.

When the feature is enabled, it can:

- apply tenant-specific enabled rate-limit policies;
- let administrators edit disabled policies without reloading the tenant;
- enable or disable one or many policies with a single tenant reload;
- keep feature-contributed named-route and endpoint-group limits active as read-only built-in limits.

Static assets are intentionally excluded because Orchard registers static-file middleware before `UseRateLimiter()`. Scripts, stylesheets, and images therefore do not consume request budgets.

## Tenant policies

The module adds **Tools** -> **Rate Limits**, where administrators can manage tenant policies.

Each policy has:

- a single editable document;
- an **enabled** flag that determines whether it is enforced at runtime;
- one target type: **Global**, **Endpoint**, or **Group**;
- an optional description;
- owner and author metadata;
- one or more limiters.

Saving a disabled policy updates it without reloading the tenant. Enabled policies remain editable only for the policy name and description, and saving those metadata changes does not reload the tenant. Enabling a disabled policy, disabling an enabled policy, or deleting an enabled policy reloads the tenant shell once so the active rate-limit configuration is refreshed.

The admin UI includes:

- policy search on the index page;
- inline **Enable** and **Disable** actions;
- an editor with a single **Save** action;
- a warning when an enabled policy cannot be modified until it is disabled or replaced;
- bulk enable, disable, and delete actions.

### Policy types

| Type | Matches | Good for |
|------|---------|----------|
| `Global` | Every tenant request | Applying a baseline budget to all dynamic requests |
| `Endpoint` | Requests whose path starts with the configured path | Protecting login, token, registration, API, or webhook paths |
| `Group` | Endpoints assigned to the configured rate-limit group | Reusing the same policy across related routes without coupling to their paths |

### Endpoint matching

Endpoint policies use a request path prefix. For example:

- `/api` matches `/api`, `/api/users`, and `/api/orders/42`;
- `/users/login` matches `/users/login` and `/users/login/reset`;
- `/graphql` is a good fit when you want a single policy for all GraphQL traffic.

Endpoint policies must start with `/`.

### Group matching

Group policies match endpoint metadata instead of request paths.

Use `RateLimitGroupAttribute` on MVC actions or controllers:

```csharp
[RateLimitGroup("authentication", "public-api")]
public async Task<IActionResult> Login(LoginViewModel model)
{
    // ...
}
```

For minimal APIs or other endpoint builders, use `WithRateLimitGroup()` or `WithRateLimitGroups()`:

```csharp
endpoints.MapPost("/api/token", HandleTokenAsync)
    .WithName("Access.Token")
    .WithRateLimitGroups("authentication", "public-api");
```

An endpoint can belong to one or more groups. A group policy does nothing when no enabled policy targets that group, so the metadata is safe to add before the Rate Limits feature is enabled or before a tenant policy exists.

## Limiter sources

Each policy can contain one or more limiter sources. When an enabled policy matches a request, every limiter on that policy is applied.

In the admin UI, **Available Limiter Types** shows a short description for each limiter and links directly to the matching section of this document.

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

#### Fixed window parameters

| Setting | What it means | Practical guidance |
|---|---|---|
| `Permit limit` | How many requests are allowed during one full window. | If you set this to `10`, the 11th matching request in the same window is delayed or rejected. |
| `Window seconds` | How long one window lasts. | `60` means "per minute", `300` means "per 5 minutes". |
| `Queue limit` | How many extra requests may wait after the limit is reached. | Use `0` when you want extra requests rejected immediately instead of waiting. |

!!! tip
    Fixed window is usually the easiest limiter for non-technical administrators to reason about because the rule reads naturally: "allow X requests every Y seconds."

### Sliding window

Use **Sliding window** when you want the same kind of rate cap as a fixed window, but with smoother enforcement near the boundary between windows.

Example uses:

- login or verification endpoints where burstiness around the reset boundary is undesirable;
- public APIs where you want less "spiky" throttling behavior.

Example behavior:

- instead of resetting all at once, the window is split into segments;
- request volume expires gradually as old segments fall out of the rolling window.

Choose **Sliding window** when a fixed window feels too abrupt.

#### Sliding window parameters

| Setting | What it means | Practical guidance |
|---|---|---|
| `Permit limit` | How many requests are allowed across the full rolling window. | If this is `10`, Orchard looks at the full window and limits once those 10 requests have been used. |
| `Window seconds` | How far back Orchard looks when counting requests. | `60` means "look at the last minute of traffic." |
| `Segments per window` | How many smaller slices exist inside the full window. | More segments make recovery smoother, but `4` to `10` is usually enough for most cases. |
| `Queue limit` | How many extra requests may wait after the limit is reached. | Use `0` if you prefer an immediate rejection instead of making requests wait. |

!!! note
    `Segments per window` does not increase the limit. It only controls how gradually old requests fall out of the rolling window.

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

#### Concurrency parameters

| Setting | What it means | Practical guidance |
|---|---|---|
| `Permit limit` | How many matching requests can run at the same time. | If you set this to `3`, a 4th request must wait or be rejected until one of the running requests finishes. |
| `Queue limit` | How many extra requests may wait for a free slot. | Use `0` when you want requests above the live limit to fail immediately. |
| `Queue processing order` | Which waiting request gets the next free slot first. | **Oldest first** is the fairest default. **Newest first** can make sense when only the latest request is still useful. |

!!! warning
    Concurrency does **not** mean "requests per minute." It means "requests running right now."

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

#### Token bucket parameters

| Setting | What it means | Practical guidance |
|---|---|---|
| `Token limit` | The maximum number of tokens the bucket can hold. | This is the largest burst a client can use at once. |
| `Tokens per period` | How many tokens are added back each refill cycle. | If this is `5`, the bucket regains up to 5 requests' worth of capacity each cycle. |
| `Replenishment seconds` | How often Orchard refills the bucket. | `10` means tokens are added every 10 seconds. |
| `Queue limit` | How many extra requests may wait when the bucket is empty. | Use `0` to reject extra requests immediately instead of waiting for refills. |
| `Queue processing order` | Which waiting request gets the next token first. | **Oldest first** is the safest default; **Newest first** favors fresh requests. |

#### Reading a token bucket as plain English

If you configure:

- `Token limit = 20`
- `Tokens per period = 5`
- `Replenishment seconds = 10`

then the rule is roughly:

> A client can spend up to 20 requests at once, and then regains up to 5 more requests every 10 seconds.

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

Features can continue to contribute named-route and group-based limits in code through `RateLimitsOptions`. These built-in limits stay active and are shown in the admin UI as read-only entries.

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

```csharp
services.Configure<RateLimitsOptions>(options =>
{
    options.AddGroupRateLimit(
        groupName: "authentication",
        partitioner: RateLimitPartitionHelpers.CreateSlidingWindowPerIpPolicy(
            policyName: "shared-authentication",
            permitLimit: 10));
});
```

Built-in route limits are matched by:

- route name; and
- optional HTTP method.

Matching by both values prevents `GET` and `POST` actions that share the same route name from unintentionally sharing the same limiter.

Built-in group limits are matched by:

- endpoint group name.

If an endpoint belongs to multiple groups, every matching built-in group limit and every matching enabled tenant group policy is combined with the rest of the active limiters for that request.

## Seeding the default global policy

When the feature runs with this policy-based model for the first time, Orchard seeds an enabled **Default Global Policy** with a fixed-window limiter using these defaults:

- permit limit: `150`
- window seconds: `60`
- queue limit: `0`

After the seed runs, tenant administrators can edit, enable, disable, or replace that policy in the UI.

## Recipes and deployment plans

Rate limit policies can also be moved between tenants with recipes and deployment plans.

- The recipe step name is `CreateOrUpdateRateLimitPolicies`.
- Recipe payloads use a `policies` array.
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
