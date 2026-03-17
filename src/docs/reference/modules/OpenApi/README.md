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

1. Enable the **OrchardCore.OpenApi** feature from the admin dashboard (Configuration → Features).
2. Log in with an account that has the **ApiViewContent** permission (granted to Administrators by default).
3. Navigate to **Configuration → Settings → OpenApi** to enable the desired UI(s) and configure authentication.
4. Navigate to one of the explorer URLs listed above.

> **Note:** All OpenAPI documentation endpoints (`/swagger`, `/redoc`, `/scalar`, `/openapi`) require authentication and the `ApiViewContent` permission. Unauthenticated users are redirected to the admin login page. Authenticated users without the permission receive a `403 Forbidden` response.

## Configuration

The OpenAPI settings page (**Configuration → Settings → OpenApi**) allows you to:

- **Enable/disable each UI** independently (Swagger UI, ReDoc, Scalar). Disabled UIs return `404 Not Found`.
- **Choose the authentication method** used by the "Try it out" / "Send" buttons in the documentation UIs.

### Authentication Types

| Type | Description |
|------|-------------|
| **Cookie (default)** | No additional configuration needed. If you are logged in, the UIs automatically use your session cookie. |
| **OAuth2 Authorization Code + PKCE** | Interactive login. The "Authorize" button redirects to the authorization server. Suitable for browser-based API access. |
| **OAuth2 Client Credentials** | Machine-to-machine authentication. The "Authorize" dialog prompts for a client ID and secret, then exchanges them for a Bearer token. |

### Cookie Authentication

This is the simplest option. The API documentation UIs include the session cookie with every request, so if you are logged into the admin panel, API calls work automatically.

### OAuth2 Setup

For OAuth2 authentication (either PKCE or Client Credentials), you need to:

1. **Enable the OpenID Server** feature (Configuration → Features → OpenID Authorization Server).
2. **Enable the OpenID Token Validation** feature — this is required for the API to validate Bearer tokens. Without it, API requests will return `401 Unauthorized` even with a valid token.
3. **Create an OpenID application** (Security → OpenID Connect → Applications):
   - For **Client Credentials**: set the type to **Confidential client**, enable **Allow Client Credentials Flow**, and assign the appropriate **Client Credentials Roles** (e.g., Administrator) so the token has permissions to access API endpoints.
   - For **PKCE**: enable **Allow Authorization Code Flow** and configure a redirect URI for the Swagger UI callback.
4. **Configure the OpenAPI settings** (Configuration → Settings → OpenApi):
   - Select the authentication type.
   - Enter the **Token URL** (e.g., `/connect/token`).
   - Enter the **Authorization URL** (PKCE only, e.g., `/connect/authorize`).
   - Enter the **Client ID** from the OpenID application.
   - Enter the **Scopes** (e.g., `api`).

## OpenAPI Client Generation

The module uses an NSwag configuration (`OrchardCore.OpenApi.nswag`) to generate typed clients from the live OpenAPI specification.

### Prerequisites

1. **NSwag CLI** — install the global tool:

    ```powershell
    dotnet tool install -g NSwag.ConsoleCore
    ```

2. **Running application** — the site must be running so NSwag can fetch the OpenAPI JSON.

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

## ProblemDetails Error Handling

API controllers should return standardized [RFC 9457 Problem Details](https://www.rfc-editor.org/rfc/rfc9457) responses for error conditions. The `ProblemDetailsApiControllerExtensions` class in `OrchardCore.Abstractions` provides convenient extension methods for this.

### Available Extension Methods

| Method | Status Code | Purpose |
|--------|-------------|---------|
| `ApiChallengeOrForbidForCookieAuth()` | 401 / 403 | Returns `Unauthorized` or `Forbidden` depending on the authentication state. |
| `ApiBadRequestProblem()` | 400 | Generic bad-request error. |
| `ApiNotFoundProblem()` | 404 | Resource not found. |
| `ApiValidationProblem()` | 400 | Validation error with a `ModelStateDictionary` containing field-level errors. |

All methods support localized `title` and `detail` parameters via `LocalizedString`.

### Usage in Controllers

```csharp
[ApiController]
[Route("api/media")]
public class MediaApiController : ControllerBase
{
    [HttpGet("{path}")]
    [ProducesResponseType(typeof(FileStoreEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMediaItem(string path)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMedia))
        {
            return this.ApiChallengeOrForbidForCookieAuth();
        }

        var item = await _mediaFileStore.GetFileInfoAsync(path);

        if (item == null)
        {
            return this.ApiNotFoundProblem(S["Media not found: {0}", path]);
        }

        return Ok(ToDto(item));
    }
}
```

### JSON Response Shape

A `ProblemDetails` response looks like this:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "detail": "Media not found: images/photo.jpg",
  "status": 404
}
```

A `ValidationProblemDetails` response includes an `errors` dictionary:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "A validation error occurred.",
  "status": 400,
  "errors": {
    "Name": ["The Name field is required."],
    "Path": ["The path contains invalid characters."]
  }
}
```

## Frontend Notification System

The Bloom frontend framework includes a notification service (`@bloom/services/notifications/notifier`) that understands `ProblemDetails` responses out of the box. This creates a seamless error-handling pipeline from the API to the user interface.

### How It Works

1. An API controller returns a `ProblemDetails` response (e.g. `ApiNotFoundProblem()`).
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

### OAuth2 "Failed to authorize" or `401 Unauthorized` on API Requests

- **Enable OpenID Token Validation**: The most common cause. Go to Configuration → Features and enable the **OpenID Token Validation** feature. Without it, the API cannot validate Bearer tokens.
- **Assign Client Credentials Roles**: For the Client Credentials flow, the OpenID application must have roles assigned (e.g., Administrator) under "Client Credentials Roles". Without roles, the token has no permissions.
- **Restart after settings changes**: OAuth2 settings are applied at startup. After changing authentication settings, the tenant must be reloaded.

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
