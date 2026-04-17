# Media Gallery

The Media Gallery is a Vue 3 application that provides the admin UI for managing media files and folders.

## File Operations

The gallery supports the following operations on files:

- **Upload** — Upload files via the upload button or drag-and-drop onto the file list area.
- **Rename** — Rename a single file via its context menu.
- **Move** — Move one or more selected files to another folder via the context menu. Files can also be moved by dragging them onto a folder in the tree.
- **Copy** — Copy a single file to another folder via its context menu.
- **Delete** — Delete one or more selected files. A confirmation dialog is shown before deletion.
- **Download** — Download a single file via its context menu.

## Folder Management

- **Create subfolder** — Right-click a folder in the tree to create a subfolder.
- **Delete folder** — Right-click a folder to delete it. The root folder cannot be deleted.
- **Breadcrumb navigation** — Navigate the folder hierarchy via the breadcrumb bar.

## Search, Sort, and Pagination

- **Search** — Filter the file list by name using the search bar.
- **Sort** — Sort by name, size, MIME type, or last modified date in ascending or descending order.
- **Pagination** — The file list is paginated. Page size can be configured via the settings popover.

## Storage Information

The storage info popover (accessible from the toolbar) displays information about the configured storage provider:

- Storage provider name
- Available storage space
- Hierarchical namespace support
- Atomic move support
- SignalR and TUS feature status

## Upload Behavior

### File Size Validation

The `MaxFileSize` setting is enforced at multiple layers:

1. **Client-side** — Files exceeding the limit are rejected before upload begins, with a localized error message.
2. **ASP.NET Core** — The `MediaSizeLimitAttribute` configures both Kestrel's `MaxRequestBodySize` and the form `MultipartBodyLengthLimit` to match `MaxFileSize`.
3. **TUS protocol** — When TUS is enabled, `MaxAllowedUploadSizeInBytesLong` is set to `MaxFileSize`.

!!! warning
    When hosting behind IIS, you must also configure `maxAllowedContentLength` in `web.config` to match or exceed `MaxFileSize`. If the IIS limit is lower, uploads will be rejected by IIS before reaching the application. A warning is logged at startup when this mismatch is detected. See [IIS Request Limits](https://docs.microsoft.com/en-us/iis/configuration/system.webserver/security/requestfiltering/requestlimits/) for details.

### File Extension Validation

The `AllowedFileExtensions` setting is enforced both client-side and server-side. Files with disallowed extensions are rejected before upload begins in the Media Gallery. The server also validates extensions on all upload endpoints.

### Concurrent Uploads

When uploading multiple files, the Media Library uploads each file as a separate request with a maximum of **5 concurrent uploads**. Files beyond this limit are queued and uploaded as previous uploads complete. This prevents overwhelming the server while still allowing efficient batch uploads.

## TUS Resumable Uploads (`OrchardCore.Media.Tus`)

The TUS feature enables resumable file uploads using the [TUS protocol](https://tus.io/). When enabled, it replaces the default upload mechanism, allowing uploads to be paused, resumed, and recovered after network interruptions.

To enable, activate the **Media TUS Uploads** feature in the admin panel.

When TUS is enabled:

- Files are uploaded in configurable chunks (default 5 MB, controlled by `MaxUploadChunkSize`).
- Uploads can be paused and resumed from the upload toast.
- Interrupted uploads automatically resume from where they left off using fingerprint-based tracking in the browser's localStorage.
- The `MaxFileSize` limit is enforced by the TUS server.

The TUS endpoint is available at `/api/media/tus`.

!!! warning
    TUS stores partial upload data on local disk by default. In multi-instance deployments, you must ensure that all upload chunks for a given file reach the same server instance. This can be achieved by configuring `TusTempPath` to a **shared filesystem** path accessible from all instances, or by enabling **sticky sessions** (session affinity) on your load balancer. Without this, a chunked upload that spans multiple instances will fail because the second instance cannot find the partial file created by the first.

To configure a shared path for TUS uploads:

```json
{
  "OrchardCore_Media": {
    "TusTempPath": "/mnt/shared/TusUploads"
  }
}
```

For Docker deployments, set `TusTempPath` to a path inside a shared volume:

```json
{
  "OrchardCore_Media": {
    "TusTempPath": "/app/data/TusUploads"
  }
}
```

```yaml
# docker-compose.yml
services:
  app:
    volumes:
      - tus-uploads:/app/data/TusUploads

volumes:
  tus-uploads:
    # For multi-instance, use a shared volume (NFS, Azure File Share, EFS)
```

Alternatively, configure sticky sessions on your load balancer instead of a shared filesystem:

- **Azure App Service** — Enable ARR Affinity in Configuration > General settings.
- **Nginx** — Use `sticky cookie` directive (requires nginx-sticky-module or NGINX Plus).
- **HAProxy** — Use `cookie SERVERID insert indirect nocache` in the backend configuration.
- **AWS ALB** — Enable stickiness on the target group.

!!! note
    Sticky sessions ensure all chunks reach the same instance, but if that instance goes down mid-upload, the partial file is lost and the upload must restart from scratch. A shared filesystem provides better resilience since any instance can continue the upload if the original instance becomes unavailable.

## SignalR Real-time Updates (`OrchardCore.Media.SignalR`)

The SignalR feature enables real-time media updates. When enabled, changes to media files and folders (uploads, renames, moves, deletes) are broadcast to all connected clients. This keeps the Media Gallery in sync across multiple browser tabs and users.

To enable, activate the **Media SignalR** feature in the admin panel.

For multi-instance deployments, a backplane is required. Two options are available:

- **`OrchardCore.Media.SignalR.Azure`** — Uses Azure SignalR Service as the backplane.
- **`OrchardCore.Media.SignalR.Redis`** — Uses Redis as the backplane.

## Multi-instance Deployment

To fully scale the Media Library across multiple application instances, the following components must be configured:

| Component | Purpose | Configuration |
|---|---|---|
| **SignalR backplane** | Broadcast real-time updates across instances | Enable `OrchardCore.Media.SignalR.Azure` or `OrchardCore.Media.SignalR.Redis` |
| **Sticky sessions** or **shared TUS path** | Ensure TUS upload chunks are accessible across instances | Configure session affinity on your load balancer, or set `TusTempPath` to a shared filesystem |
| **Shared media storage** | Store media files accessible from all instances | Configure Azure Blob Storage, Amazon S3, or a shared filesystem |

!!! note
    Without a SignalR backplane, real-time updates only work within a single instance. Without sticky sessions (or a shared temp filesystem), TUS resumable uploads may fail when chunks are routed to different instances. Standard (non-TUS) uploads are not affected since each file is uploaded in a single request.

## Media Field Editor Types

The `MediaField` supports three editor types, configurable per field instance:

### Standard (picker)

The default editor. Users select media from the Media Library via a picker modal. No direct upload is available from the field — files must already exist in the Media Library.

### Attached

Users upload files directly from the field via a file input or drag-and-drop. Uploaded files are stored in a temporary folder and moved to the final location when the content item is published. Supports TUS resumable uploads when the TUS feature is enabled.

### Gallery

A gallery-oriented editor with card and list views. Users select media from the Media Library via a picker modal. Supports reordering items via drag-and-drop.
