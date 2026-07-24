# OpenAPI (`OrchardCore.OpenApi`)

The OpenAPI module exposes an OpenAPI (Swagger) specification for your Orchard Core site and provides auto-generated TypeScript and C# clients via [NSwag](https://github.com/RicoSuter/NSwag).

It also ships UI explorers — Swagger UI, ReDoc, and Scalar — so developers can browse and test API endpoints directly from the browser.

## Features

| Feature | Description |
|---------|-------------|
| Swagger UI | Interactive API explorer at `/swagger` |
| ReDoc | Alternative read-only API documentation at `/redoc` |
| Scalar | Modern API reference at `/scalar/v1` |

## Getting Started

1. Enable the **OrchardCore.OpenApi** feature and the desired UI sub-feature(s) — **OpenApi Swagger UI**, **OpenApi ReDoc UI**, **OpenApi Scalar UI** — from the admin dashboard (Configuration → Features).
2. Log in with an account that has the **ViewOpenApiContent** permission (granted to Administrators by default).
3. To make "Try it out" / "Send" requests work against secured API endpoints, run the **OpenAPI Documentation — Bearer/PKCE** recipe (Configuration → Recipes) — see [API Authentication](#api-authentication) below. Browsing the documentation itself needs no extra setup.
4. Navigate to one of the explorer URLs listed above.

> **Note:** All OpenAPI documentation endpoints (`/swagger`, `/redoc`, `/scalar`, `/openapi`) require authentication and the `ViewOpenApiContent` permission. Unauthenticated users are redirected to the admin login page. Authenticated users without the permission receive a `403 Forbidden` response. The JSON schema endpoints (e.g. `/swagger/v1/swagger.json`) also require the `ViewOpenApiContent` permission by default — unauthenticated requests receive `401 Unauthorized` — unless **Allow anonymous access to the API schema** is enabled in the OpenApi settings.

## Configuration

The OpenAPI settings page (**Configuration → Settings → OpenApi**) shows the enablement status of each documentation UI (they are toggled on the **Features** page, not here; disabled UIs return `404 Not Found`) and exposes a single setting:

- **Allow anonymous access to the API schema** — disabled by default. When enabled, the JSON schema endpoints can be fetched without authentication, which external tools like NSwag rely on. The OpenApi generation recipes (`OpenApiGeneration` and the `OpenApiGenerationSetup` recipe used by `tools/OpenApiClientGenerator`) enable this setting automatically.

There is nothing else to configure: API authentication for the documentation UIs is automatic once provisioned, as described below.

## API Authentication

Orchard Core API endpoints authenticate with the `"Api"` scheme, which only accepts Bearer tokens — session cookies are never used for API calls. Instead of an interactive "Authorize" step, the Swagger and Scalar UIs acquire a token **silently**: a script injected into the pages runs an OAuth2 **authorization-code + PKCE** flow against the same tenant's OpenID Connect server in a hidden iframe (`prompt=none`), using your existing admin cookie session. The token is renewed the same way before it expires and is attached automatically to every "Try it out" / "Send" request. ReDoc is read-only documentation with no request surface, so it needs no token.

### Provisioning with the OpenApiPkce recipe

Run the **OpenAPI Documentation — Bearer/PKCE** recipe (Configuration → Recipes). It:

- enables the **OpenApi**, **OpenID Authorization Server**, **OpenID Token Validation**, and **OpenID Management** features;
- turns on the authorization-code flow with **PKCE required** and enables local (same-tenant) token validation for the `"Api"` scheme;
- registers a **public** (secret-less) OpenID application with client ID `openapi`, whose redirect URI points at the module's silent-renew page (`/OrchardCore.OpenApi/openapi-oidc-silent.html`).

Before running it, adjust two things to your environment:

1. Replace the `https://localhost:5001` host in `RedirectUris`/`PostLogoutRedirectUris` with your tenant's real origin. The redirect URI path must match exactly, including any tenant URL prefix.
2. Make sure the users who will use "Try it out" have **roles granting the relevant API permissions** — the `roles` scope carries them into the access token, and the API's permission checks evaluate them.

> **Note:** Client Credentials and Password grant are intentionally not supported by these JavaScript-based documentation UIs — both require a client secret to be embedded in browser-delivered code, which cannot be kept confidential. Authorization code + PKCE with a public client is the recommended flow for browser-based clients per the OAuth 2.0 Security Best Current Practice.

### Security properties

The injected script is a self-contained ES module configured through `data-*` attributes on its own `<script>` tag — no inline scripts, so the pages work under a strict Content Security Policy. It is designed conservatively:

- **Tokens never touch web storage.** Access tokens are held in memory only; each page load performs one cheap same-origin silent sign-in.
- **Same-origin only.** The Bearer token is attached exclusively to same-origin requests whose path contains `/api/` — a spec listing external servers, or an edited target URL in Scalar, cannot exfiltrate it. Schema, UI-asset, and OIDC requests keep the admin cookie instead.
- **Self-healing.** If the server rejects the cached token (for example after the site was set up again and the old token can no longer be decrypted), the script discards it, silently signs in once, and retries the request one time.
- **Manual tokens are respected.** A token pasted into Swagger's Authorize dialog is sent untouched, with no override or retry.

## OpenAPI Client Generation

The module uses an NSwag configuration (`OrchardCore.OpenApi.nswag`) to generate typed clients from the live OpenAPI specification.

### Automated regeneration (recommended)

The `tools/OpenApiClientGenerator` console project runs the whole pipeline headlessly — no running dev server or browser needed:

```powershell
dotnet run --project tools/OpenApiClientGenerator -c Release
yarn build   # refreshes the bundled JS against the regenerated TypeScript client
```

It boots the CMS in-process on an ephemeral port, provisions the Default tenant via `OrchardCore.AutoSetup` with the `OpenApiGenerationSetup` recipe (the same feature set as the `OpenApiGeneration` recipe), captures `swagger.json`, and shells out to the NSwag CLI (the same prerequisite as the manual path below). Generation is deterministic: operations are sorted by route path + HTTP verb, so regenerating without API changes produces byte-identical clients.

The manual procedure below remains useful when you want to regenerate against a real running dev server or inspect `swagger.json` interactively.

### Prerequisites

1. **NSwag CLI** — install the global tool:

    ```powershell
    dotnet tool install -g NSwag.ConsoleCore
    ```

2. **Running application** — the site must be running so NSwag can fetch the OpenAPI JSON.

3. **Anonymous schema access** — the JSON schema endpoints require authentication by default, and the NSwag CLI fetches them anonymously. Run the **OpenApi Generation** recipe on the tenant (Configuration → Recipes), which enables the full API feature set *and* the **Allow anonymous access to the API schema** setting — or enable the setting manually in **Configuration → Settings → OpenApi**.

    > **Warning:** the recipe's settings step **replaces the tenant's stored OpenApi settings**, so any configured OAuth2 authentication (client ID, endpoint URLs, scopes) is reset. Prefer running it on a throwaway generation tenant; on a configured tenant, enable the setting manually instead.

### Regeneration Steps

#### 1. Start the Application

```powershell
cd src/OrchardCore.Cms.Web
dotnet run
```

Wait for the message *"Application started."* in the console.

#### 2. Verify the OpenAPI Endpoint

Open the Swagger UI to confirm your endpoints are listed:

- Swagger UI: `https://localhost:5001/swagger`
- OpenAPI JSON: `https://localhost:5001/swagger/v1/swagger.json`

#### 3. Run NSwag

From the module directory:

```powershell
cd src/OrchardCore.Modules/OrchardCore.OpenApi
nswag run OrchardCore.OpenApi.nswag
```

This generates two clients:

| Client | Output path |
|--------|-------------|
| TypeScript (Axios) | `.scripts/bloom/services/OpenApiClient.ts` |
| C# (HttpClient) | `Services/OpenApiClient.cs` |

#### 4. Build Frontend Assets

After regenerating the TypeScript client, rebuild the frontend:

```powershell
# From the repository root
yarn build
```

### NSwag Configuration

The generation is configured via `OrchardCore.OpenApi.nswag`:

- **Source URL**: `https://localhost:5001/swagger/v1/swagger.json`
- **JSON library (C#)**: `System.Text.Json` — Newtonsoft.Json is **not** used.
- **TypeScript template**: Axios
- **C# HTTP layer**: `System.Net.Http.HttpClient`

## ApiService (`api-service.ts`)

The `ApiService` class (`.scripts/bloom/services/api-service.ts`) is a reusable HTTP service that wraps Axios with authentication handling. It supports both cookie and Bearer token authentication and provides the configured Axios instance to NSwag-generated clients.

### Authentication Types

| Type | Behavior |
|------|----------|
| `"cookie"` (default) | Sets `withCredentials: true` and attaches the anti-forgery token from the page. |
| `"bearer"` | Sets `withCredentials: false` and attaches an `Authorization: Bearer <token>` header. |

### Basic Usage

```typescript
import { ApiService, createApiService } from "@bloom/services/api-service";

// Cookie auth (default) — for admin pages where the user is logged in.
const api = new ApiService();
const response = await api.get("/api/content/my-item-id");

// Bearer auth — for machine-to-machine or external consumers.
const api = new ApiService({ authType: "bearer", token: "eyJ..." });
await api.post("/api/content", { contentType: "Article" });

// Update the token later (e.g., after refresh).
api.setToken("newToken...");
```

### Using with the NSwag-Generated Client

The `ApiService` exposes its underlying Axios instance via `getAxiosInstance()`, which can be passed directly to the NSwag-generated `Client` constructor:

```typescript
import { ApiService } from "@bloom/services/api-service";
import { Client } from "@bloom/services/OpenApiClient";

// Cookie auth — admin pages.
const apiService = new ApiService();
const client = new Client("", apiService.getAxiosInstance());
await client.contentGET("my-content-item-id");

// Bearer auth — external consumers.
const apiService = new ApiService({ authType: "bearer", token: accessToken });
const client = new Client("", apiService.getAxiosInstance());
await client.contentGET("my-content-item-id");
```

This gives the NSwag-generated client all the authentication handling (cookies + anti-forgery token, or Bearer token) without any additional configuration.

### Manual Token Management

If you already have a token (e.g., from a different auth flow), pass it directly:

```typescript
const apiService = new ApiService({ authType: "bearer", token: "eyJ..." });
const client = new Client("", apiService.getAxiosInstance());

// Update the token later (e.g., after refresh).
apiService.setToken("newToken...");
```

### Available Methods

| Method | Description |
|--------|-------------|
| `get<T>(url, config?)` | Perform a GET request. |
| `post<T>(url, data?, config?)` | Perform a POST request. |
| `put<T>(url, data?, config?)` | Perform a PUT request. |
| `patch<T>(url, data?, config?)` | Perform a PATCH request. |
| `delete<T>(url, config?)` | Perform a DELETE request. |
| `setToken(token)` | Update the Bearer token for subsequent requests. |
| `getAxiosInstance()` | Returns the underlying Axios instance for use with generated clients. |

## Adding New API Endpoints

To add a new API endpoint that is auto-discovered by the OpenAPI specification:

1. Create a controller inheriting from `ControllerBase`.
2. Add the `[ApiController]` attribute.
3. Add a `[Route("api/...")]` attribute.
4. Add `[Authorize(AuthenticationSchemes = "Api")]` for secured endpoints.
5. Use XML documentation comments (`///`) for richer Swagger descriptions.
6. Decorate actions with `[ProducesResponseType]` to document all response types.
7. Follow the regeneration steps above to update the clients.

### Example Controller

```csharp
[ApiController]
[Route("api/dashboard/myfeature")]
[Authorize(AuthenticationSchemes = "Api")]
public sealed class MyFeatureController : ControllerBase
{
    /// <summary>
    /// Gets data from my feature.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(MyDataDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDataAsync()
    {
        // Implementation
    }
}
```

## Frontend Notification System

The Bloom frontend framework includes a notification service (`@bloom/services/notifications/notifier`) that understands [RFC 9457 Problem Details](https://www.rfc-editor.org/rfc/rfc9457) responses out of the box. This creates a seamless error-handling pipeline from the API to the user interface.

### How It Works

1. An API endpoint returns a `ProblemDetails` response (e.g. via `Problem()` or `TypedResults.Problem()`).
2. The NSwag-generated client throws the response as an error.
3. The calling code passes the error to `notify()`.
4. The notifier detects the `ProblemDetails` shape and converts it to a UI notification.

### Architecture

```
┌─────────────────────┐     ProblemDetails JSON      ┌─────────────────────────┐
│  API Controller      │ ──────────────────────────► │  NSwag OpenApiClient.ts  │
│  (C# / Server)       │                             │  (throws on non-2xx)     │
└─────────────────────┘                              └────────────┬────────────┘
                                                                  │ error
                                                     ┌────────────▼────────────┐
                                                     │  Service Layer           │
                                                     │  (e.g. FileDataService)  │
                                                     └────────────┬────────────┘
                                                                  │ catch
                                                     ┌────────────▼────────────┐
                                                     │  notify(error)           │
                                                     │  (notifier.ts)           │
                                                     └────────────┬────────────┘
                                                                  │ emit("notify")
                                                     ┌────────────▼────────────┐
                                                     │  NotificationToast.vue   │
                                                     │  (UI Toast Component)    │
                                                     └─────────────────────────┘
```

### Using the Notifier

#### Setup

Register the notification bus once at application startup:

```typescript
import { registerNotificationBus } from "@bloom/services/notifications/notifier";

registerNotificationBus();
```

#### Sending Notifications

```typescript
import { notify, NotificationMessage } from "@bloom/services/notifications/notifier";
import { SeverityLevel } from "@bloom/services/notifications/interfaces";

// Success notification
notify(new NotificationMessage({
  summary: "Success",
  detail: "File uploaded successfully.",
  severity: SeverityLevel.Success,
}));

// Catch API errors — ProblemDetails are handled automatically
try {
  await fileDataService.deleteMedia(path);
} catch (error) {
  notify(error); // title → summary, detail → detail, severity → Error
}
```

#### Supported Message Types

The `notify()` function accepts several shapes:

| Input type | Handling |
|------------|----------|
| `NotificationMessage` | Passed through directly. |
| `ValidationProblemDetails` (has `errors`) | `title` → summary, field errors are joined and shown as detail. |
| `ProblemDetails` (has `title` / `detail`) | `title` → summary, `detail` → detail. |
| `Error` | `"Server Error"` summary, `message` as detail. |
| Falsy / unknown | Falls back to a generic error message. |

#### Listening for Notifications (Vue 3)

```vue
<script setup lang="ts">
import { onMounted, onUnmounted, ref } from "vue";
import { registerNotificationBus } from "@bloom/services/notifications/notifier";
import type { NotificationMessage } from "@bloom/services/notifications/notifier";

const bus = registerNotificationBus();
const messages = ref<NotificationMessage[]>([]);

function onNotify(msg: NotificationMessage) {
  messages.value.push(msg);
}

onMounted(() => bus.on("notify", onNotify));
onUnmounted(() => bus.off("notify", onNotify));
</script>
```

### Severity Levels

The `SeverityLevel` enum (`@bloom/services/notifications/interfaces`) defines four levels:

| Level | Typical use |
|-------|-------------|
| `Success` | Confirming a completed action (file moved, item saved). |
| `Info` | Informational messages. |
| `Warn` | Non-blocking warnings. |
| `Error` | API errors, validation failures, unexpected exceptions. |

## Troubleshooting

### `401 Unauthorized` on "Try it out" Requests

- **Run the OpenApiPkce recipe**: The most common cause is that the silent-auth client is not provisioned, or the **OpenID Token Validation** feature is disabled — without it the API cannot validate Bearer tokens. The recipe sets up both; see [API Authentication](#api-authentication).
- **Redirect URI mismatch**: The registered redirect URI must exactly match your tenant's origin plus `/OrchardCore.OpenApi/openapi-oidc-silent.html` (including any tenant URL prefix). A mismatch makes the silent sign-in fail — check the browser console for `[openapi-ui]` errors.
- **Missing roles**: A token is acquired but the endpoint returns `401`/`403` — verify the signed-in user has roles granting the API permissions being exercised; the `roles` scope carries them into the token.
- **Stale token after re-setup**: If the site was set up again while a documentation tab stayed open, the first rejected request triggers one automatic silent re-sign-in and retry; at worst, reload the page.

### Endpoints Not Appearing in Swagger

- Ensure the controller has the `[ApiController]` attribute.
- Verify the module containing the controller is enabled.
- Check the route does not conflict with MVC routes.
- Restart the application after adding new controllers.

### NSwag Generation Fails

- Verify the application is running and accessible.
- Check the OpenAPI JSON URL is correct.
- Ensure NSwag CLI is installed correctly (`dotnet tool list -g`).
- Check for syntax errors in the `.nswag` configuration.

### TypeScript Compilation Errors

- Run `yarn build` to rebuild all assets.
- Check for type mismatches in your components.
- Verify imports use the correct path to `OpenApiClient.ts`.
