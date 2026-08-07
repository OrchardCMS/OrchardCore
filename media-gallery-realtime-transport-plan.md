# Media Gallery real-time transport — investigation plan

**Goal:** determine whether the Media Gallery's SignalR-based real-time updates should be replaced by
gRPC (gRPC-Web) or WebTransport, and if so, on what migration path.

**Status:** investigation only. No production code changes until the decision gate in §6 is passed.

**Branch:** `skrypt/media-gallery-realtime-transport` (off `main` @ `5c60562e93`).

---

## 1. What exists today

The feature is small and one-directional. Full inventory:

**Server**

| File | Role |
| --- | --- |
| [Hubs/MediaHub.cs](src/OrchardCore.Modules/OrchardCore.Media/Hubs/MediaHub.cs) | Empty hub. No client-callable methods. `[Authorize(MediaApiConstants.AuthorizationPolicyName)]` + a `ManageMedia` check in `OnConnectedAsync` that aborts unauthorized connections. |
| [Hubs/MediaSignalREventHandler.cs](src/OrchardCore.Modules/OrchardCore.Media/Hubs/MediaSignalREventHandler.cs) | `IMediaEventHandler` that broadcasts `MediaChanged` to `Clients.All` on 6 events: `fileUploaded`, `fileDeleted`, `directoryCreated`, `directoryDeleted`, `fileMoved`, `fileCopied`. Payload is `{ action, path, newPath? }`. |
| [Startup.cs:693-728](src/OrchardCore.Modules/OrchardCore.Media/Startup.cs#L693-L728) | `MediaSignalRStartup` — `AddSignalR()`, maps `/hubs/media`, and a middleware that promotes the `access_token` query param to an `Authorization` header for the hub path (ordered before `UseAuthentication`). |
| [Startup.cs:730-794](src/OrchardCore.Modules/OrchardCore.Media/Startup.cs#L730-L794) | Azure SignalR and Redis backplane startups, driven by `OrchardCore_Media_SignalR:ConnectionString` / `OrchardCore_Redis:Configuration`. Missing connection string → warning, falls back to in-memory. |
| [Manifest.cs:97-126](src/OrchardCore.Modules/OrchardCore.Media/Manifest.cs#L97-L126) | Three features: `OrchardCore.Media.SignalR`, `.SignalR.Azure`, `.SignalR.Redis`. |
| [Controllers/AdminController.cs:49](src/OrchardCore.Modules/OrchardCore.Media/Controllers/AdminController.cs#L49) | Presence probe: `GetService<IHubContext<MediaHub>>() is not null` → `MediaIndexViewModel.SignalrEnabled`. Same probe in [Views/Admin/Options.cshtml:10](src/OrchardCore.Modules/OrchardCore.Media/Views/Admin/Options.cshtml#L10). |

**Client**

| File | Role |
| --- | --- |
| [services/SignalR.ts](src/OrchardCore.Modules/OrchardCore.Media/Assets/media-gallery/src/services/SignalR.ts) | `useSignalR()` — builds the connection, subscribes to `MediaChanged`, and on every message calls `loadDirectoryFiles(selectedDirectory, force: true)`. The message body is logged but **not** used to patch state. |
| [.scripts/bloom/services/signalr/signalr-app.ts](.scripts/bloom/services/signalr/signalr-app.ts) | Shared `SignalRApp` wrapper: transport selection, `accessTokenFactory`, `withAutomaticReconnect([0,3s,5s,10s,15s,30s])`, incoming-frame interception for the event bus. |
| [.scripts/bloom/services/signalr/eventbus.ts](.scripts/bloom/services/signalr/eventbus.ts), [useSignalRService.ts](.scripts/bloom/services/signalr/useSignalRService.ts) | mitt-based logging/event bus. `useSignalRService.ts` is currently unused by the gallery. |
| [App.vue:465-466](src/OrchardCore.Modules/OrchardCore.Media/Assets/media-gallery/src/App.vue#L465-L466) | `if (props.signalrEnabled === "true") useSignalR();` — the single call site. |
| [services/RuntimeConfig.ts](src/OrchardCore.Modules/OrchardCore.Media/Assets/media-gallery/src/services/RuntimeConfig.ts) | `hubUrl`: `/hubs/media` embedded, `${orchardBaseUrl}hubs/media` standalone; `signalrEnabled` flag. |
| [services/__tests__/SignalR.spec.ts](src/OrchardCore.Modules/OrchardCore.Media/Assets/media-gallery/src/services/__tests__/SignalR.spec.ts) | Unit coverage of the wiring (mocked `SignalRApp`). |
| [standalone.ts:61-95](src/OrchardCore.Modules/OrchardCore.Media/Assets/media-gallery/src/standalone.ts#L61-L95), [standalone/config.example.json](src/OrchardCore.Modules/OrchardCore.Media/Assets/media-gallery/standalone/config.example.json) | Cross-origin standalone host; real-time is opt-in there because it needs CORS on `/hubs/media`. |

**Docs:** [MediaGallery.md:140-207](src/docs/reference/modules/Media/MediaGallery.md#L140-L207) (feature, backplanes, multi-instance guidance).

**Dependencies:** `Microsoft.AspNetCore.SignalR.StackExchangeRedis`, `Microsoft.Azure.SignalR` in
[OrchardCore.Media.csproj](src/OrchardCore.Modules/OrchardCore.Media/OrchardCore.Media.csproj#L46-L47);
`@microsoft/signalr ^8.0.0` in [.scripts/bloom/package.json](.scripts/bloom/package.json).

### 1.1 What the feature actually needs

Distilled from the above — this is the requirement set any replacement must satisfy:

1. **Server → client only.** Zero client→server RPC. No request/response, no bidirectional streaming.
2. **Broadcast to all authorized clients.** No groups, no per-user targeting, no per-directory topics today.
3. **Tiny, infrequent payloads.** 2–3 string fields, emitted at human editing rates.
4. **The payload is advisory.** The client discards it and re-fetches the current directory. Delivery
   ordering and exactly-once do not matter; *at-least-once, eventually* is the real contract.
5. **Two auth modes.** Bearer (silent PKCE, token renewed per (re)connect) or admin cookie —
   see [MediaApiSettings.cs](src/OrchardCore.Modules/OrchardCore.Media/MediaApiSettings.cs). Plus a
   `ManageMedia` authorization check at connect time.
6. **Automatic reconnect** with backoff, and token refresh on each reconnect attempt.
7. **Multi-instance fan-out** via a pluggable backplane (Redis today, Azure SignalR as a managed option).
8. **Cross-origin (standalone) support**, gated on CORS.
9. **Multi-tenancy.** Runs per shell; the endpoint lives under the tenant prefix.
10. **Opt-in feature module** whose presence the admin UI can detect and display.

> The honest framing for this investigation: requirements 1–4 describe the *weakest* real-time
> requirement there is. gRPC's differentiators (typed contracts, cross-language codegen, streaming
> ergonomics) and WebTransport's (datagrams, multiplexed streams, no head-of-line blocking) mostly
> address problems this feature does not have. The investigation must therefore be explicit about
> what non-functional goal we are actually buying — see §2.

---

## 2. Motivation, and what it implies (Workstream 0)

**Stated driver:** SignalR is too bloated, and scales badly according to reports from the field.

These are two separate complaints with different root causes and different fixes. Conflating them is the
main way this investigation goes wrong, so they are tracked separately from here on.

### 2.1 "Bloated" — a real, measurable, transport-fixable complaint

Concretely this is:

- `@microsoft/signalr ^8.0.0` in the client bundle ([.scripts/bloom/package.json](.scripts/bloom/package.json)),
  shipped in `media2.min.js` for every admin page load that mounts the gallery.
- Two server NuGet packages, `Microsoft.AspNetCore.SignalR.StackExchangeRedis` and `Microsoft.Azure.SignalR`
  ([OrchardCore.Media.csproj:46-47](src/OrchardCore.Modules/OrchardCore.Media/OrchardCore.Media.csproj#L46-L47)),
  referenced unconditionally by the module.
- Protocol overhead: a `/negotiate` round-trip before the socket, handshake frames, and the hub-protocol
  envelope (`{type, target, arguments}` + `0x1e` separators) wrapped around a 3-field payload.
- The wrapper code we already maintain to work around the client
  ([signalr-app.ts](.scripts/bloom/services/signalr/signalr-app.ts) reaches into a private
  `processIncomingData` / `_processIncomingData` that was renamed between v7 and v10).

A transport swap genuinely addresses all of this — **and so does SSE**, at a lower cost than either
candidate. §3 must produce the actual numbers so "bloated" stops being a vibe: if the bundle delta turns
out to be small, this half of the motivation weakens considerably.

### 2.2 "Scales badly" — decompose before believing a transport fixes it

**Do this before any spike.** Ask what specifically was reported, and by whom:

- [ ] **Source the reports.** GitHub issues, forum/Discussions threads, your own load tests, or a customer
      deployment? Link them here. Without a source we cannot tell which of the causes below is at play,
      and we risk swapping transports to fix something that isn't the transport.
- [ ] **Which limit was hit?** Concurrent connections per instance, server memory/CPU at idle, backplane
      throughput, Azure SignalR Service unit cost/limits, reconnect storms after a deploy, or latency
      under load? Each points somewhere different.
- [ ] **Was a backplane configured?** A common report is "updates don't reach other instances", which is a
      *missing backplane*, not a scaling failure — see the warning path at
      [Startup.cs:757-759](src/OrchardCore.Modules/OrchardCore.Media/Startup.cs#L757-L759) and
      [MediaGallery.md:173](src/docs/reference/modules/Media/MediaGallery.md#L173).

Then weigh the three plausible causes against what a transport swap can actually do:

| Suspected cause | Would gRPC / WebTransport fix it? |
| --- | --- |
| **Broadcast amplification.** [MediaSignalREventHandler](src/OrchardCore.Modules/OrchardCore.Media/Hubs/MediaSignalREventHandler.cs) sends to `Clients.All` on every media event, and each receiving client answers with a **full directory refetch** ([SignalR.ts:34](src/OrchardCore.Modules/OrchardCore.Media/Assets/media-gallery/src/services/SignalR.ts#L34)) — regardless of whether the changed path is in the directory it is viewing. N clients × every event = N API calls + N storage listings. A bulk upload of 200 files fans out to 200 × N refetches. | **No.** This is application design. The same code over gRPC or WebTransport produces the identical stampede. Fixes: scope events to interested clients (per-directory topics), use the payload to patch the store instead of refetching, and coalesce/debounce bursts. |
| **Backplane fan-out.** Redis pub/sub delivers every message to every server, which delivers to every connection. Cost grows with instances × connections. | **No.** Any replacement needs the same Redis pub/sub (§4.1, §5) and gets the same characteristics — except we now own and maintain that code, where SignalR gave it to us. |
| **Per-connection cost.** Idle WebSocket connections holding buffers/state on the server. | **Partially / marginally.** SSE and raw WebSocket are leaner than SignalR's per-connection overhead; gRPC-Web streams are comparable. Measure it (§3) — this is the one place a transport swap moves the needle, and it is likely the smallest of the three effects. |

**Working hypothesis to disprove:** the reported scaling problem is broadcast amplification plus
refetch stampede, both of which live in ~40 lines of application code and are fixable *without*
changing transport — while the bloat complaint is real and is best answered by SSE. If the spikes
support that, the recommendation is "fix the fan-out, drop the SignalR dependency for SSE", and gRPC/
WebTransport lose on cost rather than on capability.

**Output:** fill in the report sources and the "which limit" answer above, then confirm or reject the
hypothesis in §6. If the hypothesis holds, the §5 work becomes the primary deliverable, not a side effect.

---

## 3. Baseline measurement (Workstream 1)

**Time-box: 1 day. Runs in parallel with §4.**

Everything else is scored against this, so capture it before touching alternatives.

- [ ] Stand up the gallery locally with `OrchardCore.Media.SignalR` enabled, in both auth modes.
- [ ] Measure: bytes on the wire for connect (negotiate + handshake) and per event; time from server-side
      `IMediaEventHandler` invocation to client `loadDirectoryFiles` call; reconnect time after a
      server restart; behaviour when the bearer token expires mid-session.
- [ ] Measure the client bundle contribution of `@microsoft/signalr` (build with and without, compare
      `media2.min.js`; remember the minified-asset build note — root `yarn build -n media-gallery`).
- [ ] Measure server memory/connection cost at 50 and 500 idle connections (a simple load script is enough).
      This is the one number that a transport swap can improve directly (§2.2, row 3) — get it precise.
- [ ] Repeat the fan-out check with the Redis backplane across two instances.

**Scaling-specific measurements** (these test the §2.2 hypothesis, and are the ones that decide this
investigation):

- [ ] **Amplification.** With N clients connected (N = 1, 10, 50), upload one file and count the resulting
      API requests and storage listings server-side. Expect ~N refetches per event. Confirm the multiplier.
- [ ] **Burst behaviour.** Upload 200 files via TUS with N = 10 connected clients. Record events emitted,
      refetches triggered, wall-clock to quiesce, and whether the admin UI stays responsive. This is the
      most likely shape of the reported failure — capture it carefully.
- [ ] **Irrelevant-event rate.** Fraction of `MediaChanged` events that trigger a refetch of a directory
      the changed path isn't even in. That fraction is pure waste and is removable without changing transport.
- [ ] **Reconnect storm.** Restart an instance with N = 50 connected and measure the reconnect + refetch
      thundering herd against the API.

**Output:** `docs/spikes/realtime-baseline.md` (or a section here) with a numbers table, plus an explicit
verdict on whether the bottleneck is per-connection transport cost or application-level amplification.

---

## 4. Candidate spikes

Each spike is a throwaway branch or a folder under `spikes/`. **None of them touch
`OrchardCore.Media` production code** — they may copy from it.

### 4.1 gRPC / gRPC-Web (Workstream 3) — time-box 3 days

The key constraint to validate up front: **browsers cannot speak native gRPC.** A browser client needs
gRPC-Web (`Grpc.AspNetCore.Web` server-side + `grpc-web` or `@connectrpc/connect-web` client-side), which
supports **server streaming** but not client or bidirectional streaming — acceptable here, since the
feature is server→client only.

Questions to answer:

- [ ] **Contract.** Define `media_events.proto`: a `MediaEvents` service with
      `rpc Subscribe(SubscribeRequest) returns (stream MediaChangedEvent)`. Where does the `.proto` live,
      and is it shipped in the module's NuGet package? How does it version alongside the existing
      OpenAPI surface? Does it duplicate types already generated by NSwag for the Media API?
- [ ] **Hosting inside Orchard.** Can `MapGrpcService<T>()` be called from a `StartupBase.Configure`
      on the tenant's `IEndpointRouteBuilder`, per shell? Does `AddGrpc()` in a shell container conflict
      with other tenants? Does the tenant prefix route correctly? Verify with two tenants.
- [ ] **Transport reality.** gRPC-Web runs over HTTP/1.1 (`grpc-web-text` for browsers). Confirm it
      works behind the existing Orchard middleware pipeline and does not need HTTP/2 over TLS. Check
      `UseGrpcWeb()` ordering relative to Orchard's routing/auth middleware.
- [ ] **Auth.** gRPC-Web can send `Authorization` headers (unlike EventSource/WebTransport), so bearer
      mode should be straightforward — verify. Cookie mode: does the fetch-based transport send cookies,
      and what `credentials` setting is needed for the cross-origin standalone case? Re-verify the
      `ManageMedia` check placement (per-call interceptor vs. connection start).
- [ ] **The hard part — fan-out.** gRPC has **no backplane**. We must build: a per-connection subscriber
      registry (keep the `IAsyncEnumerable`/`Channel` per open stream), plus cross-instance propagation.
      Prototype Redis pub/sub via the existing `OrchardCore.Redis` `IConnectionMultiplexer`. Estimate the
      code we now own that SignalR gave us for free. **There is no managed Azure equivalent** — the
      `OrchardCore.Media.SignalR.Azure` feature has no counterpart and would be dropped.
- [ ] **Reconnect and liveness.** No built-in reconnect. Prototype backoff + token re-acquisition on the
      client. Add keep-alive so idle streams aren't culled by intermediaries; confirm the client detects
      a half-open stream.
- [ ] **Client cost.** Measure the bundle size of `grpc-web` or `@connectrpc/connect-web` + generated
      stubs vs. the `@microsoft/signalr` baseline. Add the codegen step to the assets build
      (`assets-manager`, `yarn build -n media-gallery`) and confirm it doesn't break the pipeline.
- [ ] **Also evaluate Connect-RPC** as a variant: same proto, simpler wire format, server streaming over
      HTTP/1.1, better browser story — but .NET server support is community, not first-party. Note the
      support risk explicitly.

**Exit:** a working spike where a file upload on instance A refreshes a gallery connected to instance B,
in bearer mode, with reconnect after an instance restart. Plus an LOC delta vs. today.

### 4.2 WebTransport (Workstream 4) — time-box 3 days

Validate feasibility **before** writing any application code; this candidate is the one most likely to
fail on infrastructure grounds.

- [ ] **Server support.** Kestrel's WebTransport support is **experimental**, gated behind an
      `AppContext` switch, and requires HTTP/3. Confirm the current status on `net10.0` (the repo's TFM,
      [TargetFrameworks.props](src/OrchardCore.Build/TargetFrameworks.props)) and whether it is still
      preview/unsupported. **If it is still experimental, that is likely a hard stop for a shipped
      OrchardCore feature — record that finding and stop the spike early.**
- [ ] **Runtime prerequisites.** HTTP/3 needs TLS 1.3 and QUIC: `libmsquic` on Linux,
      Windows 11 / Server 2022+ on Windows. Document what OrchardCore hosting docs would have to require.
- [ ] **Deployment reality — the decisive question.** UDP/443 must be reachable end-to-end. Check IIS,
      Azure App Service, Azure Front Door, Cloudflare, nginx, and typical container ingress: which of
      these terminate at HTTP/1.1/HTTP/2 and would silently break real-time updates? Any that do makes
      WebTransport a "when your infrastructure allows it" feature, not a replacement.
- [ ] **Browser support.** Chrome/Edge yes; check current Firefox and **Safari** status. Safari gaps mean
      a fallback transport must ship anyway — which undercuts the "replace SignalR" premise.
- [ ] **Auth.** The browser `WebTransport` constructor accepts **no custom headers**. The bearer token
      must go in the query string (as SignalR does today, cf. the promotion middleware at
      [Startup.cs:714-724](src/OrchardCore.Modules/OrchardCore.Media/Startup.cs#L714-L724)) or in a first
      client-sent frame. Decide which, and check cookie behaviour for the cross-origin standalone case.
- [ ] **Application protocol.** WebTransport gives raw streams/datagrams — we own framing, serialization,
      heartbeat, reconnect, and backpressure. Prototype a minimal length-prefixed JSON frame over a
      unidirectional server→client stream and count the code.
- [ ] **Fan-out.** Same as gRPC: no backplane. Reuse whatever registry §4.1 produces.
- [ ] **Testability.** Can Playwright drive it? Do the functional tests
      ([test/OrchardCore.Tests.Functional](test/OrchardCore.Tests.Functional)) run over HTTP/3 locally?
      If not, what is the CI story?

**Exit:** either a working two-instance demo *plus* a written deployment-constraint matrix, or an
early "not viable yet, here's why" finding.

#### 4.2.1 Counterfactual: would WebTransport win if support were universal?

Worth settling separately, so the verdict doesn't rest on maturity alone — maturity changes, suitability
doesn't. **Assume Kestrel marks it stable, Safari ships it, and every proxy carries HTTP/3. The answer is
still no**, because its advantages are orthogonal to this feature:

| WebTransport's real advantage | Value for Media Gallery events |
| --- | --- |
| Independent multiplexed streams, no head-of-line blocking | One logical channel. No value |
| Unreliable datagrams | Wrong semantics — a dropped `fileDeleted` leaves a stale UI. We'd use reliable streams, i.e. what SSE already gives |
| Bidirectional without a second channel | The only client→server need is "subscribe to directory" — a query param (§5.1) |
| QUIC 0-RTT reconnect, connection migration | An admin at a desktop; a 1–2 RTT reconnect is imperceptible |
| Native binary framing | Payload is small JSON. Base64's 33% on nothing is nothing |
| No 6-connection-per-origin limit | HTTP/2 already solves this for SSE |
| Per-stream backpressure | Handled at the application layer by the bounded channel + `resync` (R3) |

Meanwhile the costs are unchanged by maturity: we own framing, heartbeat, reconnect, resume and
serialization; there is no `Last-Event-ID` equivalent; the browser API cannot set headers; there is no
external-hub ecosystem comparable to Mercure/Fanout/Web PubSub (§3.4.4 of the SSE plan); and functional
testing over HTTP/3 is harder.

WebTransport is a genuinely better **WebSocket**, not a better **SSE**. It wins where you need datagrams,
many concurrent streams, or high-frequency lossy updates — collaborative cursors and presence, live media,
games, telemetry firehoses. If OrchardCore ever builds collaborative content editing, revisit it there.
For "server pushes a handful of small notifications per minute", SSE is the purpose-built tool and stays
the right answer.

One fair counter-argument, recorded: if the goal were *one transport for every future OrchardCore
real-time need* — media events, notifications, collaborative editing on a single connection — generality
would favour WebTransport. That is a platform decision rather than a Media Gallery one, and betting on it
today still means shipping a fallback transport anyway.

### 4.3 ASP.NET Core-native candidates (Workstream 5) — time-box 1 day

Not "controls" — on current evidence the leading candidate is in this group. An investigation that only
compares gRPC and WebTransport picks one of them by construction.

#### 4.3.1 Server-Sent Events — **the front-runner**

The newest first-party answer in .NET, and the closest match to §1.1. Since .NET 9/10, ASP.NET Core has
built-in SSE results: an endpoint returns `IAsyncEnumerable<SseItem<T>>` via
`TypedResults.ServerSentEvents(...)`, so the server side is a minimal-API endpoint sitting next to the
existing [Endpoints/](src/OrchardCore.Modules/OrchardCore.Media/Endpoints/) ones — no new NuGet package.
The repo is already on `net10.0` ([TargetFrameworks.props](src/OrchardCore.Build/TargetFrameworks.props)),
so this is available today.

Why it fits better than either suggested candidate:

- **Zero client dependency.** Native `EventSource`, with automatic reconnect and `Last-Event-ID` resume
  built into the browser. Deletes `@microsoft/signalr` and the private-API wrapper in
  [signalr-app.ts](.scripts/bloom/services/signalr/signalr-app.ts) outright — the whole §2.1 complaint.
- **One-way by design**, which is exactly requirement §1.1.1.
- **Plain HTTP/1.1 chunked `text/event-stream`.** Traverses every proxy, CDN, IIS and App Service that
  already serves the site. No HTTP/3, no QUIC, no UDP, no gRPC-Web codegen step.
- **Auth works with the mechanism already in the codebase.** `EventSource` cannot set headers, so the
  bearer token goes in the query string — precisely what the existing promotion middleware at
  [Startup.cs:714-724](src/OrchardCore.Modules/OrchardCore.Media/Startup.cs#L714-L724) already does for
  SignalR. Cookie mode just works, same-origin. Alternative if we want real headers: a fetch +
  `ReadableStream` reader (~30 lines, reconnect becomes ours).
- **Non-browser clients too.** .NET ships `System.Net.ServerSentEvents.SseParser`, so the standalone app
  or any service can consume the same stream without a proto or a generated client.

**Browser support — the settled part.** `EventSource` is the oldest of the candidates: Opera 9/11 (2006/2011),
Chrome 6 and Safari 5 and iOS Safari 4.2 (2010), Firefox 6 (2011), Android Browser 4.4 (2013). The only gap
was Microsoft's engines — IE never shipped it and legacy EdgeHTML never did either; Chromium-based Edge 79
(January 2020) closed it. Baseline: widely available, no fallback transport required. Compare WebTransport
(§4.2): Chrome/Edge 97 (Jan 2022), Firefox 114 (Jun 2023), Safari historically absent — verify current
Safari status before spending time there.

Questions to answer in the spike:

- [ ] Per-connection server cost vs. the §3 SignalR baseline (the one metric a transport swap moves).
- [ ] **HTTP/1.1 six-connections-per-origin limit** — an SSE stream holds one for the tab's lifetime.
      Confirm behaviour with several admin tabs open, and that HTTP/2 (normal for Orchard behind TLS)
      lifts it. This is SSE's one real gotcha.
- [ ] Response buffering: verify no proxy/IIS/ANCM layer buffers the stream (`X-Accel-Buffering`, etc.).
- [ ] Do `Last-Event-ID` + a small server-side ring buffer give us gap-free resume after a reconnect?
      That would be *better* than today, where a reconnect silently drops events.
- [ ] Tenant routing, `ManageMedia` authorization, and CORS for the standalone origin.

#### 4.3.2 Other native options, scored briefly

- [ ] **Raw WebSockets** (`app.UseWebSockets()`, first-party, no package). Zero deps and bidirectional if
      §5.1's per-directory subscribe needs it — but framing, heartbeat, backoff and reconnect all become
      ours. Effectively "SignalR minus the hardening".
- [ ] **`IAsyncEnumerable<T>` JSON streaming** from a minimal API — simpler than SSE, but no event ids and
      no reconnect semantics. Strictly worse than 4.3.1 for the same effort.
- [ ] **Trim SignalR instead of replacing it.** Cheapest option on the board, and it should be measured
      before any rewrite is funded: switch to the MessagePack hub protocol, force
      `transport: WebSockets` + `skipNegotiation: true` (removes the `/negotiate` round-trip), and move
      `Microsoft.Azure.SignalR` out of the module's unconditional
      [package references](src/OrchardCore.Modules/OrchardCore.Media/OrchardCore.Media.csproj#L46-L47)
      into its own feature assembly. If this recovers most of §2.1's bloat, the case for a rewrite thins.
- [ ] **Keep SignalR as-is.** Cost of doing nothing = 0. The scorecard baseline.

#### 4.3.3 Explicitly out of scope

**Blazor** (Server or WebAssembly) — would require rewriting the Vue gallery; excluded by direction.
**Orleans, Aspire, MassTransit, Rebus, Wolverine, Azure Web PubSub** — server-to-server or managed
messaging, not browser push; Web PubSub is the same shape as Azure SignalR and would re-add a managed
dependency. None address §1.1.

Note: any option relying on a long-lived HTTP response (SSE, gRPC-Web streaming) needs a fan-out registry
and backplane too — build that piece **once** (§5.2) so it isn't re-litigated per candidate.

---

## 5. Fan-out fixes and the abstraction seam (Workstreams 2 and 6)

### 5.1 Transport-independent fan-out fixes — time-box 2 days, do **before** the §6 gate

Given the stated scaling motivation (§2.2), these move from "nice cleanup" to the primary candidate fix.
They land on SignalR as it stands today, and are then re-measured against the §3 baseline:

- [ ] **Scope events to interested clients.** Replace `Clients.All` in
      [MediaSignalREventHandler](src/OrchardCore.Modules/OrchardCore.Media/Hubs/MediaSignalREventHandler.cs)
      with per-directory groups: the client joins the group for the directory it is viewing and leaves on
      navigation. Cuts the multiplier from *N clients* to *clients actually looking at that folder*.
      Note this adds the first client→server call the feature has ever had — it changes §1.1 requirement 1
      and any non-SignalR candidate must then support it.
- [ ] **Use the payload instead of refetching.** [SignalR.ts:34](src/OrchardCore.Modules/OrchardCore.Media/Assets/media-gallery/src/services/SignalR.ts#L34)
      currently calls `loadDirectoryFiles(..., force: true)` on every event. The message already carries
      `action` + `path` (+ `newPath`) — enough to patch the store directly for all six actions. Removes the
      API/storage amplification entirely for the common cases.
- [ ] **Coalesce bursts.** Debounce/batch events server-side (or client-side as a fallback) so a 200-file
      bulk upload produces a bounded number of notifications rather than 200.
- [ ] **Re-measure §3.** If amplification collapses and per-connection cost is the only remaining
      complaint, the transport question is settled by bundle size alone — which favours SSE (§4.3).

### 5.2 The abstraction seam — time-box 1 day, do after §4 spikes

Regardless of the winner, the same refactor makes the transport swappable and shrinks the blast radius:

- [ ] Introduce an `IMediaChangeNotifier` (or Orchard-wide `IRealtimeNotifier`) that
      `MediaSignalREventHandler` becomes a thin implementation of — server side, the transport then plugs in
      per feature.
- [ ] Extract the **subscriber registry + backplane** as transport-agnostic infrastructure, so
      gRPC/SSE/WebTransport implementations only own their wire format. Registry = a `Channel<T>` per open
      connection; the transport just drains it.
- [ ] **Reuse OrchardCore's Redis connection — but not `IMessageBus` itself**; see
      [the SSE plan §5.4.1](media-gallery-sse-implementation-plan.md) for the nine catches that rule it out.
      [`IMessageBus`](src/OrchardCore/OrchardCore.Abstractions/Caching/Distributed/IMessageBus.cs)
      (`SubscribeAsync(channel, handler)` / `PublishAsync(channel, message)`) is implemented by
      [`RedisBus`](src/OrchardCore.Modules/OrchardCore.Redis/Services/RedisBus.cs) — already **tenant-scoped**
      (channel prefix includes `shellSettings.Name`) and already used by
      [`DistributedSignal`](src/OrchardCore/OrchardCore/Caching/Distributed/DistributedSignal.cs). Cross-instance
      fan-out for any non-SignalR transport is then ~10 lines, and
      `Microsoft.AspNetCore.SignalR.StackExchangeRedis` disappears with no replacement package.
      **Gotcha to verify:** `RedisBus.SubscribeAsync` drops messages published by the same host
      ([RedisBus.cs:46](src/OrchardCore.Modules/OrchardCore.Redis/Services/RedisBus.cs#L46)) — fine here,
      since the originating instance notifies its own local subscribers in-process, but the local and
      remote paths must be wired deliberately rather than assumed symmetric.
- [ ] Client side: replace the direct `useSignalR()` call in
      [App.vue:465](src/OrchardCore.Modules/OrchardCore.Media/Assets/media-gallery/src/App.vue#L465) with a
      `useMediaChangeStream()` façade so `SignalR.ts` becomes one of several drivers. Rename the
      `signalrEnabled` prop / `hubUrl` config to transport-neutral names, keeping back-compat aliases
      (`signalrEnabled` is public API on the standalone `config.json`).
- [x] **Feature/manifest story — decided.** Siblings, not a transport setting: each transport is its own
      opt-in feature, none auto-enabled, "no real-time" stays a supported state, and the active transport
      is resolved at startup with SSE preferred. See
      [media-gallery-sse-implementation-plan.md §7](media-gallery-sse-implementation-plan.md).
- [ ] Keep §5.1's improvements in the transport-agnostic layer, so no candidate gets credit for them.

---

## 6. Decision gate

Score each candidate (gRPC-Web, WebTransport, SSE, raw WebSocket, keep SignalR) 1–5 on:

| Criterion | Weight rationale |
| --- | --- |
| Meets §1.1 requirements 1–10 | Non-negotiable; a gap here is disqualifying unless the requirement is explicitly dropped |
| **Measured effect on the reported scaling limit** | The stated driver (§2.2). Must be a §3 number, not an argument |
| **Measured bundle + dependency reduction** | The other stated driver (§2.1). Also a §3 number |
| Deployment/hosting compatibility | IIS, Azure App Service, containers, CDN/reverse proxies |
| Browser support incl. Safari | Determines whether a fallback must ship anyway |
| Auth fit (bearer + cookie, cross-origin standalone) | §1.1.5 |
| Multi-instance fan-out cost | Code we must own vs. get for free |
| Net new code + long-term maintenance for OrchardCore | Reconnect, framing, heartbeat, backpressure |
| Client bundle delta | Measured, not estimated (§3) |
| Server dependency delta | NuGet packages added/removed |
| Testability (unit + Playwright functional) | Existing suites must keep working |
| Contributor familiarity / OrchardCore conventions | SignalR is idiomatic ASP.NET Core |
| Reversibility | Can we back out after one release? |

**Measure candidates against post-§5.1 SignalR, not today's SignalR.** Otherwise the fan-out fixes
inflate whichever candidate ships with them (risk 7).

**Gate:** a candidate ships only if it beats "keep SignalR (with §5.1 applied)" on the weighted score
*and* satisfies all requirements in §1.1 (or the maintainers sign off on dropping a requirement — most likely
`OrchardCore.Media.SignalR.Azure`). Otherwise the outcome is a documented "investigated, staying on
SignalR", plus whatever parts of §5 are worth landing on their own.

---

## 7. Known risks and likely findings

Recorded up front so the spikes can attack them rather than rediscover them:

1. **No backplane in any non-SignalR candidate.** gRPC, WebTransport and SSE all require us to build and
   maintain subscriber tracking plus cross-instance propagation. Mitigated for the Redis case by reusing
   the existing [`IMessageBus`/`RedisBus`](src/OrchardCore.Modules/OrchardCore.Redis/Services/RedisBus.cs)
   (§5.2), which is already tenant-scoped. Not mitigated for Azure: `Microsoft.Azure.SignalR` is a managed
   service with connection offload and has **no counterpart** under any candidate, so
   `OrchardCore.Media.SignalR.Azure` would be lost. Sites relying on it for connection scale would need
   Redis plus enough instances to hold the connections themselves — a genuine downgrade to disclose.
2. **WebTransport is experimental in Kestrel** and needs HTTP/3 end-to-end. Most shared hosting, IIS
   deployments, and several major CDNs/reverse proxies will not carry it. High probability of an early
   "not viable for a shipped feature" verdict.
3. **Safari.** If WebTransport isn't supported, a fallback transport must ship — meaning we'd maintain
   two transports instead of one.
4. **gRPC-Web needs a codegen step** in the assets pipeline (`assets-manager`), which touches the
   minified-asset build the site actually serves.
5. **The problem may not be the transport.** The event payload is discarded and the client refetches; the
   requirement is "at-least-once, eventually, tiny messages". SSE meets it with zero new dependencies.
   Guard against picking the more interesting protocol over the more appropriate one.
6. **Swapping transport can make scaling *worse*.** SignalR's per-connection overhead is real but bounded;
   its backplane, reconnect/backoff, keep-alive, and buffering are battle-tested. A hand-rolled gRPC or
   WebTransport subscriber registry inherits none of that hardening and will have its own connection-leak
   and stampede bugs to find — in production, on other people's sites. If the measured win is a few MB of
   server memory and ~100 KB of bundle, that trade is bad.
7. **The fan-out fixes are transport-independent, and should not be bundled into a transport swap.**
   Per-directory scoping, payload-driven store patching, and burst coalescing (§5) can land on SignalR
   today and would be credited to whichever transport happened to ship with them. Land them first, then
   re-measure — otherwise the scorecard compares "new transport + fixed fan-out" against "SignalR + broken
   fan-out", which is not a transport comparison at all.
8. **Public API churn.** `signalrEnabled` in the standalone `config.json`, the `signalr-enabled` Razor
   attribute, the three feature ids, and the `OrchardCore_Media_SignalR` config section are all public
   surface. Any rename needs a back-compat plan and release notes.
9. **Upstream acceptance.** This is an OrchardCMS repo — replacing an idiomatic ASP.NET Core building
   block with hand-rolled transport plumbing needs maintainer buy-in *before* implementation, not after.
   Consider opening a discussion issue with the §3 baseline + §4 findings.

---

## 8. Sequencing and deliverables

| # | Workstream | Time-box | Depends on |
| --- | --- | --- | --- |
| 0 | Source the scaling reports, decompose the complaint (§2.2) | 0.5 d | — |
| 1 | Baseline + scaling measurements (§3) | 1.5 d | 0 |
| 2 | Fan-out fixes on SignalR, then re-measure (§5.1) | 2 d | 1 |
| 3 | gRPC-Web spike (§4.1) | 3 d | 0 |
| 4 | WebTransport feasibility (§4.2) | 3 d (stop early if experimental) | 0 |
| 5 | Control candidates, esp. SSE (§4.3) | 1 d | 1 |
| 6 | Abstraction seam design (§5.2) | 1 d | 3, 4, 5 |
| 7 | Scorecard + recommendation (§6) | 0.5 d | all |

Workstreams 3, 4 and 5 are independent and can run concurrently with 2.

**Cheapest path to an answer:** 0 → 1 → 2. If the fan-out fixes remove the reported scaling problem, the
remaining question is only "is the SignalR dependency worth its bundle size", which 5 answers in a day —
and the gRPC/WebTransport spikes become optional. Consider running 0–2 first and deciding whether to
fund 3 and 4 at all.

**Deliverables**

1. This document, updated in place with findings per section.
2. Spike code under `spikes/` (or throwaway branches) — explicitly not merged.
3. A numbers table: bundle size, wire bytes, latency, reconnect time, LOC delta, per candidate.
4. A filled scorecard and a one-paragraph recommendation.
5. If "go": a follow-up implementation plan covering §5, feature/manifest migration, docs
   ([MediaGallery.md](src/docs/reference/modules/Media/MediaGallery.md)), release notes, and the
   Playwright functional-test updates.
6. If "no-go": a short write-up so the question isn't reopened from scratch, plus any §5 refactors
   worth landing independently.
