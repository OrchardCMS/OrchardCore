#!/usr/bin/env node
// Media Gallery real-time baseline harness — Step 1 of media-gallery-realtime-transport-plan.md §3.
//
// Simulates N Media Gallery clients connected to the SignalR MediaHub and reproduces exactly what the
// real client does on every MediaChanged event: call GET api/media/GetDirectoryContent for the folder
// it is "viewing" (see FileLibraryManager.loadDirectoryFiles — it refetches even on a cache hit).
//
// It measures the amplification factor: one media change -> N events -> N refetches -> N x (3 + F)
// file-store operations, where F is the subfolder count of the viewed directory.
//
// SPIKE CODE. Not for merge.

import { parseArgs } from "node:util";
import { createRequire } from "node:module";
import { performance } from "node:perf_hooks";

const require = createRequire(import.meta.url);
const signalR = require("@microsoft/signalr");

const { values: opts } = parseArgs({
  options: {
    url: { type: "string" },
    clients: { type: "string", default: "10" },
    path: { type: "string", default: "" },
    extensions: { type: "string", default: "" },
    token: { type: "string" },
    cookie: { type: "string" },
    transport: { type: "string", default: "ws" },
    duration: { type: "string", default: "0" },
    burst: { type: "string", default: "0" },
    "burst-path": { type: "string" },
    "burst-concurrency": { type: "string", default: "4" },
    refetch: { type: "boolean", default: true },
    "no-refetch": { type: "boolean", default: false },
    insecure: { type: "boolean", default: false },
    json: { type: "string" },
    help: { type: "boolean", default: false },
  },
});

if (opts.help || !opts.url) {
  console.log(`
Media Gallery real-time baseline harness

  node harness.mjs --url <base> [options]

Required
  --url <base>             Tenant base URL, e.g. https://localhost:5001/ (trailing slash optional).
                           Include the tenant prefix for non-default tenants.

Auth (pick one; must satisfy the ManageMedia permission)
  --token <jwt>            Bearer token. Use when Media API is in bearer mode.
  --cookie <header>        Raw Cookie header value. Use when Media API is in cookie mode.

Load shape
  --clients <n>            Simulated gallery clients to connect (default 10).
  --path <dir>             Directory each client is "viewing" (default "" = root).
  --extensions <list>      Extensions filter the client sends (default "").
  --no-refetch             Count events only; skip the GetDirectoryContent refetch. Isolates
                           transport cost from application amplification.

Triggering changes
  --burst <n>              Upload n tiny files to --burst-path to generate the burst, then report.
                           Omit to drive changes manually from the admin UI while this runs.
  --burst-path <dir>       Upload target (defaults to --path).
  --burst-concurrency <n>  Parallel uploads (default 4).

Misc
  --duration <seconds>     Stop after n seconds (default 0 = run until Ctrl-C).
  --transport <ws|sse|lp>  SignalR transport (default ws).
  --insecure               Accept self-signed dev certificates.
  --json <file>            Also write the summary as JSON.

Examples
  # Watch amplification while you upload from the admin UI:
  node harness.mjs --url https://localhost:5001/ --cookie "$COOKIE" --clients 50 --path "" --insecure

  # Automated 200-file burst with 10 clients:
  node harness.mjs --url https://localhost:5001/ --token "$TOKEN" --clients 10 --burst 200 --insecure
`);
  process.exit(opts.url ? 0 : 1);
}

if (opts.insecure) {
  process.env.NODE_TLS_REJECT_UNAUTHORIZED = "0";
}

const baseUrl = opts.url.endsWith("/") ? opts.url : opts.url + "/";
const clientCount = Number.parseInt(opts.clients, 10);
const burstCount = Number.parseInt(opts.burst, 10);
const burstConcurrency = Number.parseInt(opts["burst-concurrency"], 10);
const durationSec = Number.parseInt(opts.duration, 10);
const doRefetch = opts.refetch && !opts["no-refetch"];
const viewPath = opts.path;
const burstPath = opts["burst-path"] ?? viewPath;

if (!opts.token && !opts.cookie) {
  console.error("Warning: neither --token nor --cookie supplied. Expect 401s unless the site is open.\n");
}

const authHeaders = () => {
  const h = {};
  if (opts.token) h.Authorization = `Bearer ${opts.token}`;
  if (opts.cookie) h.Cookie = opts.cookie;
  return h;
};

// ---------------------------------------------------------------------------- metrics

const metrics = {
  startedAt: null,
  connected: 0,
  connectFailures: 0,
  eventsReceived: 0,
  eventsByAction: new Map(),
  refetches: 0,
  refetchFailures: 0,
  refetchLatencies: [],
  bytesFetched: 0,
  // Distinct source changes, keyed by the action+path the server broadcast. One source change
  // should produce exactly `clients` events; this proves the fan-out multiplier.
  sourceChanges: new Map(),
  folderCount: null, // F, from the first successful refetch
  firstEventAt: null,
  lastEventAt: null,
  uploads: 0,
  uploadFailures: 0,
};

const record = (map, key) => map.set(key, (map.get(key) ?? 0) + 1);

function percentile(sorted, p) {
  if (sorted.length === 0) return 0;
  const i = Math.min(sorted.length - 1, Math.max(0, Math.ceil((p / 100) * sorted.length) - 1));
  return sorted[i];
}

// ---------------------------------------------------------------------------- refetch

async function refetch() {
  const url = `${baseUrl}api/media/GetDirectoryContent?path=${encodeURIComponent(viewPath)}&extensions=${encodeURIComponent(opts.extensions)}`;
  const t0 = performance.now();
  try {
    const res = await fetch(url, { headers: authHeaders() });
    const body = await res.text();
    metrics.refetchLatencies.push(performance.now() - t0);
    metrics.refetches++;
    metrics.bytesFetched += body.length;

    if (!res.ok) {
      metrics.refetchFailures++;
      if (metrics.refetchFailures <= 3) {
        console.error(`  refetch ${res.status}: ${body.slice(0, 200)}`);
      }
      return;
    }

    if (metrics.folderCount === null) {
      try {
        const parsed = JSON.parse(body);
        metrics.folderCount = (parsed.folders ?? parsed.Folders ?? []).length;
      } catch {
        /* shape probe only */
      }
    }
  } catch (err) {
    metrics.refetches++;
    metrics.refetchFailures++;
    if (metrics.refetchFailures <= 3) console.error(`  refetch error: ${err.message}`);
  }
}

// ---------------------------------------------------------------------------- connections

function transportType() {
  switch (opts.transport) {
    case "sse": return signalR.HttpTransportType.ServerSentEvents;
    case "lp": return signalR.HttpTransportType.LongPolling;
    default: return signalR.HttpTransportType.WebSockets;
  }
}

async function connectClient(index) {
  const options = {
    transport: transportType(),
    headers: authHeaders(),
  };
  if (opts.token) {
    options.accessTokenFactory = () => opts.token;
  }

  const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${baseUrl}hubs/media`, options)
    .configureLogging(signalR.LogLevel.Error)
    .withAutomaticReconnect([0, 3000, 5000, 10000, 15000, 30000])
    .build();

  connection.on("MediaChanged", (message) => {
    const now = performance.now();
    metrics.eventsReceived++;
    metrics.firstEventAt ??= now;
    metrics.lastEventAt = now;

    const action = message?.action ?? "unknown";
    const path = message?.path ?? "";
    record(metrics.eventsByAction, action);
    record(metrics.sourceChanges, `${action}|${path}`);

    if (doRefetch) {
      void refetch();
    }
  });

  try {
    await connection.start();
    metrics.connected++;
    return connection;
  } catch (err) {
    metrics.connectFailures++;
    if (metrics.connectFailures <= 3) {
      console.error(`  client ${index} failed to connect: ${err.message}`);
    }
    return null;
  }
}

// ---------------------------------------------------------------------------- burst

async function uploadOne(index) {
  const url = `${baseUrl}api/media/Upload?path=${encodeURIComponent(burstPath)}&extensions=`;
  // A 1x1 transparent GIF keeps storage writes trivial so we measure fan-out, not upload throughput.
  const gif = Buffer.from("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7", "base64");
  const form = new FormData();
  form.append("files", new Blob([gif], { type: "image/gif" }), `harness-${Date.now()}-${index}.gif`);

  try {
    const res = await fetch(url, { method: "POST", headers: authHeaders(), body: form });
    if (res.ok) {
      metrics.uploads++;
    } else {
      metrics.uploadFailures++;
      if (metrics.uploadFailures <= 3) {
        console.error(`  upload ${res.status}: ${(await res.text()).slice(0, 200)}`);
      }
    }
  } catch (err) {
    metrics.uploadFailures++;
    if (metrics.uploadFailures <= 3) console.error(`  upload error: ${err.message}`);
  }
}

async function runBurst(count, concurrency) {
  console.log(`\nUploading ${count} files to "${burstPath || "(root)"}" with concurrency ${concurrency}...`);
  let next = 0;
  const workers = Array.from({ length: concurrency }, async () => {
    while (next < count) {
      await uploadOne(next++);
    }
  });
  await Promise.all(workers);
  console.log(`Uploaded ${metrics.uploads}, failed ${metrics.uploadFailures}.`);
}

// ---------------------------------------------------------------------------- report

function report() {
  const wall = (performance.now() - metrics.startedAt) / 1000;
  const sorted = [...metrics.refetchLatencies].sort((a, b) => a - b);
  const changes = metrics.sourceChanges.size;
  const F = metrics.folderCount;
  const storageOpsPerRefetch = F === null ? null : 3 + F;
  const totalStorageOps = storageOpsPerRefetch === null ? null : metrics.refetches * storageOpsPerRefetch;
  const quiesce = metrics.firstEventAt && metrics.lastEventAt
    ? (metrics.lastEventAt - metrics.firstEventAt) / 1000
    : 0;

  const summary = {
    clients: metrics.connected,
    connectFailures: metrics.connectFailures,
    viewPath,
    refetchEnabled: doRefetch,
    wallClockSeconds: +wall.toFixed(2),
    distinctSourceChanges: changes,
    eventsReceived: metrics.eventsReceived,
    eventsPerSourceChange: changes ? +(metrics.eventsReceived / changes).toFixed(2) : 0,
    refetches: metrics.refetches,
    refetchFailures: metrics.refetchFailures,
    refetchesPerSourceChange: changes ? +(metrics.refetches / changes).toFixed(2) : 0,
    subfolderCountF: F,
    storageOpsPerRefetch,
    estimatedStorageOps: totalStorageOps,
    estimatedStorageOpsPerSourceChange:
      totalStorageOps !== null && changes ? +(totalStorageOps / changes).toFixed(2) : null,
    refetchLatencyMs: {
      avg: sorted.length ? +(sorted.reduce((a, b) => a + b, 0) / sorted.length).toFixed(1) : 0,
      p50: +percentile(sorted, 50).toFixed(1),
      p95: +percentile(sorted, 95).toFixed(1),
      max: sorted.length ? +sorted[sorted.length - 1].toFixed(1) : 0,
    },
    bytesFetched: metrics.bytesFetched,
    timeToQuiesceSeconds: +quiesce.toFixed(2),
    eventsByAction: Object.fromEntries(metrics.eventsByAction),
    uploads: metrics.uploads,
    uploadFailures: metrics.uploadFailures,
  };

  console.log("\n=========== BASELINE SUMMARY ===========");
  console.log(`Clients connected              ${summary.clients}${summary.connectFailures ? ` (${summary.connectFailures} failed)` : ""}`);
  console.log(`Viewed directory               "${viewPath || "(root)"}"`);
  console.log(`Refetch on event               ${doRefetch ? "yes (mirrors the real client)" : "no (transport only)"}`);
  console.log(`Wall clock                     ${summary.wallClockSeconds}s`);
  console.log("----------------------------------------");
  console.log(`Distinct source changes        ${summary.distinctSourceChanges}`);
  console.log(`Events received (all clients)  ${summary.eventsReceived}`);
  console.log(`  -> events per source change  ${summary.eventsPerSourceChange}   <= the fan-out multiplier N`);
  console.log(`Refetch requests issued        ${summary.refetches}${summary.refetchFailures ? ` (${summary.refetchFailures} failed)` : ""}`);
  console.log(`  -> refetches per change      ${summary.refetchesPerSourceChange}`);
  if (storageOpsPerRefetch !== null) {
    console.log("----------------------------------------");
    console.log(`Subfolders in view (F)         ${F}`);
    console.log(`Storage ops per refetch        ${storageOpsPerRefetch}   (1 dir-info + 1 dirs + 1 files + ${F} HasChildren)`);
    console.log(`Estimated storage ops total    ${totalStorageOps}`);
    console.log(`  -> per source change         ${summary.estimatedStorageOpsPerSourceChange}   <= N x (3 + F)`);
  } else {
    console.log("\n(No successful refetch, so F and storage-op estimates are unavailable.)");
  }
  console.log("----------------------------------------");
  console.log(`Refetch latency ms             avg ${summary.refetchLatencyMs.avg}  p50 ${summary.refetchLatencyMs.p50}  p95 ${summary.refetchLatencyMs.p95}  max ${summary.refetchLatencyMs.max}`);
  console.log(`Bytes fetched                  ${summary.bytesFetched}`);
  console.log(`Time to quiesce                ${summary.timeToQuiesceSeconds}s`);
  console.log(`Events by action               ${JSON.stringify(summary.eventsByAction)}`);
  if (burstCount > 0) {
    console.log(`Uploads                        ${summary.uploads} ok, ${summary.uploadFailures} failed`);
  }
  console.log("========================================\n");

  if (opts.json) {
    require("node:fs").writeFileSync(opts.json, JSON.stringify(summary, null, 2));
    console.log(`Wrote ${opts.json}`);
  }
}

// ---------------------------------------------------------------------------- main

const connections = [];
let reported = false;

async function shutdown() {
  if (reported) return;
  reported = true;
  await Promise.allSettled(connections.filter(Boolean).map((c) => c.stop()));
  report();
  process.exit(0);
}

process.on("SIGINT", () => void shutdown());

console.log(`Connecting ${clientCount} clients to ${baseUrl}hubs/media (transport: ${opts.transport})...`);
metrics.startedAt = performance.now();

for (let i = 0; i < clientCount; i++) {
  connections.push(await connectClient(i));
}

console.log(`Connected ${metrics.connected}/${clientCount}.`);
if (metrics.connected === 0) {
  console.error("No clients connected — check the URL, auth, and that OrchardCore.Media.SignalR is enabled.");
  process.exit(1);
}

// Take a shape probe so F is known even if nothing ever changes.
if (doRefetch) {
  await refetch();
  metrics.refetches = 0;
  metrics.refetchLatencies = [];
  metrics.bytesFetched = 0;
}

if (burstCount > 0) {
  await runBurst(burstCount, burstConcurrency);
  // Let the fan-out drain before reporting.
  console.log("Waiting 10s for event fan-out to drain...");
  await new Promise((r) => setTimeout(r, 10_000));
  await shutdown();
} else if (durationSec > 0) {
  console.log(`Watching for ${durationSec}s. Trigger media changes from the admin UI now.`);
  setTimeout(() => void shutdown(), durationSec * 1000);
} else {
  console.log("Watching. Trigger media changes from the admin UI, then press Ctrl-C for the summary.");
}
