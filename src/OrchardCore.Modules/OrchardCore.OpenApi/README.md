# OpenAPI Client Generation

This document explains how to regenerate the TypeScript and C# OpenAPI clients after adding new API endpoints.

## Prerequisites

1. **NSwag CLI**: Install NSwag Studio or NSwag CLI tools
   ```powershell
   dotnet tool install -g NSwag.ConsoleCore
   ```

2. **Running Application**: The application must be running to generate the OpenAPI specification

## Regeneration Steps

### 1. Start the Application

Start the application targeting the tenant you want to generate clients for:

```powershell
cd src/OrchardCore.Cms.Web
dotnet run -f net9.0
```

Wait for the message "Application started." in the console.

### 2. Verify OpenAPI Endpoint

Navigate to the Swagger UI to verify your new endpoints are discovered:

- For Vloparts tenant: `https://localhost:5001/vloparts/swagger`
- OpenAPI JSON: `https://localhost:5001/vloparts/openapi/v1.json`

Check that your new `MediaApiController` endpoints appear in the list.

### 3. Regenerate Clients

From the OrchardCore.OpenApi module directory:

```powershell
cd src/OrchardCore.Modules/OrchardCore.OpenApi
nswag run OrchardCore.OpenApi.nswag
```

This will generate both:
- **TypeScript Client**: `.scripts/bloom/services/OpenApiClient.ts` (for React apps)
- **C# Client**: `src/OrchardCore.Modules/OrchardCore.OpenApi/Services/OpenApiClient.cs`

### 4. Verify Generated Code

Check that the new `MediaApiController` methods appear in both generated files:

```typescript
// In OpenApiClient.ts, look for:
export class MediaApiClient {
    uploadAsync(path?: string, extensions?: string): Promise<MediaUploadResponseDto>
}
```

```csharp
// In OpenApiClient.cs, look for:
public partial class MediaApiClient
{
    public async Task<MediaUploadResponseDto> UploadAsync(string path, string extensions)
}
```

### 5. Build Assets

After regenerating the TypeScript client, rebuild the frontend assets:

```powershell
# From repository root
yarn build
```

Or for faster incremental builds:

```powershell
cd src/OrchardCore.Modules/GoRide.Dashboard/Assets/content-editor
yarn build
```

## Configuration

The generation is configured via `OrchardCore.OpenApi.nswag`:

- **Source URL**: `https://localhost:5001/vloparts/openapi/v1.json`
- **TypeScript Output**: `../../../.scripts/bloom/services/OpenApiClient.ts`
- **C# Output**: `Services/OpenApiClient.cs`
- **Template**: Axios (for TypeScript), HttpClient (for C#)

## Adding New API Endpoints

To add a new API endpoint that will be auto-discovered:

1. Create a controller inheriting from `ControllerBase`
2. Add `[ApiController]` attribute
3. Add `[Route("api/...")]` attribute  
4. Add `[Authorize(AuthenticationSchemes = "Api")]` for secured endpoints
5. Use XML documentation comments (///) for better Swagger docs
6. Use `[ProducesResponseType]` attributes to document responses
7. Follow the steps above to regenerate clients

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
    /// <returns>The feature data.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(MyDataDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDataAsync()
    {
        // Implementation
    }
}
```

## Using the Generated Clients

### TypeScript/React

```typescript
import { MediaApiClient } from '@/services/OpenApiClient';

const client = new MediaApiClient();
const result = await client.uploadAsync('products', 'jpg,png');
```

### C#

```csharp
using OrchardCore.OpenApi.Services;

public class MyService
{
    private readonly MediaApiClient _mediaApiClient;

    public MyService(MediaApiClient mediaApiClient)
    {
        _mediaApiClient = mediaApiClient;
    }

    public async Task UploadFileAsync()
    {
        var result = await _mediaApiClient.UploadAsync("products", "jpg,png");
    }
}
```

## Troubleshooting

### Endpoints Not Appearing in Swagger

- Ensure the controller has `[ApiController]` attribute
- Verify the module containing the controller is enabled
- Check the route doesn't conflict with MVC routes
- Restart the application after adding new controllers

### Generation Fails

- Verify the application is running and accessible
- Check the OpenAPI JSON URL is correct for your tenant
- Ensure NSwag CLI is installed correctly
- Check for syntax errors in the .nswag configuration

### TypeScript Compilation Errors

- Run `yarn build` to rebuild all assets
- Check for type mismatches in your React components
- Verify imports use the correct path to OpenApiClient.ts

## MediaApiController Specifics

The new `MediaApiController` provides:

- **Endpoint**: `POST /api/dashboard/media/upload`
- **Authentication**: Requires bearer token or cookie authentication
- **Authorization**: Requires `ManageMediaContent` permission
- **Parameters**:
  - `path` (query, optional): Folder path for uploads (e.g., "products")
  - `extensions` (query, optional): Allowed extensions (e.g., "jpg,png,gif")
- **Request**: Multipart form data with file(s)
- **Response**: `MediaUploadResponseDto` with file metadata and URLs

This replaces the previous minimal API endpoint at `/dashboard/api/media/upload` and is fully documented in Swagger.
