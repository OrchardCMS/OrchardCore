# Media Gallery real-time via Server-Sent Events — implementation plan

Companion to [media-gallery-realtime-transport-plan.md](media-gallery-realtime-transport-plan.md) (the
investigation). This document is the concrete design for the recommended outcome: replace the SignalR
transport with first-party ASP.NET Core SSE, and fix the fan-out amplification that is the actual
scaling problem.

**Branch:** `skrypt/media-gallery-realtime-transport`.

---

## 1. Goals

1. **Scale.** Cut the per-event server cost from `N × (3 + F)` storage operations to `1` (§3).
2. **Shed dependencies.** Remove `@microsoft/signalr` from the client bundle, and
   `Microsoft.AspNetCore.SignalR.StackExchangeRedis` + `Microsoft.Azure.SignalR` from the module.
   Net new packages: **zero** — SSE is in the shared framework.
3. **Preserve behaviour — including Azure SignalR.** Every requirement in §1.1 of the investigation
   plan. Connection offload is kept by retaining SignalR as an opt-in transport rather than deleting it
   (§3.4.4).
4. **Keep it reversible.** Transport behind an abstraction, shipped as its own feature, so SSE and
   SignalR coexist indefinitely and either can be the active one.
5. **Build a reusable OrchardCore capability, not a media feature.** Real-time push that any module can
   publish through, with the Media Gallery as the reference consumer and `OrchardCore.Notifications` as
   the second (§4.3). Adoption elsewhere is the point — which means the abstraction must be proven by a
   second consumer before it ships, and the scalability claims must rest on measurements (§3.5, R7),
   since "more efficient than SignalR" is the reason other modules would adopt it.

**Non-goals:** Blazor, gRPC, WebTransport, changing the Media API's REST surface, changing TUS uploads.

### 1.1 Decisions settled

The sections below record the reasoning; this is the summary, so nothing has to be re-litigated.

| # | Decision | Where |
| --- | --- | --- |
| D1 | **SSE**, in-process, as the transport. gRPC and WebTransport rejected — WebTransport would lose even with universal support | §2, investigation plan §4.2.1 |
| D2 | **Fix the fan-out first** (payload-carrying events, topic scoping, coalescing) on SignalR as it stands; it ships alone and is the actual scaling fix | §3.2, Phase 1 |
| D3 | Real-time is a **shared OrchardCore capability**, Media Gallery is the reference consumer, Notifications the second | §4 |
| D4 | **One multiplexed stream per tab** with a subscription endpoint — not one stream per feature | §4.2 |
| D5 | Every transport is an **opt-in feature**; "no real-time" stays a supported state; SSE preferred when several are enabled | §7 |
| D6 | Backplane goes **direct to `IConnectionMultiplexer`**, not `IMessageBus` (nine catches, three unfixable from outside) | §5.4.1 |
| D7 | **SignalR retained** as an opt-in transport, not deleted, so Azure SignalR offload users are not stranded | §3.4.5, §9 |
| D8 | Optional Phase 7 offload hub: **Mercure**, deployed same-origin at `/.well-known/mercure`, subscriber JWTs validated via JWKS. Pushpin/GRIP and Azure Web PubSub documented as alternatives | §3.4.8b |
| D9 | **PKCE unchanged**; the hub credential is a derived, short-lived, topic-scoped token. Cookie mode remains a first-class peer and must not be weakened for a hub | §3.4.7, §3.4.8a |

Everything above is settled **except** where it depends on measurements that have not run yet — see
§12 and Phase 0.

---

## 2. The .NET SSE support timeline (verified locally)

Answering "since when is SSE fully supported in .NET?" — it arrived in two steps, and only became
end-to-end first-party in .NET 10:

| Stage | Version | What shipped |
| --- | --- | --- |
| Hand-rolled | ASP.NET Core 1.0+ (2016) | You could always write `text/event-stream` to `Response.Body` yourself. No framework support: you formatted `data:`/`id:`/`retry:` frames and managed flushing by hand. |
| **Client-side parsing** | **.NET 9** (Nov 2024) | `System.Net.ServerSentEvents` with `SseParser` / `SseParser<T>` — consume an SSE stream from `HttpClient`. Read-only; no server story. |
| **Server-side writing** | **.NET 10** (Nov 2025) | `SseFormatter.WriteAsync(...)` plus minimal-API results `TypedResults.ServerSentEvents(...)` / `Results.ServerSentEvents(...)`. This is the "fully supported" milestone. |

Verified against the ref packs installed on this machine
(`~/.dotnet/packs/Microsoft.AspNetCore.App.Ref/10.0.10`), so these are exact, not remembered:

```
Microsoft.AspNetCore.Http.TypedResults.ServerSentEvents<T>(IAsyncEnumerable<SseItem<T>>)
Microsoft.AspNetCore.Http.TypedResults.ServerSentEvents<T>(IAsyncEnumerable<T>, string eventType)
Microsoft.AspNetCore.Http.TypedResults.ServerSentEvents(IAsyncEnumerable<string>, string eventType)
System.Net.ServerSentEvents.SseItem<T>            // .Data, .EventType, .EventId, .ReconnectionInterval
System.Net.ServerSentEvents.SseFormatter.WriteAsync<T>(IAsyncEnumerable<SseItem<T>>, Stream, ...)
System.Net.ServerSentEvents.SseParser.Create<T>(Stream, SseItemParser<T>)
```

`System.Net.ServerSentEvents` lives in `Microsoft.NETCore.App` (the base runtime), and the `TypedResults`
overloads in `Microsoft.AspNetCore.App` — both already referenced by every OrchardCore module. The repo
targets `net10.0` ([TargetFrameworks.props](src/OrchardCore.Build/TargetFrameworks.props)), so this is
available today with no package reference.

Two practical consequences:

- The **standalone gallery and any .NET consumer** can read the same stream via `SseParser` — no proto,
  no generated client. That is the "typed contract for other clients" benefit gRPC was going to buy,
  for free.
- Because server support is new in .NET 10, there is **little community mileage on it**. Budget spike
  time for the two behaviours flagged "verify" in §5.3 (raw vs. JSON-quoted data) and §6.2
  (connection ceiling).

---

## 3. Scalability design — the core of this plan

### 3.1 The problem, quantified

Established by reading the code (investigation plan §3 will confirm empirically):

- Every media change calls `Clients.All` in
  [MediaSignalREventHandler](src/OrchardCore.Modules/OrchardCore.Media/Hubs/MediaSignalREventHandler.cs) —
  **all** connected clients, regardless of what folder they are viewing.
- Every client responds by calling `loadDirectoryFiles(..., force: true)`
  ([SignalR.ts:34](src/OrchardCore.Modules/OrchardCore.Media/Assets/media-gallery/src/services/SignalR.ts#L34)),
  which always issues `GET api/media/GetDirectoryContent` — **even on a cache hit**, since the cached
  branch still refreshes in the background
  ([FileLibraryManager.ts:350-370](src/OrchardCore.Modules/OrchardCore.Media/Assets/media-gallery/src/services/FileLibraryManager.ts#L350-L370)).
- Each such request costs, in [GetDirectoryContentEndpoint](src/OrchardCore.Modules/OrchardCore.Media/Endpoints/Api/GetDirectoryContentEndpoint.cs)
  + [MediaEndpointHelpers](src/OrchardCore.Modules/OrchardCore.Media/Endpoints/Api/MediaEndpointHelpers.cs):
  1 × `GetDirectoryInfoAsync` (non-root) + 1 × `GetDirectoriesAsync` + 1 × `GetFilesAsync` +
  **one `GetDirectoriesAsync` per subfolder** for the `HasChildren` probe
  ([MediaEndpointHelpers.cs:94-98](src/OrchardCore.Modules/OrchardCore.Media/Endpoints/Api/MediaEndpointHelpers.cs#L94-L98)).

So the cost of **one** file upload is:

> **`N × (3 + F)` file-store operations**, where `N` = connected clients and `F` = subfolders in the
> directory each is viewing.

With 50 admins connected and a 10-subfolder view: **650 storage operations per uploaded file**. A
200-file bulk upload: **130,000**. Against Azure Blob or S3 those are billed network round-trips, and
they arrive as a burst. This is almost certainly the reported scaling failure — and note it is caused by
`Clients.All` + blind refetch, **not** by SignalR.

### 3.1a Upstream PR #19660 changes this picture — read before Phase 1

[PR #19660](https://github.com/OrchardCMS/OrchardCore/pull/19660) (`gvkries/fix-media-permissions`, open
against `main`, mergeable, 16 files) lands folder-level authorization **and** does part of what §3.2
proposes. Verified by reading the branch:

**What it already fixes — lever 2 (topic scoping):**

- `MediaSignalREventHandler` now sends to `Clients.Group(...)`, **not `Clients.All`**. Group names derive
  from the **parent** folder of the changed path (a file at `/folder/img.jpg` notifies the `/folder`
  group) via `GetParentFolderPath()` + `MediaHub.GetFolderGroupName()`.
- `MediaHub` gains `SubscribePath` / `UnsubscribePath`, each checking
  `AuthorizeAsync(user, MediaPermissions.ManageMediaFolder, (object)path)` **before**
  `Groups.AddToGroupAsync` — so unauthorized users cannot subscribe to a folder's broadcasts.
- The client subscribes/unsubscribes on folder navigation and **re-subscribes on reconnect** (SignalR
  group membership does not survive reconnection).
- API endpoints (`GetDirectoryContent`, `GetDirectoryTree`, `GetFolders`, `GetAllMediaItems`) now check
  `ManageMediaFolder` per path and filter unauthorized folders out of results.

**What it does not fix — lever 1, the dominant cost.** The client still calls
`loadDirectoryFiles(selectedDirectory, true)` on every event; the payload is logged and discarded. The
full refetch remains.

**And the per-refetch cost went up.** Each `GetDirectoryContent` now performs authorization checks per
folder on top of the `3 + F` storage operations, and PR reviewers explicitly flagged the risk of
"authorization checks becoming a denial-of-service vector" on large hierarchies. So the post-merge cost
per event is roughly:

> **`N_folder × ((3 + F) storage ops + ~(F + 1) authorization checks)`**
>
> where `N_folder` is now only the clients *viewing that folder* — a large win — but each of them costs
> **more** than before.

**Net effect on this plan:**

| Item | Change |
| --- | --- |
| Lever 2 (topic scoping) | **Done upstream.** Remove from Phase 1 |
| Lever 1 (payload patching) | **Unchanged, and now more valuable** — it also avoids the new authorization work |
| Lever 3 (coalescing) | Unchanged, still ours |
| §10.7 Secure Media | **Partially** resolved — see §3.1b. Subscription authorization is settled (`ManageMediaFolder` per path, §5.7); content protection is a different feature entirely |
| §1.1 requirement 1 of the investigation plan ("server→client only") | **Obsolete** — the feature is now bidirectional by design, which is exactly what §4.2's subscription endpoint anticipated |
| Branch | **Decided: all work stays on `skrypt/media-gallery-realtime-transport`** (cut from `main` @ `5c60562e93`, before #19660). We do not branch from the PR. Merge `main` in once #19660 lands — see the conflict-minimisation rule below |

It also independently validates two of our design choices: per-folder topics, and the need for an
explicit subscribe/unsubscribe channel with re-subscription on reconnect.

### 3.1b #19660 hides folder *names*, it does not protect file *content*

Verified in the module, and worth stating because it changes how much the added cost is worth:

- **Listing/management** is guarded by `ManageMedia` / `ManageMediaFolder` — what #19660 extends.
- **Serving the files** is a different path entirely: [Startup.cs:270](src/OrchardCore.Modules/OrchardCore.Media/Startup.cs#L270)
  serves media through `app.UseStaticFiles(...)`, with **no authorization**, unless the separate opt-in
  feature **`OrchardCore.Media.Security` ("Secure Media")** is enabled — which adds
  `SecureMediaMiddleware` ([Startup.cs:411](src/OrchardCore.Modules/OrchardCore.Media/Startup.cs#L411))
  and a *different* permission family, `ViewMedia` / `ViewMediaContent_{folder}`
  ([SecureMediaPermissions.cs](src/OrchardCore.Modules/OrchardCore.Media/SecureMediaPermissions.cs)).

So on a tenant **without** Secure Media enabled, #19660 removes a folder from the admin listing while its
files stay downloadable to anyone who knows or guesses the URL. The two features defend different assets
— *metadata* versus *content* — and only the pair is a complete story. That is worth documenting for
operators, who will reasonably assume folder-level permissions imply folder-level protection.

**The cost objection is legitimate.** #19660 adds per-folder authorization work to every listing call,
and today every real-time event triggers a listing call **per connected client** — so the authorization
cost is multiplied by the same amplification factor as the storage cost (§3.1a). PR reviewers already
flagged a DoS risk on large hierarchies.

**Lever 1 is the mitigation, and this is the strongest argument for doing it first.** Once events carry
their payload and the client patches its store, the refetch disappears — and with it the repeated
authorization work. The `ManageMediaFolder` check then runs where it belongs: once at subscription time
(already in `MediaHub.SubscribePath`), and on genuine user-initiated navigation. The per-event
authorization multiplier goes to zero.

**How Secure Media actually works** (read from the code, since it bears on lever 1's payload):
[`SecureMediaMiddleware`](src/OrchardCore.Modules/OrchardCore.Media/Services/SecureMediaMiddleware.cs)
intercepts **every** request under `MediaOptions.AssetsRequestPath`, authenticates the caller (falling
back to the `"Api"` bearer scheme, so PKCE tokens work for direct file access), then authorizes
`MediaPermissions.ViewMedia` **with the requested path as the resource**, returning **404 — not 403 —**
on denial so existence isn't disclosed.
[`ViewMediaFolderAuthorizationHandler`](src/OrchardCore.Modules/OrchardCore.Media/Services/ViewMediaFolderAuthorizationHandler.cs)
then resolves what that means per path: root → `ViewRootMedia`; `_Users/…` → `ViewOwnMedia` vs
`ViewOthersMedia`; `mediafields/…` → resolves the **content item id** and checks
`Contents.CommonPermissions.ViewContent` on it; anything else → a **dynamic per-folder permission**
`ViewMediaContent_{folder}` assignable to roles. Two limits worth knowing: folder permissions exist only
at the **first tier** (`/photos`, not `/photos/2026`, which inherits), and the handler deliberately
`Fail()`s rather than abstains, so no other handler can grant what it denied.

**Consequence for lever 1:** the payload carries the file's public URL, and that URL still passes through
`SecureMediaMiddleware` — so adding it bypasses nothing.

**Correction to an earlier reading of this plan:** the two permission families are *not* independent when
Secure Media is on. [`ManageMediaFolderAuthorizationHandler`](src/OrchardCore.Modules/OrchardCore.Media/Services/ManageMediaFolderAuthorizationHandler.cs#L92-L97)
succeeds only if the management permission passes **and**, when
`IsSecureMediaEnabled()`, `ViewMedia` passes for the same path. So with Secure Media enabled, manage ⊆
view; with it disabled, they are unrelated and a manage-only user can see names for content they cannot
fetch.

**Conflict-minimisation rule for Phase 1.** We build on our own branch, so #19660 will land underneath
us and it rewrites the two files lever 1 must also touch — `MediaSignalREventHandler.cs` (`Clients.All`
→ `Clients.Group`) and `SignalR.ts` (adds subscribe/unsubscribe). To keep the eventual merge cheap:

- Put the payload-building logic in a **new** file (e.g. `Realtime/MediaChangeEventFactory.cs`) and the
  client-side patching in a **new** module (e.g. `services/applyMediaChange.ts`).
- Leave the contested files with the smallest possible edit — ideally one call into the new code — so
  the merge resolves to a couple of lines instead of a whole-file reconciliation.
- Don't pre-implement #19660's grouping on our branch; take it from upstream at merge time.

### 3.1c Bounding the DoS risk in #19660 — where the cost actually is

Raised in review and in the team meeting: per-folder authorization could become a denial-of-service
vector, and caching folder permissions "doesn't look possible". Reading both handlers shows the cost is
very unevenly distributed, and most of it can be removed **without** caching.

**Cost of one `ManageMediaFolder` check, Secure Media OFF.** Pure string work — recompute the path
separator, normalize two folder constants, read the user's asset-folder name — then classify the path
into one of **four static permissions** (`ManageMedia`, `ManageAttachedMediaFieldsFolder`,
`ManageOwnMedia`, `ManageOthersMedia`) by prefix test, and one resource-less `AuthorizeAsync`. No I/O.
`F + 1` of these per listing is wasteful but not dangerous.

**Cost with Secure Media ON — this is the vector.** Every `ManageMediaFolder` check then also runs
`AuthorizeAsync(user, ViewMedia, path)`, and
[`ViewMediaFolderAuthorizationHandler`](src/OrchardCore.Modules/OrchardCore.Media/Services/ViewMediaFolderAuthorizationHandler.cs#L76-L89)
performs **`GetDirectoryInfoAsync(folderPath)`** — a storage round-trip, i.e. a network call on Azure
Blob/S3 — and sometimes `GetFileInfoAsync` as well; for `mediafields/{contentItemId}` paths it loads the
**content item from the database** to check `ViewContent`. So a single directory listing becomes
`F + 1` storage stats (plus possible DB reads) *on top of* the `3 + F` listings the endpoint already
does — and, before lever 1, multiplied again by every connected client on each media event.

**Four mitigations, in order of value. Only the last one needs a cache.**

1. **Hoist the zone classification (no cache required).** `ManageMediaFolder` maps a path to one of only
   four *static* permissions, decided by prefix. Evaluate those four `AuthorizeAsync` calls **once per
   request**, then classify each folder by prefix with zero further authorization calls. `F + 1` → **≤ 4**.
   This is a pure refactor with identical semantics — the strongest candidate to propose on the PR.
2. **Pass a "known to be a directory" hint (removes the I/O).** The `GetDirectoryInfoAsync` in the view
   handler exists only to disambiguate "new directory vs file in root" — a question the *listing* code
   has already answered, since it is enumerating known directories. An overload accepting that hint skips
   the storage round-trip entirely. This removes the dangerous part of the Secure Media path without
   touching authorization semantics.
3. **Short-circuit on `ManageMedia` (global).** A user holding the global permission for a zone needs no
   per-folder evaluation in that zone at all. Covers the common administrator case in one check.
4. **Cache the `ViewMedia` decision by *first-tier folder*, not by full path.** This is why caching looks
   impossible but partly is not: the view handler derives `folderPath` as the path **up to the first
   separator**, so for `a/b/c` the generic decision depends only on `a`. Within one listing the distinct
   key count is usually **1**. Cache `(userId + roles version, firstTierFolder) → bool` with a short
   absolute expiry plus an `ISignal` token — exactly the pattern
   [`SecureMediaPermissions`](src/OrchardCore.Modules/OrchardCore.Media/SecureMediaPermissions.cs#L55-L66)
   already uses (5-minute expiry + `_signal.GetToken(...)`).
   **Exclude the two zones that genuinely cannot be cached**: `mediafields/…`, whose decision depends on
   per-content-item permissions that change independently, and `_Users/…`, which is caller-relative. That
   exclusion is, I think, the substance of the "not possible" objection — it is true *for those zones*,
   not for the general case.
   Add per-request memoization as a safe backstop regardless: within a single request the decision is
   deterministic, so repeated checks of the same path across endpoint and helper layers cost once.

**Ordering matters:** 1 and 2 are semantics-preserving refactors and should land first; 4 is the only one
carrying invalidation risk and should be last, if at all. And **lever 1 is complementary, not a
substitute** — it removes the *N clients ×* multiplier, while these bound the per-request cost that
remains for genuine navigation.

**Measure it:** the Phase 0 harness answers this directly — run the matrix with `OrchardCore.Media.Security`
enabled and disabled, with the file-store call counter wired in
([spikes/realtime-baseline](spikes/realtime-baseline/README.md)). That produces the numbers the PR
discussion currently lacks.

Two further points worth raising upstream:

- **Document the pairing**: "folder permissions hide names; enable Secure Media to protect content."
- **The manage⊆view coupling** (§3.1b correction) deserves to be explicit in the docs, since it means
  enabling Secure Media silently changes what `ManageMediaFolder` grants.

### 3.2 The four levers, in order of impact

| # | Lever | Effect on the formula |
| --- | --- | --- |
| 1 | **Carry the payload** so clients patch their store instead of refetching | removes the `× (3 + F)` entirely |
| 2 | **Scope by topic** (directory) so only interested clients get an event | `N` → clients viewing that folder |
| 3 | **Coalesce bursts** into one event / one `resync` | 200 events → O(1) per window |
| 4 | Cheaper per-connection transport (SSE vs. SignalR) | second-order; the smallest win |

Target after all four: **1 storage call server-side per event** (to build the payload), **0** client
refetches in the common case, and Redis traffic of one message per event per tenant regardless of
connection count.

Levers 1–3 are transport-independent — they are the reason this plan scales, and they would work on
SignalR too. Keep that honest in the release notes.

### 3.2a Wire format — JSON, and why binary would be *worse* here

The payload is JSON today. The instinct to reach for something denser is right in general and wrong for
SSE specifically, because **SSE is a text protocol**: `data:` lines are UTF-8, framed by `\n\n`. Any
binary encoding must be Base64'd to survive it.

| Format | Raw size vs JSON | After Base64 (mandatory on SSE) | Other costs |
| --- | --- | --- | --- |
| **JSON** | baseline | n/a — sent as-is | none |
| MessagePack | ~-30% | ~+33% on top ⇒ **net larger than JSON** | encode/decode both ends, a JS dependency |
| Protobuf | ~-40% | ~+33% on top ⇒ **roughly JSON, or worse** | schema + codegen — the thing we rejected with gRPC |

So binary loses on the very axis it was chosen for. Add the qualitative points and it is not close:

- **Reliability / evolution.** JSON is self-describing, so additive changes don't break older clients, and
  there is no schema artifact to version and distribute across modules. That matters because this is a
  shared capability other modules will adopt (D3).
- **Debuggability.** An operator can `curl` the stream and read it. For a protocol meant to be adopted
  across OrchardCore, that is worth more than a few hundred bytes.
- **Zero dependencies.** `JSON.parse` is native and fast; `System.Text.Json` is in the framework.

**Where the real performance wins are — inside JSON, not away from it:**

1. **Serialize once per event, never per subscriber** (R1). This is the big one: it turns N
   serializations into one.
2. **Write pre-serialized UTF-8 bytes, with no string round-trip.** Use
   `SseFormatter.WriteAsync<T>(source, stream, itemFormatter, ct)` — the overload taking a custom
   `Action<SseItem<T>, IBufferWriter<byte>>` — and hand it the bytes from
   `JsonSerializer.SerializeToUtf8Bytes`. Carry `ReadOnlyMemory<byte>` in the channel instead of `string`.
   **This also removes the plan's one gating unknown**: we no longer care whether `SseItem<string>` is
   written raw or JSON-quoted, because we control the bytes.
3. **`System.Text.Json` source generation** (`JsonSerializerContext`) — no reflection on the hot path,
   AOT-friendly.
4. **Compact output** (`WriteIndented = false`). Also a *correctness* requirement: a literal newline in a
   `data:` line would split the frame. `System.Text.Json` escapes control characters, so compact output
   is safe on a single line — assert it in a test rather than assuming.
5. **Let SSE be the envelope.** `event:` carries the topic/type and `id:` the sequence, natively. Don't
   wrap the payload in a JSON envelope duplicating them.

**Compatibility rule to document:** changes to the payload are **additive only**; clients must ignore
unknown fields. That is the whole versioning story, and it is why JSON was the right call.

### 3.3 Scalability requirements the implementation must satisfy

- **R1 — Serialize once per event, not once per connection.** Build the JSON payload a single time and
  hand every subscriber the same immutable string. Publishing to N subscribers must be N cheap channel
  writes, not N serializations. (Drives the `SseItem<string>` choice in §5.3.)
- **R2 — Topic index, not a scan.** Look subscribers up by directory path via a dictionary keyed on
  topic. Never enumerate all subscribers per event.
- **R3 — Bounded per-connection buffers with a resync escape hatch.** Each subscriber gets a small
  bounded `Channel` (capacity ~32, `FullMode = DropWrite`). On overflow, mark the subscriber dirty and
  send a single `resync` event instead of the backlog. This bounds memory per connection, gives
  backpressure for free, and converts a burst into one refetch rather than 200.
- **R4 — One shared heartbeat, not one timer per connection.** A single `PeriodicTimer` on a background
  service ticks a comment/`keepalive` frame to all subscribers. N timers at N=5,000 is a real cost.
- **R5 — O(instances) backplane traffic.** One Redis publish per event per tenant; each instance fans
  out locally. Never one Redis message per connection.
- **R6 — Reconnect with jitter.** Full jittered exponential backoff on the client, so a deploy or pod
  restart does not produce a synchronized reconnect storm (which today also triggers N refetches).
- **R7 — Observability.** `System.Diagnostics.Metrics` counters: active subscribers (by tenant),
  events published, events dropped, resyncs issued, heartbeats. Without these we cannot prove any of
  the above in production.
- **R8 — Bounded work on disconnect.** Removing a subscriber must be O(1) and must not leave the topic
  index growing; prune empty topic buckets.

### 3.4 Scale-out: what replaces Azure SignalR Service

The fair objection to this plan: `OrchardCore.Media.SignalR.Azure` gives you a managed service that
**offloads client connections** — browsers connect to Azure SignalR, your app servers hold only a
handful of server-side connections, so 100k clients don't consume 100k app-server sockets. There is no
managed Azure service that does this for SSE. Here is the honest accounting.

#### 3.4.1 What Azure SignalR does *not* solve here

Azure SignalR offloads **connections**, not the **consequences of events**. Under today's design, each
`MediaChanged` broadcast still causes every client to call `GetDirectoryContent` against *your* app
servers and *your* blob storage — `N × (3 + F)` storage operations (§3.1). Azure SignalR delivers the
broadcast more scalably and then the stampede lands on your origin anyway.

So on the actual reported bottleneck, Azure SignalR contributes nothing. Levers 1–3 remove that load
entirely; the connection-holding question is what remains, and it is a much smaller problem.

#### 3.4.2 How many connections are we actually holding?

This feature is admin-only: the endpoint requires `ManageMedia`, so a connection exists only for a
signed-in editor with the Media Gallery open. Realistic concurrency is **tens, maybe low hundreds** for
a large editorial team — not the consumer-scale fan-out Azure SignalR is built for.

An idle SSE connection costs a socket + TLS state + `HttpContext` + a 32-slot channel: order tens of KB.
Kestrel holds tens of thousands of idle connections per instance within ordinary memory. **Measure this
in Phase 0** (`harness.mjs --no-refetch` at increasing N) rather than trusting the estimate — but the
gap between "hundreds of editors" and "what one instance holds" is several orders of magnitude.

Operational caveats that bite before memory does: raise `ulimit -n` (the common 1024 default), and check
any proxy's own per-backend connection ceiling.

#### 3.4.3 The scale-out ladder, in order of reach

1. **Horizontal scale + `IMessageBus` backplane.** N instances each hold a share of the connections;
   each event costs **one Redis message per instance**, independent of connection count (R5). Linear and
   cheap. This is the answer for essentially every real OrchardCore deployment.
2. **No sticky sessions required — an advantage over SignalR.** An SSE stream is a single long-lived
   `GET`. Any instance can serve it, and a reconnect may land anywhere. Self-hosted SignalR needs session
   affinity for `/negotiate` and the long-polling fallback; SSE removes that deployment constraint
   outright. Fewer load-balancer requirements, not more.
3. **Route the stream to a dedicated pool.** Because it is plain HTTP, `api/media/events` can be
   path-routed at the load balancer to a small set of instances tuned for long-lived connections,
   isolating them from request-serving instances. That is connection offload, built from ordinary
   infrastructure and no proprietary protocol.
4. **An external SSE hub** — the direct Azure SignalR equivalent. See §3.4.4; this turned out to be a
   well-populated category, not a gap.

**Azure has no SSE connection-offload service.** API Management can *proxy* SSE and App Service can
*serve* it, but neither offloads connections. **Azure Web PubSub** is not an SSE service either — its
clients connect over WebSocket or MQTT. It is, however, a legitimate *offload* option that fits the same
external-hub seam; see §3.4.4a.

#### 3.4.4 External SSE hubs — the true Azure SignalR equivalents

Researched August 2026. The architecture is identical to Azure SignalR: **browsers connect to the hub,
not to OrchardCore**; OrchardCore holds zero client connections and publishes events to the hub over
plain HTTP. The difference is that these are open protocols with multiple implementations, so there is
no single-vendor lock-in.

| Option | Model | Client transport | How OrchardCore publishes | Notes |
| --- | --- | --- | --- | --- |
| **Mercure** | Open protocol + hub. Self-hosted (Go/Caddy binary, Docker, Helm) **or** managed (Mercure Cloud / Enterprise) | **Native `EventSource`**, topics as query params | `POST` form-urlencoded + `Authorization: Bearer` JWT | Purpose-built for exactly this. Best fit — see below. Hub is **AGPL**; Cloud/Enterprise for commercial support |
| **Centrifugo** | Open-source server, self-hosted; scales via Redis/NATS | SSE/EventSource (`uni_sse`), plus WS/WebTransport | `POST /api/publish` (HTTP or gRPC) | Explicitly markets itself as a self-hosted alternative to Pusher/Ably/**SignalR**. Heavier; its own connect protocol (`cf_connect` param) |
| **Nchan** | nginx module; self-hosted; Redis/Redis Cluster for HA | EventSource, WS, long-poll | `POST` to the channel URL | Fits sites already fronted by nginx. Publish is just an HTTP POST to a channel |
| **Fastly Fanout / Pushpin** | GRIP proxy — managed at Fastly's edge, or self-hosted Pushpin (now a Fastly OSS project) | Transparent SSE — the proxy holds the response open | Origin returns `Grip-Hold: stream`, then publishes updates | Edge-scale. Most "invisible" to the app: your normal endpoint just returns GRIP headers |
| **Ably** | Fully managed SaaS | `/sse` + `/event-stream` endpoints, token or basic auth | REST publish | Turnkey, no ops. Vendor SaaS with per-message pricing |
| **Azure Web PubSub** | Fully managed Azure service | **WebSocket, not SSE** — but via *native* `new WebSocket(url, 'json.webpubsub.azure.v1')`, no JS SDK | REST `POST …/:send` (SDK optional) | The Azure-native offload path. See §3.4.4a |

**Mercure is the closest thing to "Azure SignalR for SSE"** and maps onto this plan's design almost
one-for-one — verified against [the specification](https://mercure.rocks/spec):

- **Publish** = `POST` to the hub, `application/x-www-form-urlencoded`, fields `topic`, `data`, `id`,
  `type`, `private`, with a `Authorization: Bearer <JWT>` publisher token. **No SDK exists for .NET —
  and none is needed.** That is ~15 lines of `HttpClient` and keeps goal §1.2 (zero new packages) intact.
- **Subscribe** = a plain `GET` with `topic` query parameters, consumed by **native `EventSource`**.
  Supports exact match and URL-pattern matchers — which is precisely our per-directory topic scoping
  (§3.2 lever 2) handed to us by the protocol.
- **Authorization** = JWT with an `authorization_details` claim granting `subscribe` on specific topics;
  updates flagged `private` reach only authorized subscribers, re-checked at dispatch. So OrchardCore
  performs its `ManageMedia` check once, mints a short-lived subscriber JWT scoped to the topics that
  user may see, and the hub enforces it thereafter. This also addresses the Secure Media concern in §10.7.
- **Resume** = `Last-Event-ID` header or `last_event_id` query param; the hub replays events published
  after that id. That is §5.3's ring-buffer requirement, implemented by the hub instead of by us.

##### 3.4.4a Azure Web PubSub — the Azure-native offload path

Worth stating precisely, because it is the natural question for anyone currently on
`OrchardCore.Media.SignalR.Azure`.

**Is it equivalent to Azure SignalR?** As a *managed connection-offload service*, yes — same
architecture, same Azure billing model, browsers connect to the service instead of to your app. But
Microsoft is explicit that **Web PubSub is not a replacement for Azure SignalR Service**: there is no
hub, no strongly-typed client invocation, no RPC, and no transport fallback. It is generic WebSocket
pub/sub.

**For this feature, every one of those missing pieces is something we don't use.** The Media Gallery
needs one-way broadcast of tiny payloads (§1.1.1) — no RPC, no hub methods, no streaming. So the gap
between the two services is real in general and empty for us.

It also has a concrete advantage over the SignalR transport, confirmed in the subprotocol docs: a client
connects with **plain browser WebSocket** —

```js
new WebSocket('wss://<name>.webpubsub.azure.com/client/hubs/<hub>', 'json.webpubsub.azure.v1')
```

— with **no Microsoft JS SDK**. Groups map onto our per-directory topics (§3.2 lever 2), and the server
publishes over REST. So a `OrchardCore.Media.Realtime.WebPubSub` transport would give Azure users
managed offload **while still shedding the `@microsoft/signalr` bundle** — which retaining the SignalR
transport (§3.4.5) cannot do. It slots into the same external-hub notifier seam as Mercure; only the
browser-side transport differs (WebSocket vs `EventSource`).

Caveats: Azure-only and paid; `Azure.Messaging.WebPubSub` is optional but token minting still has to be
implemented; and it is a fourth transport to test and maintain. Treat it as a *candidate for Phase 7*
alongside Mercure, decided by which the OrchardCore userbase actually needs — not as day-one work.

**License note:** the reference Mercure hub is **AGPL**. Run as a separate service (not linked into
OrchardCore) that is normally acceptable, but it must be called out in the docs, and it is a reason
some deployments will prefer Centrifugo (MIT), Nchan (dual BSD/GPL-ish — verify), or the managed
editions.

**Design implication.** The §5 transport seam should admit a third implementation shape beyond
"in-process SSE" and "SignalR": an **external-hub notifier** where OrchardCore only publishes and never
holds connections. That is a small `IMediaChangeNotifier` implementation plus a subscriber-token
endpoint, and it makes the Azure SignalR capability gap disappear — with *more* options than SignalR
had, since one implementation can target Mercure and the same pattern covers Nchan and Centrifugo.

**Recommendation:** do **not** build this in Phase 2–5. Ship in-process SSE first (it covers realistic
admin-scale, §3.4.2), keep the seam clean, and add `OrchardCore.Media.Realtime.Mercure` as a later,
optional feature if any deployment actually needs connection offload. Record it now so the seam is
designed with it in mind.

#### 3.4.5 The mitigation for existing Azure SignalR users

Until an external-hub feature (§3.4.4 / §3.4.4a) exists — and note that a Web PubSub transport would
serve Azure offload users *better* than this, by dropping the JS SDK — **keep `OrchardCore.Media.SignalR`
as a supported alternative transport**
behind the §5 abstraction, with SSE as the default. The transport seam makes this nearly free: both
implement the same notifier/registry contract, and the client picks based on the feature that is
enabled. Deployments that genuinely need managed connection offload keep Azure SignalR; everyone else
drops two packages and a JS dependency.

This changes §9 from "delete SignalR" to "demote SignalR to opt-in", and it is the recommendation.

#### 3.4.6 Which hub — decision: **Mercure**, with Pushpin/GRIP as runner-up

Scored on what actually matters here, not on raw throughput (all of them exceed our needs by orders of
magnitude):

| Criterion | **Mercure** | Centrifugo | Nchan | Pushpin / Fanout | Ably | Web PubSub |
| --- | --- | --- | --- | --- | --- | --- |
| Browser client library needed | **none** (native `EventSource`) | `centrifuge-js`, or raw SSE with `cf_connect` | none | none (transparent) | none | none (native `WebSocket`) |
| **Works with cookie auth mode** | only if hub shares a parent domain (§3.4.8) | mint + `cf_connect` | nginx `auth_request` | **yes, natively — same origin** | mint | mint |
| Token out of the URL | **yes** — `mercureAuthorization` cookie | no for uni-SSE (`cf_connect` query param) | via nginx `auth_request` | n/a — auth stays at origin | header or query | query |
| Per-topic authorization | **JWT topic selectors** — direct map of `IRealtimeTopicAuthorizer` | channels + token claims | nginx subrequest to OrchardCore | origin decides, natively | capability tokens | groups in token |
| Replay after reconnect | **`Last-Event-ID` replay** | history API | yes | depends | yes | reliable subprotocol |
| Publish from .NET | form POST, no SDK | HTTP/gRPC, no SDK | HTTP POST | GRIP headers | REST | REST |
| License / hosting | **AGPL** hub; Cloud + Enterprise | MIT, self-host only | BSD-ish, self-host | Apache (Pushpin) / Fastly | SaaS | Azure |

**Why Mercure wins for us:**

1. **It is the only option that keeps the zero-JS-dependency goal (§1.2) intact** while offloading
   connections — plain `EventSource`, no client library. Centrifugo's full feature set wants
   `centrifuge-js`, which re-adds exactly what we removed.
2. **Its authorization model *is* our design.** A JWT carrying subscribe-topic selectors is the literal
   serialization of what `IRealtimeTopicAuthorizer` computes (§4.2). No impedance mismatch.
3. **It fixes §5.4.1 catch 5.** Redis pub/sub has no replay, so cross-instance `Last-Event-ID` resume is
   unsound in the self-hosted design. Mercure's hub replays events after a given id — the hub mode is
   strictly *better* on resume than the in-process one.
4. **The token stays out of the URL** via the `mercureAuthorization` cookie — the documented answer to
   `EventSource`'s inability to set headers. Query-string tokens leak into logs and referrers.
5. Self-hosted **and** managed, so it does not force an ops decision on adopters.

Accepted trade-off: the reference hub is **AGPL** (run as a separate process, so this is a deployment
note rather than a licensing constraint on OrchardCore) — and Centrifugo (MIT) stays the documented
alternative for those who object.

> **Decision history:** §3.4.8 provisionally moved to Pushpin/GRIP once cookie mode was treated as a peer
> of bearer. §3.4.8b then established that Mercure's canonical **same-origin** deployment
> (`/.well-known/mercure`) satisfies cookie mode natively, restoring Mercure as the decision. Read
> §3.4.8b for the current answer.

**Runner-up on the bearer-only analysis: Pushpin / Fastly Fanout.** With GRIP, the browser calls
*OrchardCore's own* `api/realtime/stream`, OrchardCore authorizes with the existing bearer token exactly
as in the in-process design, and then hands the held connection to the proxy. **No token minting, no new
auth surface, no second authorization model.**

**For Azure-hosted deployments**, `…Realtime.WebPubSub` (§3.4.4a) remains the natural choice regardless.

#### 3.4.7 Does PKCE survive hub mode? Yes — it moves one step

The short answer: **PKCE is unchanged.** It governs how the browser authenticates to *OrchardCore*, and
that is untouched. The hub credential is a separate, narrower, derived token.

```
1. Browser → OrchardCore OIDC   silent PKCE in a hidden iframe (prompt=none)   [UNCHANGED]
                                → OrchardCore access token
2. Browser → OrchardCore        POST api/realtime/hub-token  (Bearer <that token>)
                                → OrchardCore validates it, runs IRealtimeTopicAuthorizer
                                  over the requested topics, mints a SHORT-LIVED hub JWT
                                  carrying only the granted subscribe selectors
3. Browser → Hub                EventSource(hubUrl, {withCredentials:true})
                                with the hub JWT in the `mercureAuthorization` cookie
4. OrchardCore → Hub            POST updates, signed with the publisher JWT (server-side only)
```

This is a security **improvement** over handing the hub a general-purpose token:

- The hub token is **subscribe-only** and **topic-scoped** — it cannot call the Media API, upload, or
  read anything else. Stolen, it leaks only the events for folders the user could already see.
- The `ManageMedia` and Secure Media checks still run in OrchardCore, once, at mint time — authorization
  logic never leaves the application.
- Mercure can validate via **JWKS** (`subscriber_jwks_url`), so it can consume OrchardCore's OpenIddict
  signing keys with standard rotation, instead of a shared HMAC secret. Prefer that.
- No token in the URL (cookie), unlike the in-process design's `?access_token=`.

**The one real caveat: revocation.** The hub honours a minted token until it expires; OrchardCore cannot
recall it. If a user loses `ManageMedia`, they keep receiving events for the remainder of the TTL. So:
**short TTL (2–5 minutes)** with the client silently re-minting — which the existing silent-PKCE renewal
loop already gives us the shape for — and document the window. The in-process design has no such gap,
because authorization is re-checked per connection at the origin. That is a genuine cost of hub mode,
and another point in Pushpin/GRIP's favour.

Cross-origin note: a hub on a different host means the cookie needs `SameSite=None; Secure`, and the hub
needs a CORS policy allowing credentials from the tenant origin — the same shape as the existing
standalone-gallery CORS work.

#### 3.4.8 Cookie mode changes the hub decision — **Pushpin/GRIP**

[`MediaApiSettings`](src/OrchardCore.Modules/OrchardCore.Media/MediaApiSettings.cs) makes bearer and
cookie **peer** authentication schemes, exactly one active at a time. PKCE is the more secure of the two
and is rightly the recommended default, but cookie mode is a first-class supported configuration — so
any hub we adopt must serve both without a second-class path.

**Where Mercure struggles in cookie mode.** Its browser credential is the `mercureAuthorization` cookie,
and `Set-Cookie` is bound by the domain rules: OrchardCore can only set that cookie for its own domain
or a **shared registrable parent** (`app.example.com` setting a cookie on `.example.com` for
`hub.example.com`). A managed hub on a foreign domain — Mercure Cloud on `*.mercure.rocks` — **cannot
receive a cookie OrchardCore sets**. The fallbacks are both bad: put the JWT in the query string (leaks
into logs, proxies and referrers — the very thing §3.4.6 credited Mercure for avoiding), or use an
`EventSource` polyfill that can set headers, which re-adds a JS dependency and defeats §1.2. So cookie
mode + Mercure works only in the same-parent-domain deployment, and quietly degrades otherwise.

**Why GRIP has no such problem — verified.** Pushpin passes the **original request headers and cookies
through to the backend**; when the backend doesn't respond with GRIP instructions, requests simply pass
through. The proxy has **no authentication model of its own** — it delegates entirely to the origin. So:

- **Cookie mode**: the admin cookie reaches OrchardCore on the stream request exactly as it does for
  `api/media/*` today. Zero new work.
- **Bearer mode**: the `Authorization` header (or the existing `?access_token=` promotion) reaches
  OrchardCore identically. Zero new work.
- **Same origin by construction** — the proxy fronts the tenant, so the stream URL *is* the OrchardCore
  URL. No CORS changes, no cookie-domain constraints, and the standalone gallery's existing CORS setup
  is untouched.
- **No token minting, no second credential type, and no §3.4.7 revocation gap** — authorization is
  re-evaluated at the origin on every connect, precisely as in the in-process design.
- **Dynamic subscriptions are supported**: GRIP control messages sent in the held stream can change which
  channels a connection is subscribed to, which is what §4.2's folder-navigation design needs. *Verify
  against Pushpin's docs during the spike* — this is the one mechanism the design depends on.

**Provisional decision (superseded — see §3.4.8b):** `OrchardCore.Realtime.Pushpin` (GRIP), on the
grounds that it is the only genuinely auth-agnostic option.

Also note the corollary: because GRIP keeps authorization at the origin, **the `IRealtimeTopicAuthorizer`
work in §4.2 is reused unchanged** in hub mode. With Mercure, that logic has to be re-expressed as JWT
topic selectors — a second encoding of the same rules, and a second place to get them wrong.

#### 3.4.8b Re-examined — **Mercure wins, deployed same-origin**

§3.4.8's cookie objection assumed a hub on a **foreign domain**. That is not Mercure's canonical
deployment, and researching the actual topology dissolves the blocker.

**The documented default is same-origin.** The hub is served at **`/.well-known/mercure`** and is
routinely run behind the application's own reverse proxy — nginx, Traefik, or as a Caddy module (the hub
*is* a Caddy build). Mercure's own docs state the constraint directly: to use cookie authentication, the
app and the hub must be served **from the same domain (subdomains allowed)**. Mount it under the tenant's
domain and the constraint is satisfied by construction.

So both `MediaApiSettings` schemes work:

| Mode | Flow |
| --- | --- |
| **Cookie** (default) | Browser is already authenticated by the admin cookie. OrchardCore mints a short-lived subscriber JWT and returns it as the `mercureAuthorization` cookie on **its own domain**; the browser sends it to `/.well-known/mercure` automatically with `EventSource(url, {withCredentials: true})`. No cross-origin, no `SameSite=None`. |
| **Bearer / PKCE** | Unchanged silent PKCE against OrchardCore (§3.4.7). The access token buys a subscriber JWT from `POST api/realtime/hub-token`; it travels in the cookie or the `Authorization` header. |

**JWKS removes the shared secret.** `subscriber_jwks_url` lets the hub fetch OrchardCore's OpenIddict
public keys and validate subscriber tokens without any shared HMAC secret, with standard key rotation.
That is a materially better operational story than a copied secret in config.

**Why this now beats GRIP**, given the stated preference for a verified protocol:

- **Mercure is an IETF Internet-Draft** (`draft-dunglas-mercure`) with a public specification. GRIP has
  no published draft — a 2013 intent that never landed (§3.4.9). For an upstream OrchardCore
  contribution, that difference is worth real weight.
- Mercure's spec has an explicit authorization model; GRIP's spec, verified by reading it, **says nothing
  about securing the publish/control endpoint** (§3.4.9).
- Native `EventSource`, no JS dependency; `Last-Event-ID` replay (fixes §5.4.1 catch 5); publish by form
  POST with no SDK; self-hosted **and** managed.

**Costs kept honestly on the books** — these are real and GRIP avoids them:

1. **A token-minting endpoint is required in *both* modes**, cookie included. GRIP needed none.
2. **The revocation window remains** (§3.4.7): short TTL, silent re-mint, documented gap.
3. **Authorization rules are expressed twice** — once as `IRealtimeTopicAuthorizer`, once as JWT topic
   selectors. Keep the authorizer the single source of truth and *derive* the selectors from it.
4. **Managed Mercure Cloud on a foreign domain loses cookie mode** — document it as bearer-only.
5. CORS: with any authorization mechanism the hub cannot use `cors_origins: *`; origins must be listed.

**Decision: `OrchardCore.Realtime.Mercure`, deployed same-origin at `/.well-known/mercure`.** Keep
Pushpin/GRIP documented as the alternative for anyone who prefers zero token minting and no revocation
window, and `…WebPubSub` for Azure. All three are small adapters behind the §5 seam — this only settles
which one is built first.

##### 3.4.8a Cookie mode is a *topology* constraint, not a scalability one

Worth stating plainly, because the two get conflated. The authentication scheme has **no effect on any
of the three quantities that determine scalability**: connection count, fan-out cost per event, and
storage operations per event (§3.1). Cookie validation (Data Protection decrypt) and bearer validation
(signature verify) are both trivial, and both happen **once per connection**, not per event. Nothing in
§3.2's four levers changes with the scheme.

What we actually found is narrower: **cookie credentials are domain-bound**, so they don't reach a hub
that lives on a foreign domain and carries its own credential. That bites Mercure Cloud. It does not
bite GRIP — same origin by construction — which is exactly why §3.4.8 chose it. And it does not touch
the recommended path at all: in-process SSE (Phases 2–5) is same-origin, so the cookie works natively.

Two genuine cookie-related constraints, both pre-existing and neither about real-time:

- **Multi-instance needs a shared Data Protection key ring** so the cookie decrypts on any instance.
  Already an OrchardCore requirement, site-wide, and already documented in
  [MediaGallery.md](src/docs/reference/modules/Media/MediaGallery.md).
- **The cross-origin standalone gallery already requires bearer.** Cookie mode is an embedded-admin
  configuration by design.

That last point resolves the tension: the deployments that would ever want an external hub — very large,
multi-origin, standalone — are **already** the bearer ones. Cookie is the zero-config default
([MediaApiSettings.cs:24](src/OrchardCore.Modules/OrchardCore.Media/MediaApiSettings.cs#L24)) for
ordinary embedded admin use, which is precisely the scale where in-process SSE is more than sufficient.
The two ends of the spectrum barely intersect.

**So: do not weaken cookie mode to suit a hub.** It exists so the gallery works out of the box before
OpenID is configured; a Phase 7 optional transport is not a reason to compromise the default experience.

#### 3.4.9 Is GRIP a *recognised secure* way to build real-time endpoints? Qualified yes

Assessed August 2026. Worth separating three different questions that tend to get conflated.

**Is the architecture sound?** Yes, and it is the most conservative of the hub options. Authorization
never leaves the application: the client authenticates to OrchardCore with the credential it already has,
OrchardCore runs its normal checks, and only then does the proxy hold the connection. There is no second
authorization system, no minted credential, no revocation window (§3.4.7), and no new secret in the
browser. Compared with every other hub, GRIP *reduces* the security surface rather than adding one.

**Is the project credible?** Reasonably. Pushpin is a Fastly open-source project and powers **Fastly
Fanout**, a commercial service running at CDN edge scale — a meaningful production signal, not a hobby
project. `Grip-Sig` is a signed JWT with a mandatory `exp` by which the backend verifies a request came
from a trusted proxy, and the spec is explicitly **fail-closed**: "if the token cannot be fully verified
for any reason, including expiration, then the backend should behave as if the header wasn't present."

**Is it a standard, and is it audited?** This is where honesty is required:

- **GRIP is not an IETF standard.** A draft was described as "planned" in 2013 and there is no evidence
  of a published RFC or current draft. It is a vendor-originated open protocol. By contrast **Mercure
  *is* an IETF Internet-Draft** (`draft-dunglas-mercure`) — a point in Mercure's favour that §3.4.8's
  decision does not erase.
- **The GRIP spec gives no security guidance for the EPCP publish/control endpoint** — verified by
  reading it. Anyone who can reach that endpoint can inject messages into any channel. Protecting it is
  entirely the operator's job and entirely undocumented by the protocol. **Our docs must fill that gap**:
  the control port must never be exposed beyond the application network.
- **No CVEs surfaced for Pushpin.** Do not read that as a clean bill of health — for a niche project it
  as plausibly reflects low audit attention. The audit surface is far smaller than SignalR, nginx, or
  ASP.NET Core.

**One concrete risk specific to our design.** Our channel names encode media directory paths
(`media:dir:client-contracts/2026`). GRIP instructions travel as **response headers** from the origin. If
the origin stays directly reachable while a proxy is deployed, a client bypassing the proxy receives
`Grip-Channel: media:dir:…` in plain response headers — leaking folder names to anyone who can reach the
app. **Requirement:** the stream endpoint must verify `Grip-Sig` and refuse to emit GRIP headers when it
is absent or invalid, falling back to the in-process SSE path. Add it to §8's test matrix as a
security test, not an afterthought.

**Verdict:** safe to build on, with three conditions — verify `Grip-Sig` and fail closed; keep the
control endpoint on a private network; and treat this as a *later optional* transport (Phase 7) rather
than a dependency of the core plan. The in-process SSE design (§5) depends on none of it, which is
precisely why it ships first.

### 3.5 Capacity expectations to validate in the spike

| Metric | SignalR today (to be measured) | SSE target |
| --- | --- | --- |
| Storage ops per event (N=50, F=10) | 650 | 1 |
| Server bytes per event per client | hub envelope + payload | `data:` line + `\n\n` |
| Redis messages per event | 1 (SignalR backplane) | 1 (`IMessageBus`) |
| Per-connection server memory | baseline | target ≤ baseline |
| Reconnect storm at N=50 | N refetches | 0 refetches (resync only if stale) |

---

## 4. Architecture — a shared capability, with Media Gallery as the reference consumer

Per the project goal (§1.5), this is **not** a media-specific transport. It is an OrchardCore real-time
capability that any module can publish through, with the Media Gallery as its first consumer and worked
example, and `OrchardCore.Notifications` as the intended second (§4.3).

### 4.1 Layers

```
  ┌─ consumers ───────────────────────────────────────────────────────────┐
  │  OrchardCore.Media          IMediaEventHandler → IRealtimeNotifier    │
  │                             MediaTopicAuthorizer : IRealtimeTopicAuthorizer
  │  OrchardCore.Notifications  INotificationMethodProvider ("Real-time") │
  │  …any module                                                          │
  └───────────────────────────────┬───────────────────────────────────────┘
                                  │  Publish(topic, payload)
  ┌─ OrchardCore.Realtime.Core ───▼───────────────────────────────────────┐
  │  RealtimeRegistry     topic → subscribers  (per-tenant singleton)     │
  │                       Channel<SseItem<string>> each (bounded)         │
  │  RealtimeBackplane    IConnectionMultiplexer pub/sub  ⇄ other instances│
  │  Heartbeat            one shared PeriodicTimer (R4)                   │
  └───────────────────────────────┬───────────────────────────────────────┘
                                  │
  ┌─ OrchardCore.Realtime (module, feature: …Realtime.Sse) ───────────────┐
  │  GET  api/realtime/stream?topics=…   TypedResults.ServerSentEvents    │
  │  POST api/realtime/subscriptions     add/remove topics, no reconnect  │
  │  IRealtimeTopicAuthorizer[] consulted per requested topic             │
  └───────────────────────────────┬───────────────────────────────────────┘
                                  │  one stream per browser tab, multiplexed
  ┌─ browser ─────────────────────▼───────────────────────────────────────┐
  │  @bloom/services/realtime  →  per-feature handlers                    │
  │      media  → patch the gallery store (no refetch)                    │
  │      notifications → toast + badge                                    │
  └───────────────────────────────────────────────────────────────────────┘
```

Local/remote split is unchanged: the originating instance notifies its own subscribers **in-process** and
publishes to the backplane for the others.

### 4.2 Two decisions the reuse goal forces

**One multiplexed stream per tab, not one per feature.** If every module opened its own `EventSource`,
a browser on HTTP/1.1 would exhaust its six-connection budget after a handful of features, and each
stream would carry its own TLS and heartbeat overhead. So there is a **single** `api/realtime/stream`
carrying every topic the user is subscribed to, demultiplexed client-side by the topic prefix. This
also makes the "several admin tabs" concern (§6.2) a per-tab constant rather than per-feature growth.

**Subscriptions must change without tearing down the stream.** The Media Gallery changes topic on every
folder navigation. With a shared stream, reconnecting for that would also drop the notifications feed.
Hence `POST api/realtime/subscriptions` against a connection id, instead of the reconnect-on-navigation
approach a media-only design would have used. The client generates the connection id and passes it on
connect; the server rejects ids it does not own.

**Topic naming is a contract**, namespaced per feature to prevent collisions:

| Feature | Topic | Fan-out shape |
| --- | --- | --- |
| Media | `media:dir:{path}` (and `media:tree` for folder changes) | group broadcast |
| Notifications | `notification:user:{userId}` | single user |
| *(convention)* | `{feature}:{kind}:{scope}` | — |

**Authorization is per topic, provided by the owning module.** `IRealtimeTopicAuthorizer` answers
"may this principal subscribe to this topic?". The stream endpoint asks the authorizers for each
requested topic and **silently drops** unauthorized ones rather than erroring, so topic existence is not
leaked. Media's authorizer checks `ManageMedia` (plus Secure Media, §10.7); Notifications' checks that
the topic's user id is the current user. A topic no authorizer claims is denied by default.

### 4.3 The second consumer, concretely

`OrchardCore.Notifications` currently has no browser push at all — notifications materialize on page
load. Its [`INotificationMethodProvider`](src/OrchardCore/OrchardCore.Notifications.Abstractions/INotificationMethodProvider.cs)
is a ready-made extension point: a "Real-time" provider publishes to `notification:user:{id}` and the
client renders a toast and updates the badge. That is a small, genuinely useful feature that proves the
abstraction is not media-shaped — and it is the honest test of §1.5, since an abstraction with one
consumer is just an indirection.

Media broadcasts to a *group*; notifications target a *single user*. Supporting both is what keeps the
registry generic (topics are opaque strings) rather than accidentally media-specific.

---

## 5. Server implementation

Split between new core projects and the Media consumer:

- `src/OrchardCore/OrchardCore.Realtime.Abstractions/` — `IRealtimeNotifier`, `IRealtimeTopicAuthorizer`,
  `RealtimeEvent`. This is all a consuming module references.
- `src/OrchardCore/OrchardCore.Realtime.Core/` — `RealtimeRegistry`, backplane, heartbeat.
- `src/OrchardCore.Modules/OrchardCore.Realtime/` — the SSE endpoints and features.
- `src/OrchardCore.Modules/OrchardCore.Media/Realtime/` — the Media consumer: event handler and topic
  authorizer only. (`Hubs/` stays until §9.)

§5.1–5.5 below describe the core; §5.6 the Media consumer. Names use `Realtime*` where a type is generic
and `Media*` where it is not.

### 5.1 The contracts (Abstractions)

What a consuming module sees — nothing media-specific:

```csharp
// OrchardCore.Realtime.Abstractions
public interface IRealtimeNotifier
{
    // payload is serialized ONCE by the caller (R1); topic follows the {feature}:{kind}:{scope} convention.
    ValueTask PublishAsync(string topic, string eventType, string payload);
}

public interface IRealtimeTopicAuthorizer
{
    // Return null for "not my topic" so the endpoint can deny unclaimed topics by default.
    bool? Handles(string topic);
    Task<bool> AuthorizeAsync(ClaimsPrincipal user, string topic);
}
```

The Media event shape then belongs to the Media module, not the core:

```csharp
// OrchardCore.Media/Realtime/MediaChangeEvent.cs
public sealed record MediaChangeEvent
{
    public required string Action { get; init; }       // fileUploaded | fileDeleted | directoryCreated | …
    public required string Path { get; init; }
    public string NewPath { get; init; }
    public FileStoreEntryDto Item { get; init; }       // populated for create/copy/move so clients patch
}
```

`Item` is what removes the refetch (lever 1). It is built **once**, server-side, from a single
`GetFileInfoAsync` — reusing `MediaEndpointHelpers.CreateFileResult` so the DTO is byte-identical to
what `GetDirectoryContent` returns and the client can splice it into `fileItems` directly.

Sequence numbers and `SseItem` framing are the core's business, not the consumer's.

### 5.2 Registry (Core)

```csharp
// OrchardCore.Realtime.Core/RealtimeRegistry.cs  — per-tenant singleton
public sealed class RealtimeRegistry
{
    // R2: topic → subscribers. R8: buckets pruned when empty.
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, Subscriber>> _byTopic = new();
    // Connection id → subscriber, so subscriptions can be edited without reconnecting (§4.2).
    private readonly ConcurrentDictionary<Guid, Subscriber> _byConnection = new();

    public Subscriber Connect(Guid connectionId, IEnumerable<string> topics);
    public void Disconnect(Subscriber subscriber);      // O(1), prunes empty buckets
    public void UpdateTopics(Guid connectionId, IEnumerable<string> add, IEnumerable<string> remove);
    public void Publish(string topic, string eventType, string payload, long sequence);  // R1
    public void Heartbeat();                            // R4: called by the shared timer
}
```

A subscriber now holds a *set* of topics rather than one, since a single stream is multiplexed across
features. Fan-out still indexes by topic (R2) — a subscriber appears in one bucket per topic.

`Subscriber` wraps `Channel.CreateBounded<SseItem<string>>(new BoundedChannelOptions(32) { FullMode = BoundedChannelFullMode.DropWrite, SingleReader = true })` plus a `_dirty` flag for R3. On a dropped
write, set `_dirty`; the reader emits one `resync` event and clears it.

**Topic granularity (Media).** Exact directory path — `media:dir:a/b` for a change to `a/b/c.png` —
plus `media:tree` for events that alter the folder hierarchy, since the tree is visible from every view.
Revisit prefix-matching only if measurements justify it.

### 5.3 The endpoints (Realtime module)

Generic and feature-agnostic: authentication is the tenant's normal scheme, and *authorization is
delegated per topic* to whichever module owns it.

```csharp
// OrchardCore.Realtime/Endpoints/RealtimeStreamEndpoint.cs
builder.MapGet("api/realtime/stream", HandleAsync).ExcludeFromDescription();   // a stream, not REST
builder.MapPost("api/realtime/subscriptions", UpdateAsync).ExcludeFromDescription();

[Authorize]
private static async Task<IResult> HandleAsync(
    HttpContext httpContext,
    RealtimeRegistry registry,
    IEnumerable<IRealtimeTopicAuthorizer> authorizers,
    Guid connectionId,
    string topics)            // comma-separated, namespaced
{
    // Unauthorized topics are dropped silently, never 403'd: existence must not leak.
    var granted = await AuthorizeTopicsAsync(authorizers, httpContext.User, Split(topics));

    return TypedResults.ServerSentEvents(Stream(registry, connectionId, granted, httpContext.RequestAborted));
}

private static async IAsyncEnumerable<SseItem<string>> Stream(
    RealtimeRegistry registry, Guid connectionId, IReadOnlyList<string> topics,
    [EnumeratorCancellation] CancellationToken ct)
{
    using var subscriber = registry.Connect(connectionId, topics);
    await foreach (var item in subscriber.Reader.ReadAllAsync(ct))
    {
        yield return item;
    }
}
```

`SseItem.EventType` carries the topic namespace so the client demultiplexes without parsing the payload.
`POST api/realtime/subscriptions` re-runs the same authorization for added topics and calls
`registry.UpdateTopics` — that is what lets the gallery change folder without disturbing the
notifications feed (§4.2).

`using var` + `RequestAborted` gives deterministic cleanup on disconnect (R8). Uses the verified
`TypedResults.ServerSentEvents<T>(IAsyncEnumerable<SseItem<T>>)` overload with `T = string`, so the
payload is written verbatim — satisfying R1 (JSON built once by the publisher).

> **Verify in the spike:** that `SseItem<string>` data is written raw rather than JSON-quoted. If it is
> quoted, switch to `SseFormatter.WriteAsync` over a manual `Results.Stream`, which definitely writes
> raw UTF-8. This is the single most important API behaviour to confirm before building on it.

Also set `ReconnectionInterval` on the first item to advertise a `retry:` value, and use
`SseItem.EventId = Sequence` to enable `Last-Event-ID` resume.

### 5.4 The event handler and backplane

```csharp
// OrchardCore.Media/Realtime/MediaRealtimeEventHandler.cs : IMediaEventHandler
//   — replaces MediaSignalREventHandler; the Media consumer of IRealtimeNotifier (§5.6)
//   builds MediaChangeEvent (one GetFileInfoAsync for create/copy/move)
//   → serializes once
//   → notifier.PublishAsync("media:dir:{path}", "media", json)
//
// OrchardCore.Realtime.Core/RealtimeNotifier.cs : IRealtimeNotifier
//   → registry.Publish(topic, eventType, json, seq)   [local subscribers]
//   → backplane.PublishAsync(topic, json)             [other instances, R5]

// Realtime/MediaChangeBackplane.cs : IModularTenantEvents
//   ActivatedAsync → messageBus.SubscribeAsync("MediaChanged", (channel, json) => registry.Publish(...))
//   Follows the DistributedSignal pattern; RedisBus is already tenant-scoped by shell name.
```

Backplane is optional: with no `IMessageBus` registered, single-instance in-memory delivery still works —
matching today's degradation behaviour. Resolve it with `GetService<IMessageBus>()`, never
`GetRequiredService`.

#### 5.4.1 Catches in `IMessageBus` / `RedisBus` — read before relying on it

`IMessageBus` was built for one caller: [`DistributedSignal`](src/OrchardCore/OrchardCore/Caching/Distributed/DistributedSignal.cs),
which publishes **cache-signal keys** — short, slash-free, loss-tolerant identifiers. Our payload is
JSON containing media paths. Several of its design choices only hold for the original use case.

**1. `RedisBus` silently drops any message containing `/` — a blocker for us.**
[RedisBus.cs:44-49](src/OrchardCore.Modules/OrchardCore.Redis/Services/RedisBus.cs#L44-L49) publishes
`"{hostname}:{pid}/{message}"` and parses it back with:

```csharp
var tokens = redisValue.ToString().Split('/').ToArray();
if (tokens.Length != 2 || ...) { return; }   // no log, no error
```

An unbounded `Split('/')`. Any payload containing a slash yields more than two tokens and is **discarded
without a trace**. Media paths (`"photos/2026/banner.png"`) contain slashes by definition, so
cross-instance delivery would fail silently for exactly the events we care about — while working
perfectly on a single instance and in every unit test. `DistributedSignal` never hit this because
signal keys have no slashes.

Options, in order of preference:
  - **Encode the payload** to a slash-free alphabet before publishing: Base64**Url** (`Base64Url.EncodeToString`,
    built into .NET 9+) or hex. Note plain Base64 is **not** safe — its standard alphabet includes `/`.
  - **Fix it upstream**: `Split('/', 2)` is a two-character change and a good OrchardCore PR. Ship the
    encoding regardless, so we work against unpatched versions.
  - **Bypass `IMessageBus`** and use `IConnectionMultiplexer.GetSubscriber()` directly — more control
    (see catches 3, 5, 7), at the cost of not reusing shared infrastructure.

**2. It is gated on the wrong feature.** `IMessageBus` is registered by
[`OrchardCore.Redis.Bus`](src/OrchardCore.Modules/OrchardCore.Redis/Startup.cs#L98-L106), not
`OrchardCore.Redis`. The `OrchardCore.Media.Realtime.Redis` feature must depend on
**`OrchardCore.Redis.Bus`** (§7.1 corrected accordingly), and the docs must say so — enabling plain
`OrchardCore.Redis` gives you a working cache and no backplane.

**3. If Redis is unreachable at tenant activation, the backplane is off — permanently and quietly.**
`SubscribeAsync` logs an error and returns without subscribing, with no retry
([RedisBus.cs:28-36](src/OrchardCore.Modules/OrchardCore.Redis/Services/RedisBus.cs#L28-L36)). The
tenant then runs indefinitely with no cross-instance delivery until it is recycled. Ordering matters at
container start-up. (Transient blips *after* subscribing are fine — StackExchange.Redis restores
subscriptions on reconnect.) Mitigation: a health check plus a retry-until-subscribed loop on our side.

**4. Publish failures are swallowed.** `PublishAsync` catches and logs
([RedisBus.cs:76-79](src/OrchardCore.Modules/OrchardCore.Redis/Services/RedisBus.cs#L76-L79)); the caller
cannot tell that an event never propagated. No retry, no outbox. Our metrics (R7) must count publishes
separately from *successful* publishes, or a broken backplane looks healthy.

**5. Fire-and-forget delivery undermines cross-instance `Last-Event-ID` resume.** Redis pub/sub has no
replay: a momentarily disconnected instance simply misses events. So a client reconnecting to instance B
cannot be resumed for events only instance A ever saw. **Design consequence:** treat resume as
best-effort *within one instance*, and make `resync` (R3) the guaranteed fallback. Alternatively use a
Redis Stream instead of pub/sub — but that is well beyond reusing `IMessageBus`.

**6. The handler is `Action<string, string>` — synchronous and void.** No async, no cancellation, and it
runs on StackExchange.Redis's subscriber callback. Anything blocking there hurts every subscriber in the
process. Our handler must do nothing but non-blocking `TryWrite`s — which fits R1/R3, but **forbids
enriching the payload on the receiving side**. The `Item` DTO (§5.1) must therefore be built by the
*publishing* instance before it goes on the wire. That is what we want anyway (one `GetFileInfoAsync`
total, §3.2 lever 1), but it is now a constraint rather than a preference.

**7. There is no `Unsubscribe`, and the multiplexer is process-wide.**
[`RedisDatabaseFactory`](src/OrchardCore.Modules/OrchardCore.Redis/Services/RedisDatabaseFactory.cs#L13)
caches connections in a `static ConcurrentDictionary` shared across tenants and ref-counted, and
`RedisService` never disposes it per shell. Because `IMessageBus` exposes no unsubscribe, **every shell
reload adds another handler** capturing the old shell's registry, and the old one is never removed.
Handlers accumulate per reload. Mitigation: subscribe once per process and dispatch by tenant name
through a stable lookup, or manage `ISubscriber.UnsubscribeAsync` ourselves on `TerminatedAsync` (which
means catch 1's option 3 — going direct).

**8. Event sequence numbers must be globally unique.** A per-instance counter for the SSE `id:` field
collides across instances and corrupts `Last-Event-ID`. Use an instance-scoped prefix
(`{instanceId}-{counter}`) or a Redis `INCR`, and treat ids as opaque on the client.

**9. Self-messages are filtered** (already covered in §4): the publishing host's own subscription drops
its own message, so local delivery must be wired in-process. This is by design, not a bug — but it means
the local and remote paths are asymmetric and both need testing.

**Net assessment — go direct to `IConnectionMultiplexer`.** `IMessageBus` is attractive (tenant-scoped,
already deployed, no new package), but three of the nine catches cannot be worked around from the
outside:

- catch 1 forces us to encode payloads to dodge a parsing bug,
- catch 5 rules out replay and so caps what resume can ever offer,
- **catch 7 is decisive**: with no `Unsubscribe` on the interface and a process-wide static multiplexer,
  every shell reload leaks a handler bound to a dead shell — and OrchardCore reloads shells on ordinary
  settings changes.

Define our own `IMediaChangeBackplane` and implement it over `IConnectionMultiplexer.GetSubscriber()`
(still supplied by `OrchardCore.Redis`, still no new package). We then own channel naming — tenant
prefix included, mirroring `RedisBus` — raw payloads with no encoding workaround, `UnsubscribeAsync` on
`TerminatedAsync`, subscribe-retry, and a clean path to Redis Streams later if replay becomes a
requirement. Roughly 60 lines against a wrapper of similar size that would still carry catches 1, 5 and 7.

Submit the `Split('/', 2)` fix upstream regardless: it is a latent bug for **any** future `IMessageBus`
caller with structured payloads, not just this one.

### 5.5 Heartbeat + shutdown

A single `BackgroundService` per tenant calls `registry.Heartbeat()` every 20s (R4), emitting a
`keepalive` frame so idle proxies do not cull the connection. On shell shutdown, complete all channels
so streams close cleanly instead of hanging until timeout.

### 5.6 Auth

Unchanged in substance from SignalR. The bearer token arrives as `?access_token=…`, promoted to an
`Authorization` header by the existing middleware — **generalize the path check** at
[Startup.cs:714-724](src/OrchardCore.Modules/OrchardCore.Media/Startup.cs#L714-L724) from `/hubs/media`
to `api/realtime/*`, and move it out of the Media module into the Realtime one, since it is no longer
media-specific. Cookie mode needs nothing. Cross-origin standalone needs the stream route added to the
tenant's CORS policy alongside the existing Media API entries.

Authentication proves *who*; **authorization is per topic** via `IRealtimeTopicAuthorizer` (§4.2), so a
module cannot accidentally expose another module's topics.

### 5.7 The Media consumer — what this module actually ships

The whole point of the split: with the core in place, Media's real-time support is small.

```csharp
// OrchardCore.Media/Realtime/MediaRealtimeEventHandler.cs   : IMediaEventHandler   (§5.4)
// OrchardCore.Media/Realtime/MediaTopicAuthorizer.cs        : IRealtimeTopicAuthorizer
//     Handles(topic)      => topic.StartsWith("media:")
//     AuthorizeAsync(...) => ManageMedia, plus a Secure Media path check for media:dir:* (§10.7)
```

That is the reference example other modules copy — one event handler that publishes, one authorizer that
guards the namespace. Ship it in the docs as the worked example (§1.5), because a capability nobody can
see how to adopt will not be adopted.

---

## 6. Client implementation

### 6.1 `RealtimeClient` in bloom — replaces `signalr-app.ts`

New `.scripts/bloom/services/realtime/realtime-client.ts` (~120 lines, no dependency), built on `fetch` +
`ReadableStream` rather than native `EventSource`. **Shared across features**, like the server side: one
connection per tab, a singleton in the bloom layer, with `subscribe(topic, handler)` /
`unsubscribe(topic)` that call `POST api/realtime/subscriptions` and demultiplex by the `event:` field.
The Media Gallery and Notifications are both consumers of this one object.

**Why not native `EventSource`?** It cannot set request headers, and — more importantly — on reconnect it
re-requests the **original URL**, so a bearer token embedded in the query string comes back **expired**.
Today's SignalR client avoids this with an `accessTokenFactory` invoked per reconnect
([SignalR.ts:21-29](src/OrchardCore.Modules/OrchardCore.Media/Assets/media-gallery/src/services/SignalR.ts#L21-L29)).
A fetch reader restores that behaviour: fresh token per attempt, real `Authorization` header, and one
code path for both auth modes. The costs are reconnect and `Last-Event-ID` tracking, ~30 lines.

Responsibilities: connect with token or `credentials: 'include'`; parse `id:`/`event:`/`data:` frames;
reconnect with **full jitter** backoff (R6); resend `Last-Event-ID`; expose `on(event, handler)`;
`close()` on unmount.

### 6.2 `useMediaChangeStream()` — replaces `useSignalR()`

Handles the six actions by patching the store via the existing `FileLibraryManager` mutators
(`setFileItems`, `invalidateFileCache`, the tree helpers) instead of refetching:

| Action | Client behaviour |
| --- | --- |
| `fileUploaded`, `fileCopied` | splice `Item` into `fileItems` if the topic matches the current view |
| `fileDeleted` | filter it out |
| `fileMoved` | remove from old path's view, insert into new if visible |
| `directoryCreated`, `directoryDeleted` | patch the folder tree in place |
| `resync` (R3) | the only case that calls `loadDirectoryFiles(..., true)` |

On directory navigation, swap topics via `realtime.subscribe("media:dir:new") / unsubscribe(old)` —
**without** closing the stream, so the notifications feed sharing it is unaffected (§4.2). Debounce
~150 ms so rapid clicking doesn't churn subscription calls. Respect `pendingDeletes` exactly as today
([FileLibraryManager.ts:39-46](src/OrchardCore.Modules/OrchardCore.Media/Assets/media-gallery/src/services/FileLibraryManager.ts#L39-L46)).

Verify in the spike: the **HTTP/1.1 six-connections-per-origin** ceiling with several admin tabs open,
and that HTTP/2 lifts it. Note the shared stream makes this one connection *per tab* rather than one per
feature — the reason §4.2 chose multiplexing.

### 6.3 Config and props

`hubUrl` → `eventsUrl` and `signalrEnabled` → `realtimeEnabled` in
[RuntimeConfig.ts](src/OrchardCore.Modules/OrchardCore.Media/Assets/media-gallery/src/services/RuntimeConfig.ts),
[standalone.ts](src/OrchardCore.Modules/OrchardCore.Media/Assets/media-gallery/src/standalone.ts),
[App.vue](src/OrchardCore.Modules/OrchardCore.Media/Assets/media-gallery/src/App.vue) and
[Views/Admin/Index.cshtml](src/OrchardCore.Modules/OrchardCore.Media/Views/Admin/Index.cshtml) — **keeping
the old names as accepted aliases**, since `signalrEnabled` is public API in standalone `config.json`.

Rebuild with root `yarn build -n media-gallery` under the pinned `.node-version` (the site serves the
`.min` assets).

---

## 7. Feature and manifest

### 7.1 SSE is opt-in, exactly like SignalR

**Real-time stays an opt-in capability, and "no real-time at all" remains a first-class supported
state.** That is how the module behaves today — with `OrchardCore.Media.SignalR` disabled, the gallery
works fine and simply doesn't live-update ([App.vue:465](src/OrchardCore.Modules/OrchardCore.Media/Assets/media-gallery/src/App.vue#L465)
never calls the hook) — and nothing here changes it.

New in [Manifest.cs](src/OrchardCore.Modules/OrchardCore.Media/Manifest.cs), all **disabled until an
operator enables them**, none auto-enabled by setup recipes:

In the new `OrchardCore.Realtime` module (platform-level, §4):

- `OrchardCore.Realtime.Sse` — the in-process SSE stream + subscription endpoints. No packages.
- `OrchardCore.Realtime.Redis` — Redis backplane for multi-instance fan-out, over
  `IConnectionMultiplexer` from `OrchardCore.Redis` (see §5.4.1 for why not `IMessageBus`).
  Transport-agnostic.
- *(later, §3.4.4)* `OrchardCore.Realtime.Mercure` / `.WebPubSub` — external-hub transports.

In `OrchardCore.Media`:

- `OrchardCore.Media.Realtime` — the Media consumer (§5.7): event handler + topic authorizer. Depends on
  `OrchardCore.Realtime.Sse`.

In `OrchardCore.Notifications` (§4.3, the second consumer):

- `OrchardCore.Notifications.Realtime` — a "Real-time" `INotificationMethodProvider`.

The three `OrchardCore.Media.SignalR*` features stay — **not deprecated**, but redescribed as the
transport for deployments using Azure SignalR Service (§3.4.5).

"SSE is the default" in this plan means **recommended, and preferred when more than one transport is
enabled** — never "switched on for you".

### 7.2 Resolving the active transport

With several optional transports, exactly one must win at runtime. Registration order in the shell
container decides it: SSE > SignalR, with an `IMediaRealtimeTransport` marker resolved once at startup.

Replace the `IHubContext<MediaHub>` presence probes at
[AdminController.cs:49](src/OrchardCore.Modules/OrchardCore.Media/Controllers/AdminController.cs#L49) and
[Options.cshtml:10](src/OrchardCore.Modules/OrchardCore.Media/Views/Admin/Options.cshtml#L10) with that
resolution, and surface it rather than a boolean:

- `MediaIndexViewModel.SignalrEnabled` (bool) → `RealtimeTransport` (string: `none` | `sse` | `signalr`),
  keeping the old property as an obsolete alias computed as `RealtimeTransport != "none"`.
- The Razor attribute `signalr-enabled="true|false"` → `realtime-transport="…"`, old name still accepted
  (§6.3).
- [Options.cshtml:137-140](src/OrchardCore.Modules/OrchardCore.Media/Views/Admin/Options.cshtml#L137-L140)
  shows the active transport by name instead of a SignalR tick, so an operator with both features on can
  see which one is actually serving.

**No auto-disable.** Enabling SSE does not turn SignalR off — both may run during migration, the client
picks SSE, and the release notes tell operators to disable the SignalR features once verified. Log an
informational message at startup when more than one transport is registered, naming the winner.

### 7.3 Client behaviour when real-time is off

`realtime-transport="none"` must be a clean no-op: no stream opened, no polling fallback, no console
noise — identical to today's `signalrEnabled === "false"` path. Cover it in the Vitest suite, since it
is the configuration most OrchardCore sites will actually run.

---

## 8. Testing

- **xUnit** — registry: subscribe/unsubscribe, topic routing, bounded-channel overflow → `resync`,
  empty-bucket pruning, concurrent publish/unsubscribe. Endpoint: authorization (401/403), stream
  terminates on `RequestAborted`, `Last-Event-ID` resume.
- **Vitest** — `SseClient` frame parsing (partial frames, multi-line `data:`, comments), jittered
  reconnect, token refresh per attempt; `useMediaChangeStream` store-patching for all six actions plus
  `resync`. Replaces [SignalR.spec.ts](src/OrchardCore.Modules/OrchardCore.Media/Assets/media-gallery/src/services/__tests__/SignalR.spec.ts).
  Remember to unmount wrappers so timers don't outlive the DOM.
- **Playwright** — two browser contexts: upload in one, assert the other's grid updates **without** a
  `GetDirectoryContent` request (assert on network, which directly proves the amplification fix).
  Extend the existing media fixtures; add a Redis two-instance variant mirroring the current
  `media-tus-redis` recipes.
- **A load check** reusing the §11 harness: assert storage-op count per event stays O(1) as N grows.
- **Security tests** — unauthorized topics are silently dropped rather than 403'd (§4.2); a topic no
  authorizer claims is denied; and, if Phase 7 lands, the stream endpoint emits **no** `Grip-*` headers
  when `Grip-Sig` is missing or invalid (§3.4.9 — channel names leak folder paths).

---

## 9. Demoting SignalR to opt-in (not deleting it)

Per §3.4.4, SignalR **stays** as an alternative transport so deployments that need Azure SignalR
Service's connection offload keep it. What changes:

- SSE becomes the **recommended opt-in** transport: `OrchardCore.Media.Realtime.Sse` is what the docs
  tell new sites to enable, and the client prefers it when both are active (§7.1). Neither transport is
  enabled automatically.
- `MediaSignalREventHandler` is reimplemented on top of the shared notifier/registry contract (§5), so
  levers 1–3 — payload-carrying events, topic scoping, coalescing — apply to **both** transports. The
  scaling fix must not be SSE-only.
- The three `OrchardCore.Media.SignalR*` features and the `OrchardCore_Media_SignalR` configuration
  section stay and stay documented, described as "for deployments using Azure SignalR Service".
- `@microsoft/signalr` moves from an unconditional bundle dependency to a **lazily imported chunk**
  loaded only when the SignalR transport is the active one — so the default install still sheds the
  bundle weight (§1.2) without losing the capability.
- `Microsoft.Azure.SignalR` and `Microsoft.AspNetCore.SignalR.StackExchangeRedis` move out of the
  module's unconditional [package references](src/OrchardCore.Modules/OrchardCore.Media/OrchardCore.Media.csproj#L46-L47)
  into the feature that actually needs them.

Revisit full removal only if telemetry or maintainer input shows nobody uses the Azure backplane.

---

## 10. Known losses and risks

1. **No *Azure-native* SSE offload service exists** (Azure Web PubSub is WebSocket/MQTT). But the
   capability is not lost: external SSE hubs — Mercure, Centrifugo, Nchan, Fastly Fanout/Pushpin, Ably —
   provide the same "browsers connect to the hub, not to your app" architecture over an open protocol
   (§3.4.4). Combined with levers 1–3 removing the load that actually hurts (§3.4.1), admin-scale
   connection counts (§3.4.2), and SignalR retained as an opt-in transport (§3.4.5), this is no longer
   a capability gap — only a "not on day one" item.
2. **Server-side SSE is new in .NET 10** — little community mileage. §5.3's raw-vs-JSON-quoted check is
   the gating unknown.
3. **HTTP/1.1 connection ceiling** per origin; confirm HTTP/2 in the deployment guidance.
4. **Proxy buffering** of `text/event-stream` (nginx `X-Accel-Buffering`, some ANCM/IIS setups).
5. **We now own reconnect and framing** on the client. Bounded — ~120 lines with tests — but real.
6. **Payload-carrying events widen the contract.** `Item` must stay consistent with
   `GetDirectoryContent`'s DTO; share `CreateFileResult` rather than duplicating shaping logic.
7. **Secure Media.** Verify events for paths a user cannot view are not delivered — topic scoping must
   not become an information leak; the `ManageMedia` check at subscribe time is necessary but confirm it
   is sufficient against `SecureMediaPermissions`.

---

## 11. Phases

| Phase | Content | Effort | Ships independently? |
| --- | --- | --- | --- |
| **0** | Measure: instrumentation + harness (investigation §3) | 1 d | n/a |
| **1** | Levers 1–3 on **SignalR as it stands** — payload patching, topic scoping, coalescing. Re-measure. | 2 d | **Yes** — the scaling fix, no transport change |
| **2** | `OrchardCore.Realtime` core: abstractions, registry, stream + subscription endpoints, heartbeat, metrics | 3 d | No |
| **2b** | Media consumer (§5.7): event handler + topic authorizer | 0.5 d | After 2 |
| **3** | Client `RealtimeClient` in bloom + `useMediaChangeStream`, config aliases | 2 d | With 2 |
| **4** | `IMessageBus` backplane + two-instance functional test. **Budget for §5.4.1** — payload encoding, subscribe-retry, per-tenant dispatch | 2 d | With 2–3 |
| **5** | Docs (incl. the §5.7 worked example for module authors), release notes | 1 d | With 2–4 |
| **5b** | **Second consumer: `OrchardCore.Notifications.Realtime`** (§4.3) — real-time notification method + toast/badge client. Proves the abstraction isn't media-shaped | 2 d | After 3 |
| **6** | Demote SignalR to opt-in: lazy-load its client chunk, move its packages into its own feature (§9) | 1 d | Yes |
| **7** *(optional, later)* | External-hub offload — **`OrchardCore.Realtime.Mercure`** (decided in §3.4.8b), deployed same-origin at `/.well-known/mercure`: hub notifier publishing by form POST, `POST api/realtime/hub-token` minting topic-scoped short-lived JWTs validated via JWKS, cookie for the cookie scheme. `…Pushpin` (GRIP) and `…WebPubSub` as sibling adapters. | 3 d | Yes |

**Phase 1 is the one that fixes scaling.** Phases 2–5 are what remove the dependency. Shipping them
separately keeps the benefit attribution honest and gives the fix to users sooner.

---

## 12. Open questions

Hub selection (§3.4.8b) and transport selection (§1.1) are **closed**. What remains:

- [x] ~~Does `SseItem<string>` write raw or JSON-quoted data?~~ **Avoided** — §3.2a writes pre-serialized
      UTF-8 bytes through `SseFormatter.WriteAsync`'s custom-formatter overload, so the ambiguity is moot.
      Nothing gates Phase 2 any more.
- [ ] Topic granularity: exact directory only, or prefix subscriptions for tree views?
- [ ] Should `resync` carry the topic so a client can refetch just one folder?
- [ ] Ring-buffer depth for `Last-Event-ID` resume — or skip resume in v1 and always `resync`?
- [ ] Confirm with maintainers that retaining SignalR as an opt-in transport (§3.4.5) is preferred over
      eventual removal — this is a two-transport maintenance commitment.
- [ ] Measure real per-connection memory for SSE vs. SignalR at N = 100 / 1,000 / 10,000 (§3.4.2), so the
      "you don't need connection offload" claim rests on numbers.
- [ ] **Phase 0 measurements have not run** (no tenant available). Every scaling claim here rests on the
      static analysis in §3.1 until they do. If they show the bottleneck is per-connection cost rather
      than amplification, D2's ordering flips — SSE would precede the fan-out fixes.
- [ ] Mercure spike, before committing Phase 7: same-origin deployment behind the tenant's reverse proxy
      (`flush_interval -1` on Caddy, buffering off on nginx), cookie minting in cookie mode, JWKS
      validation against OrchardCore's OpenIddict keys, and `Last-Event-ID` replay across a hub restart.
