# Real-time baseline harness (Step 1)

Measures the Media Gallery's SignalR fan-out amplification, per
[media-gallery-realtime-transport-plan.md](../../media-gallery-realtime-transport-plan.md) §3.

**Spike code. Not for merge.**

## What it measures

One media change today costs:

```
N clients  ×  1 GetDirectoryContent request  ×  (3 + F) file-store operations
```

- `N` — every connected client, because [MediaSignalREventHandler](../../src/OrchardCore.Modules/OrchardCore.Media/Hubs/MediaSignalREventHandler.cs)
  sends to `Clients.All`.
- `1 request` — because `loadDirectoryFiles(..., true)` refetches even on a cache hit
  ([FileLibraryManager.ts:350-370](../../src/OrchardCore.Modules/OrchardCore.Media/Assets/media-gallery/src/services/FileLibraryManager.ts#L350-L370)).
- `3 + F` — `GetDirectoryInfoAsync` + `GetDirectoriesAsync` + `GetFilesAsync` + one
  `GetDirectoriesAsync` per subfolder for the `HasChildren` probe
  ([MediaEndpointHelpers.cs:94-98](../../src/OrchardCore.Modules/OrchardCore.Media/Endpoints/Api/MediaEndpointHelpers.cs#L94-L98)).

The harness confirms `N` and `3 + F` empirically instead of by reading code.

## Prerequisites

- A running OrchardCore tenant with the **Media** and **OrchardCore.Media.SignalR** features enabled.
- Credentials with `ManageMedia`.
- Node (the repo's `@microsoft/signalr` at the root `node_modules` is used — no install needed).

## Getting credentials

**Cookie mode** (default Media API scheme): sign in to the admin, open DevTools → Network → any
`api/media/*` request → copy the entire `Cookie` request header.

**Bearer mode**: DevTools → Network → filter `token` → copy `access_token` from the
`/connect/token` response, or grab the `Authorization` header off an `api/media/*` request.

Reads (`GetDirectoryContent`) need only the credential. Uploads via `--burst` additionally need
antiforgery in cookie mode, so **prefer bearer mode when using `--burst`**; otherwise drive uploads
from the admin UI and let the harness watch.

## Running

Watch mode — trigger changes yourself in the admin UI, Ctrl-C for the summary:

```bash
node spikes/realtime-baseline/harness.mjs \
  --url https://localhost:5001/ \
  --cookie "$COOKIE" \
  --clients 50 \
  --path "" \
  --insecure
```

Automated 200-file burst (bearer mode):

```bash
node spikes/realtime-baseline/harness.mjs \
  --url https://localhost:5001/ \
  --token "$TOKEN" \
  --clients 10 \
  --burst 200 \
  --json burst-n10.json \
  --insecure
```

Transport cost only, no application amplification:

```bash
node spikes/realtime-baseline/harness.mjs --url ... --clients 50 --no-refetch --duration 60
```

`--help` lists every flag.

## The measurement matrix

Run each row and keep the JSON:

| Run | Flags | Question answered |
| --- | --- | --- |
| 1 | `--clients 1 --duration 120` | Per-event cost floor |
| 2 | `--clients 10 --duration 120` | Does the multiplier track N? |
| 3 | `--clients 50 --duration 120` | Multiplier at realistic admin scale |
| 4 | `--clients 10 --burst 200` | Burst behaviour + time to quiesce |
| 5 | `--clients 50 --no-refetch --duration 120` | Transport-only cost (isolates SignalR itself) |
| 6 | Run 3, viewing a folder with many subfolders | Confirms `F` dominates `3 + F` |
| 7 | Run 3, then restart the server | Reconnect storm |

Key output line:

```
  -> events per source change  50.00   <= the fan-out multiplier N
  -> per source change         650.00  <= N x (3 + F)
```

## Optional: exact server-side counts

`3 + F` is derived from the response's folder count. To count **actual** `IMediaFileStore` calls:

1. Copy `instrumentation/MediaFileStoreCallCounter.cs` into
   `src/OrchardCore.Modules/OrchardCore.Media/`.
2. In [Startup.cs](../../src/OrchardCore.Modules/OrchardCore.Media/Startup.cs), at the **end** of the
   main `MediaStartup.ConfigureServices`, add:
   ```csharp
   services.AddMediaFileStoreCallCounter();   // using OrchardCore.Media.Spike;
   ```
   It wraps whichever `IMediaFileStore` descriptor is already registered, so it must run last.
3. In the same class's `Configure`, add:
   ```csharp
   routes.MapMediaFileStoreCounters();
   ```
4. Then around each run:
   ```bash
   curl -k -X POST https://localhost:5001/api/media/_spike/counters/reset
   # ... run the harness ...
   curl -k https://localhost:5001/api/media/_spike/counters | jq
   ```

The counter endpoints are **anonymous** for convenience — local dev only, and revert all three edits
when the measurements are done.

## Recording results

Write findings to `docs/spikes/realtime-baseline.md` (or the §3 section of the investigation plan) with
the numbers table and an explicit verdict: is the bottleneck per-connection transport cost, or
application-level amplification? That verdict decides whether the SSE work in
[media-gallery-sse-implementation-plan.md](../../media-gallery-sse-implementation-plan.md) is Phase 1
(fan-out fixes) only, or the full transport swap.
