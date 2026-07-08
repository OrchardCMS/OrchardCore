---
name: orchardcore-nswag-regenerate
description: Regenerate the OrchardCore.OpenApi module's NSwag-generated C#/TypeScript API clients, and verify the regeneration produces a stable (non-reshuffled) diff. Use when asked to "regenerate the NSwag client(s)", update Services/OpenApiClient.cs or .scripts/bloom/services/OpenApiClient.ts, or investigate noisy diffs from NSwag regeneration.
---

# OrchardCore NSwag Regenerate

Regenerates the OpenApi module's NSwag-generated C#/TypeScript API clients and verifies the regeneration produces a stable, non-reshuffled diff.

## Where things live

- Config: `src/OrchardCore.Modules/OrchardCore.OpenApi/OrchardCore.OpenApi.nswag` — **this file already exists**, do not assume it's missing. It defines both generators:
  - `openApiToCSharpClient` → outputs `Services/OpenApiClient.cs` (relative to the module folder)
  - `openApiToTypeScriptClient` → outputs `../../../.scripts/bloom/services/OpenApiClient.ts`
- Source document: `documentGenerator.fromDocument.url` points at a running instance's Swashbuckle endpoint, `https://localhost:5001/swagger/v1/swagger.json`. NSwag also accepts a **local file path** in this same field instead of a URL — useful for reproducible/offline generation (see below).
- Recipe: `src/OrchardCore.Modules/OrchardCore.OpenApi/Recipes/openapi-generation.recipe.json` (name `OpenApiGeneration`) enables every feature that exposes an API endpoint: `OrchardCore.Contents`, `OrchardCore.Queries`, `OrchardCore.Tenants`, `OrchardCore.Search.Lucene`, `OrchardCore.Search.Elasticsearch`, `OrchardCore.OpenApi`.
  - It has `"issetuprecipe": false`, so it does **not** appear in the new-tenant/site-setup recipe dropdown. Run it against an already-set-up tenant instead, via **Admin ▸ Configuration ▸ Recipes**, click "Run" next to "OpenApi Generation", confirm the modal. (`/Admin/Recipes`, `AdminController.Execute` in `OrchardCore.Recipes`.)

## NSwag CLI

Installed as a dotnet tool: `~/.dotnet/tools/nswag` (or `dotnet tool install -g NSwag.ConsoleCore` if missing). Run with:

```bash
nswag run src/OrchardCore.Modules/OrchardCore.OpenApi/OrchardCore.OpenApi.nswag
```

This requires a live server at the configured `url`. For a real regeneration that updates the committed files, run the actual local dev server, set it up with the `OpenApiGeneration` recipe, and run the command above unmodified.

## Reproducible / offline generation (for testing determinism, without touching committed files)

1. Fetch `swagger.json` from a running instance into a scratch file.
2. Copy the `.nswag` config, and in the copy only change `documentGenerator.fromDocument.url` to the scratch file's path, and both generators' `output` to scratch paths. Do this with a small Python/jq one-liner rather than hand-editing — the config is plain JSON.
3. `nswag run <scratch-config>`.

This lets you diff two independent generations without ever touching the real generated files or needing HTTPS/dev-cert setup.

## Determinism: why regeneration used to reshuffle unrelated methods

Two independent root causes were found and fixed (skrypt/openapi branch):

1. **Swashbuckle's operation order wasn't pinned.** Without an explicit sort, `swagger.json` operations come out in whatever order ASP.NET Core's action discovery enumerates them — which depends on assembly/feature load order in OrchardCore's modular architecture, not source order. Fixed in `OrchardCore.OpenApi/Startup.cs`'s `AddSwaggerGen` call:
   ```csharp
   c.OrderActionsBy(apiDesc => $"{apiDesc.RelativePath}_{apiDesc.HttpMethod}");
   ```
   Route path + verb is fixed at compile time and unique per operation, so this is stable regardless of load order.

2. **A duplicate `operationId` across two operations.** `QueryApiController` used to have one action handling both `GET` and `POST` on `api/queries/{name}` under a single `[EndpointName("ApiExecuteQuery")]`. OpenAPI requires `operationId` to be unique per operation — reusing one across two operations forced NSwag to invent a disambiguating suffix internally, and that suffix logic wasn't deterministic across runs (e.g. `ApiExecuteQueryPOSTAsync` vs `ApiExecuteQueryPOSTPOSTAsync`). Fixed by splitting into two actions with distinct names (`ApiExecuteQueryGet` / `ApiExecuteQueryPost`), delegating to a shared private method — the pattern already used by e.g. `ElasticsearchApiController` (`ApiGetElasticsearchContent`/`ApiPostElasticsearchContent`).

**Lesson for any future controller/endpoint added to the OpenApi-exposed surface**: never reuse the same `[EndpointName]`/`operationId` across two different HTTP verbs on the same route. Give each verb its own action and its own name, even if they share implementation via a private helper.

## Verifying stability empirically

Boot two independently-provisioned tenants (each gets its own `ShellContext` and fresh feature/extension discovery — this is what actually varies, not wall-clock time), run the `OpenApiGeneration` recipe on each, fetch `swagger.json` from each, generate against both, and diff. A stable setup produces byte-identical output except for the tenant's own base-URL prefix embedded in the client's default `baseUrl` (an expected, real difference between distinct tenants — not a bug).

The existing functional test suite (`test/OrchardCore.Tests.Functional/Tests/Cms/OpenApiTests.cs`) already exercises the relevant plumbing (feature enablement, swagger.json access) if you need a template for scripting this via Playwright/`CmsTestBase`. Any throwaway verification test written for this should be deleted afterward — it's not meant to be a permanent part of the suite.
