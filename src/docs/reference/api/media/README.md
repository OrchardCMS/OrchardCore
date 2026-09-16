# Media management API (`OrchardCore.Media`)

## Purpose and feature

The Media management API lets a remote client browse, inspect, upload, copy, move, and delete files and folders in the current tenant's media store. Enable the **Media** feature (`OrchardCore.Media`). Enable **Media TUS Uploads** (`OrchardCore.Media.Tus`) before using the completed resumable-upload lookup.

These operations are advertised as the `media` remote-management capability, version `1.0`. Except for the localization document, they are management operations and are distinct from the legacy Media API routes used by the admin media application.

## Authentication and authorization

Send an access token through the Orchard Core `Api` authentication scheme:

```http
Authorization: Bearer ACCESS_TOKEN
```

Every management operation requires:

1. an authenticated `Api` bearer token;
2. the `AccessRemoteManagement` permission; and
3. the Media permissions checked by the operation.

All operations require `ManageMediaContent` (**Manage Media**). Folder-scoped operations also authorize `ManageMediaFolder` (**Manage All Media Folders**) against each affected path. Secure Media may restrict that authorization to particular folders. The list-all, tree, and constraints operations authorize the media root. The completed-upload lookup checks only `ManageMediaContent`.

Uploads and copy/move destinations also enforce the configured extension policy.
`AllowedFileExtensions` are available to Media managers; `RestrictedFileExtensions`
require the existing `UploadRestrictedMedia` permission. By default `.css`, `.js`,
and `.svg` are restricted. Extensions absent from both lists are rejected even
with this permission. Comparisons are case-insensitive. See
[Media configuration](../../modules/Media/README.md) for the policy and storage
providers. These APIs use `IMediaFileStore`, the same extensible store as the UI;
multi-node deployments must configure a shared backend to share uploads.

An application identity without a personal Media folder cannot use
`ManageOwnMediaContent` to access other users' folders. Grant the appropriate
folder or other-users Media permission when that access is required; ordinary
custom-asset folders follow the existing Media folder policy.

`GET /api/media/localizations` is intentionally anonymous. It exposes only UI labels.

An absent or invalid bearer token produces `401 Unauthorized` and a `WWW-Authenticate` challenge. An authenticated principal missing `AccessRemoteManagement` or an operation-specific Media permission produces `403 Forbidden`.

## Base route

Routes are relative to the tenant base URL:

```text
https://host/{tenant-prefix}/api/media
```

Paths in parameters are media-store-relative and use `/`, for example `images/logo.png`. An omitted or empty folder path means the media root unless an operation says otherwise.

## Endpoint summary

| Method | Path | Purpose | Media authorization |
| --- | --- | --- | --- |
| `GET` | `/api/media/localizations` | Get culture-specific UI labels | Anonymous |
| `GET` | `/api/media/constraints` | Get storage and upload constraints | Manage Media + root folder |
| `GET` | `/api/media/folders/tree` | Get the accessible folder tree | Manage Media + root folder |
| `GET` | `/api/media/folders` | List direct child folders | Manage Media + requested folder |
| `POST` | `/api/media/folders` | Create a folder | Manage Media + new folder path |
| `DELETE` | `/api/media/folder` | Delete a folder | Manage Media + requested folder |
| `GET` | `/api/media/directories/content` | List direct child folders and files | Manage Media + requested folder |
| `GET` | `/api/media/files` | List direct child files | Manage Media + requested folder |
| `GET` | `/api/media/items` | Recursively list accessible files | Manage Media + root/folder checks |
| `GET` | `/api/media/file` | Get one file's metadata | Manage Media + file path |
| `DELETE` | `/api/media/file` | Delete one file | Manage Media + file path |
| `GET` | `/api/media/files/metadata` | Get metadata for several files | Manage Media + every file path |
| `PUT` | `/api/media/files/content` | Upload a binary stream | Manage Media + target folder |
| `POST` | `/api/media/files:copy` | Copy one file | Manage Media + source/destination paths |
| `POST` | `/api/media/files:move` | Move one file | Manage Media + source/destination paths |
| `POST` | `/api/media/files:delete` | Delete several files | Manage Media + every path |
| `POST` | `/api/media/files:move-batch` | Move several files between folders | Manage Media + both folders |
| `GET` | `/api/media/uploads/{uploadId}` | Resolve a completed TUS upload | Manage Media |

## Common representations

JSON property names are camel-cased.

### File or folder

`FileStoreEntryDto` has these fields:

| Property | Type | Meaning |
| --- | --- | --- |
| `name` | string | Entry name. |
| `size` | integer (`int64`) | File length; the store's entry length for a folder. |
| `directoryPath` | string | Parent path for a file; full folder path for a folder. |
| `filePath` | string or null | Full media path for a file; null for folders. |
| `lastModifiedUtc` | string (`date-time`) | UTC last-modified value. |
| `isDirectory` | boolean | `true` for a folder. |
| `url` | string or null | Absolute URL for a file, including the version query when available; null for folders. |
| `mime` | string or null | Detected file content type, or `application/octet-stream`; null for folders. |
| `hasChildren` | boolean or null | Whether an accessible child folder exists; null when not computed. |

`filePath` and `directoryPath` remain media-store paths for commands such as
copy, move, and delete. `url` is a direct link to the file. Local media URLs use
the request's scheme and host (including any port), with the tenant/media path
provided by the media store. Configured CDN URLs retain their origin, escaped
file names, and query parameters. The mapping uses the same `IMediaFileStore`
provider as GraphQL; management responses also resolve relative mappings to
absolute URLs. Configure forwarded headers correctly when serving Orchard
behind a reverse proxy so the request reflects the public scheme and host.

URLs do not grant additional access: secured media still requires the access
configured for its serving endpoint. Folders have no direct file URL, and delete
responses retain paths without links to deleted resources.

The CLI includes both **Path** and **URL** in media file list tables and file
metadata output. The URL is not shortened. Upload, copy, move, and completed
TUS upload results expose the resulting file URL as well.

Example file:

```json
{
  "name": "logo.png",
  "size": 3812,
  "directoryPath": "images",
  "filePath": "images/logo.png",
  "lastModifiedUtc": "2026-08-28T20:15:30Z",
  "isDirectory": false,
  "url": "https://cms.example.com/tenant-a/media/images/logo.png?v=abc123",
  "mime": "image/png",
  "hasChildren": null
}
```

Example folder:

```json
{
  "name": "images",
  "size": 0,
  "directoryPath": "images",
  "filePath": null,
  "lastModifiedUtc": "2026-08-28T20:10:00Z",
  "isDirectory": true,
  "url": null,
  "mime": null,
  "hasChildren": true
}
```

### Paged media list

Folder, file, and recursive item lists use:

```json
{
  "skip": 0,
  "take": 50,
  "totalCount": 1,
  "items": [
    {
      "name": "logo.png",
      "size": 3812,
      "directoryPath": "images",
      "filePath": "images/logo.png",
      "lastModifiedUtc": "2026-08-28T20:15:30Z",
      "isDirectory": false,
      "url": "https://cms.example.com/tenant-a/media/images/logo.png?v=abc123",
      "mime": "image/png",
      "hasChildren": null
    }
  ]
}
```

`skip` defaults to `0`; `take` defaults to `50`. `skip` must be at least `0`, and `take` must be from `1` through `200`. Sorting is ordinal, case-insensitive by path before paging.

### Problem Details

Errors use `application/problem+json`. A representative permission response is:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.4",
  "title": "Forbidden",
  "status": 403,
  "detail": "You do not have sufficient permissions to complete this request"
}
```

Validation failures are HTTP `400` and use Validation Problem Details:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "A validation error occurred.",
  "status": 400,
  "detail": "This file extension is not allowed: .exe",
  "errors": {}
}
```

Titles and details produced through localizers can vary with the request culture.

## Operations

### Get localizations

```http
GET /api/media/localizations
```

No authentication is required.

| Header | Required | Description |
| --- | --- | --- |
| `Accept-Language` | No | Selects the current UI culture. |
| `If-None-Match` | No | One or more ETags from an earlier response; weak comparison is accepted. |

```bash
curl -H 'Accept-Language: fr-FR' \
  'https://cms.example.com/tenant-a/api/media/localizations'
```

`200 OK` returns a JSON object whose keys and values are the merged `media-gallery` localization set:

```json
{
  "Select": "Sélectionner",
  "Cancel": "Annuler"
}
```

The exact keys and translated values depend on enabled localization providers and culture. The response includes:

```http
Content-Type: application/json; charset=utf-8
ETag: "0123456789abcdef"
Cache-Control: public, max-age=300
Vary: Accept-Language
```

A matching `If-None-Match` returns `304 Not Modified` with no response body.

### Get constraints

```http
GET /api/media/constraints
```

There are no operation parameters.

```bash
curl -H 'Authorization: Bearer ACCESS_TOKEN' \
  'https://cms.example.com/tenant-a/api/media/constraints'
```

`200 OK` returns:

```json
{
  "bytes": 1073741824,
  "text": "1 GB",
  "allowedFileExtensions": [".gif", ".jpeg", ".jpg", ".png"],
  "maxFileSize": 31457280,
  "maxUploadChunkSize": 10485760,
  "authenticationScheme": "Bearer"
}
```

| Property | Type | Notes |
| --- | --- | --- |
| `bytes` | integer (`int64`) or null | Store-reported permitted bytes; null when unspecified. |
| `text` | string | Formatted capacity, or localized `Unspecified`. |
| `allowedFileExtensions` | array of string | Extensions this caller may upload: configured allowed extensions plus restricted extensions when `UploadRestrictedMedia` is granted, deduplicated and sorted ordinal case-insensitively. |
| `maxFileSize` | integer (`int64`) | Maximum upload size in bytes. |
| `maxUploadChunkSize` | integer (`int32`) | Configured chunk size in bytes. |
| `authenticationScheme` | string | Current legacy Media API authentication setting: `Cookie` (default) or `Bearer`. It does not change the bearer-only management routes. |

Other responses: `401`, `403`.

### Get folder tree

```http
GET /api/media/folders/tree
```

There are no operation parameters.

```bash
curl -H 'Authorization: Bearer ACCESS_TOKEN' \
  'https://cms.example.com/tenant-a/api/media/folders/tree'
```

`200 OK` returns one root `DirectoryTreeNodeDto`. Inaccessible branches are omitted, and `hasChildren` counts accessible children only:

```json
{
  "name": "",
  "path": "",
  "hasChildren": true,
  "children": [
    {
      "name": "images",
      "path": "images",
      "hasChildren": false,
      "children": []
    }
  ]
}
```

Other responses: `401`, `403`.

### List folders

```http
GET /api/media/folders
```

| Query parameter | Type | Required | Default/constraints |
| --- | --- | --- | --- |
| `path` | string | No | Media root when omitted or empty. |
| `skip` | integer | No | `0`; must be at least `0`. |
| `take` | integer | No | `50`; range `1`–`200`. |

```bash
curl -G -H 'Authorization: Bearer ACCESS_TOKEN' \
  --data-urlencode 'path=images' \
  --data-urlencode 'skip=0' \
  --data-urlencode 'take=50' \
  'https://cms.example.com/tenant-a/api/media/folders'
```

`200 OK` returns the paged media envelope. Every item is a folder; `hasChildren` considers accessible child folders only.

```json
{
  "skip": 0,
  "take": 50,
  "totalCount": 1,
  "items": [
    {
      "name": "products",
      "size": 0,
      "directoryPath": "images/products",
      "filePath": null,
      "lastModifiedUtc": "2026-08-28T20:10:00Z",
      "isDirectory": true,
      "url": null,
      "mime": null,
      "hasChildren": false
    }
  ]
}
```

Other responses: `400` for invalid paging, `401`, `403`, and `404` when a non-root folder does not exist.

### Create a folder

```http
POST /api/media/folders
Content-Type: application/json
```

| Body property | Type | Required | Constraints |
| --- | --- | --- | --- |
| `path` | string | No | Parent folder; null or empty selects root. A protected system folder cannot be the parent. |
| `name` | string | Yes for a usable request | Normalized by the active media-name normalizer; may not contain `/` or `\`. |

```bash
curl -X POST -H 'Authorization: Bearer ACCESS_TOKEN' \
  -H 'Content-Type: application/json' \
  -d '{"path":"images","name":"products"}' \
  'https://cms.example.com/tenant-a/api/media/folders'
```

`200 OK` returns:

```json
{
  "folder": {
    "name": "products",
    "size": 0,
    "directoryPath": "images/products",
    "filePath": null,
    "lastModifiedUtc": "2026-08-28T20:20:00Z",
    "isDirectory": true,
    "url": null,
    "mime": null,
    "hasChildren": null
  }
}
```

Other responses: `400` when the normalized name contains a path separator, the parent is a system folder, or a file/folder already occupies the new path; `401`; `403`.

### Delete a folder

```http
DELETE /api/media/folder?path={path}
```

| Query parameter | Type | Required | Constraints |
| --- | --- | --- | --- |
| `path` | string | Yes | Must be a non-root, existing directory and not the configured users or attached-media system folder. |

```bash
curl -X DELETE -H 'Authorization: Bearer ACCESS_TOKEN' \
  'https://cms.example.com/tenant-a/api/media/folder?path=images%2Fproducts'
```

`200 OK`:

```json
{
  "path": "images/products"
}
```

Other responses: `400` for root, protected system folders, or a path that resolves to a non-directory entry; `401`; `403`; `404` when deletion fails or the directory is absent.

### Get directory content

```http
GET /api/media/directories/content
```

| Query parameter | Type | Required | Default/constraints |
| --- | --- | --- | --- |
| `path` | string | No | Media root when omitted or empty. |
| `extensions` | string | No | Space- or comma-separated extensions. Only configured allowed extensions are recognized, compared case-insensitively. If none of the requested values is allowed, no extension filter is applied. |

```bash
curl -G -H 'Authorization: Bearer ACCESS_TOKEN' \
  --data-urlencode 'path=images' \
  --data-urlencode 'extensions=.png,.jpg' \
  'https://cms.example.com/tenant-a/api/media/directories/content'
```

`200 OK` returns direct children, without paging:

```json
{
  "folders": [
    {
      "name": "products",
      "size": 0,
      "directoryPath": "images/products",
      "filePath": null,
      "lastModifiedUtc": "2026-08-28T20:10:00Z",
      "isDirectory": true,
      "url": null,
      "mime": null,
      "hasChildren": false
    }
  ],
  "files": [
    {
      "name": "logo.png",
      "size": 3812,
      "directoryPath": "images",
      "filePath": "images/logo.png",
      "lastModifiedUtc": "2026-08-28T20:15:30Z",
      "isDirectory": false,
      "url": "https://cms.example.com/tenant-a/media/images/logo.png?v=abc123",
      "mime": "image/png",
      "hasChildren": null
    }
  ]
}
```

Other responses: `401`, `403`, `404` when a non-root directory does not exist.

### List files

```http
GET /api/media/files
```

| Query parameter | Type | Required | Default/constraints |
| --- | --- | --- | --- |
| `path` | string | No | Media root when omitted or empty. |
| `extensions` | string | No | Space- or comma-separated configured extensions. If no requested value is allowed, no filter is applied. |
| `skip` | integer | No | `0`; must be at least `0`. |
| `take` | integer | No | `50`; range `1`–`200`. |

```bash
curl -G -H 'Authorization: Bearer ACCESS_TOKEN' \
  --data-urlencode 'path=images' \
  --data-urlencode 'extensions=.png .jpg' \
  'https://cms.example.com/tenant-a/api/media/files'
```

`200 OK` returns the paged media envelope containing direct child files.

Other responses: `400` for invalid paging, `401`, `403`, `404` when a non-root directory does not exist.

### List all media items

```http
GET /api/media/items
```

| Query parameter | Type | Required | Default/constraints |
| --- | --- | --- | --- |
| `extensions` | string | No | Space- or comma-separated configured extensions. Only matching files are returned when a filter is supplied. |
| `skip` | integer | No | `0`; must be at least `0`. |
| `take` | integer | No | `50`; range `1`–`200`. |

```bash
curl -G -H 'Authorization: Bearer ACCESS_TOKEN' \
  --data-urlencode 'extensions=.png,.jpg' \
  --data-urlencode 'take=50' \
  'https://cms.example.com/tenant-a/api/media/items'
```

`200 OK` returns the paged media envelope containing accessible files recursively
below the root. Folders are excluded from both `items` and `totalCount`; paging
applies to files only. Use `GET /api/media/folders` to list folders.

The corresponding CLI commands are:

```bash
pomi media items list                        # Files recursively below the media root
pomi media files list --path assets          # Files directly inside assets
pomi media folders list --path assets        # Folders directly inside assets
```

Other responses: `400` for invalid paging, `401`, `403`.

### Get one file

```http
GET /api/media/file?path={path}
```

| Query parameter | Type | Required | Constraints |
| --- | --- | --- | --- |
| `path` | string | Yes | Existing media file path. |

```bash
curl -G -H 'Authorization: Bearer ACCESS_TOKEN' \
  --data-urlencode 'path=images/logo.png' \
  'https://cms.example.com/tenant-a/api/media/file'
```

`200 OK` returns one file representation:

```json
{
  "name": "logo.png",
  "size": 3812,
  "directoryPath": "images",
  "filePath": "images/logo.png",
  "lastModifiedUtc": "2026-08-28T20:15:30Z",
  "isDirectory": false,
  "url": "https://cms.example.com/tenant-a/media/images/logo.png?v=abc123",
  "mime": "image/png",
  "hasChildren": null
}
```

Other responses: `401`, `403`, `404` for an empty or unknown path.

### Get metadata for several files

```http
GET /api/media/files/metadata?paths={path}&paths={path}
```

| Query parameter | Type | Required | Constraints |
| --- | --- | --- | --- |
| `paths` | array of string | No | Repeat the query key. Blank values are removed and exact duplicates are collapsed. Every remaining path must be authorized. |

```bash
curl -G -H 'Authorization: Bearer ACCESS_TOKEN' \
  --data-urlencode 'paths=images/logo.png' \
  --data-urlencode 'paths=documents/guide.pdf' \
  'https://cms.example.com/tenant-a/api/media/files/metadata'
```

`200 OK` returns an array. Unknown files are omitted; no paths returns `[]`.

```json
[
  {
    "name": "logo.png",
    "size": 3812,
    "directoryPath": "images",
    "filePath": "images/logo.png",
    "lastModifiedUtc": "2026-08-28T20:15:30Z",
    "isDirectory": false,
    "url": "https://cms.example.com/tenant-a/media/images/logo.png?v=abc123",
    "mime": "image/png",
    "hasChildren": null
  }
]
```

Other responses: `401`, `403`.

### Upload a file stream

```http
PUT /api/media/files/content?fileName={name}&path={folder}
Content-Type: application/octet-stream
```

`fileName` must be a base name and cannot contain `/`, `\`, `.`, or `..` path
segments. The fully combined destination path is authorized again with
`ManageMediaFolder` before writing, so a file name cannot escape the permitted
folder.

| Input | Type | Required | Constraints |
| --- | --- | --- | --- |
| `fileName` query | string | Yes | Must not be blank. It is normalized, its extension must be permitted for the caller, and the resulting target must not already exist. |
| `path` query | string | No | Target folder; root when omitted or empty. |
| Request body | binary stream | Yes | Maximum `maxFileSize` bytes from `/api/media/constraints`. Both declared and streamed lengths are enforced. |

```bash
curl -X PUT -H 'Authorization: Bearer ACCESS_TOKEN' \
  -H 'Content-Type: application/octet-stream' \
  --data-binary '@logo.png' \
  'https://cms.example.com/tenant-a/api/media/files/content?path=images&fileName=logo.png'
```

`200 OK` returns the created file representation.

Other responses: `400` Validation Problem Details for a missing file name, disallowed extension, or existing target; `400` Problem Details for storage failures; `401`; `403`; `413` when the body exceeds `maxFileSize`; `500` for an unexpected upload failure.

Example `413`:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.14",
  "status": 413,
  "detail": "The file exceeds the maximum allowed size of 31457280 bytes."
}
```

### Copy a file

```http
POST /api/media/files:copy
Content-Type: application/json
```

| Body property | Type | Required | Constraints |
| --- | --- | --- | --- |
| `oldPath` | string | Yes | Existing source file. |
| `newPath` | string | Yes | Allowed extension and no existing file at the destination. |

```bash
curl -X POST -H 'Authorization: Bearer ACCESS_TOKEN' \
  -H 'Content-Type: application/json' \
  -d '{"oldPath":"images/logo.png","newPath":"archive/logo.png"}' \
  'https://cms.example.com/tenant-a/api/media/files:copy'
```

`200 OK`:

```json
{
  "oldPath": "images/logo.png",
  "newPath": "archive/logo.png",
  "file": {
    "name": "logo.png",
    "size": 3812,
    "directoryPath": "archive",
    "filePath": "archive/logo.png",
    "lastModifiedUtc": "2026-08-28T20:25:00Z",
    "isDirectory": false,
    "url": "https://cms.example.com/tenant-a/media/archive/logo.png?v=def456",
    "mime": "image/png",
    "hasChildren": null
  }
}
```

Other responses: `400` Validation Problem Details for a disallowed extension or occupied destination; `401`; `403`; `404` for an empty path or missing source.

### Move a file

```http
POST /api/media/files:move
Content-Type: application/json
```

The body has the same `oldPath` and `newPath` contract as copy.

```bash
curl -X POST -H 'Authorization: Bearer ACCESS_TOKEN' \
  -H 'Content-Type: application/json' \
  -d '{"oldPath":"images/logo.png","newPath":"archive/logo.png"}' \
  'https://cms.example.com/tenant-a/api/media/files:move'
```

`200 OK` has the copy response shape with the moved file:

```json
{
  "oldPath": "images/logo.png",
  "newPath": "archive/logo.png",
  "file": {
    "name": "logo.png",
    "size": 3812,
    "directoryPath": "archive",
    "filePath": "archive/logo.png",
    "lastModifiedUtc": "2026-08-28T20:25:00Z",
    "isDirectory": false,
    "url": "https://cms.example.com/tenant-a/media/archive/logo.png?v=def456",
    "mime": "image/png",
    "hasChildren": null
  }
}
```

Other responses: `400` Validation Problem Details for a disallowed extension or occupied destination; `401`; `403`; `404` for an empty path or missing source.

### Delete a file

```http
DELETE /api/media/file?path={path}
```

| Query parameter | Type | Required | Constraints |
| --- | --- | --- | --- |
| `path` | string | Yes | Existing media file path. |

```bash
curl -X DELETE -H 'Authorization: Bearer ACCESS_TOKEN' \
  'https://cms.example.com/tenant-a/api/media/file?path=archive%2Flogo.png'
```

`200 OK`:

```json
{
  "path": "archive/logo.png"
}
```

Other responses: `401`, `403`, and `404` for an empty path or when the file-store deletion fails.

### Delete several files

```http
POST /api/media/files:delete
Content-Type: application/json
```

| Body property | Type | Required | Constraints |
| --- | --- | --- | --- |
| `paths` | array of string | No | Defaults to `[]` when omitted. An explicit JSON `null` produces `404`. Every path is authorized before any deletion starts. |

```bash
curl -X POST -H 'Authorization: Bearer ACCESS_TOKEN' \
  -H 'Content-Type: application/json' \
  -d '{"paths":["archive/logo.png","archive/banner.jpg"]}' \
  'https://cms.example.com/tenant-a/api/media/files:delete'
```

`200 OK` echoes the requested paths after all deletions succeed:

```json
{
  "paths": [
    "archive/logo.png",
    "archive/banner.jpg"
  ]
}
```

Other responses: `401`; `403`; `404` if `paths` is null or any deletion fails. Deletions before a failure are not rolled back.

### Move several files

```http
POST /api/media/files:move-batch
Content-Type: application/json
```

Every value in `mediaNames` must be a base name without directory separators
or traversal segments. Before any mutation, the API authorizes every fully
combined source and destination path with `ManageMediaFolder`.

| Body property | Type | Required | Constraints |
| --- | --- | --- | --- |
| `mediaNames` | array of string | Yes | At least one file name. |
| `sourceFolder` | string | Yes | Non-empty. The exact accepted alias `root` is normalized to `""`. |
| `targetFolder` | string | Yes | Non-empty. The exact accepted alias `root` is normalized to `""`. |

```bash
curl -X POST -H 'Authorization: Bearer ACCESS_TOKEN' \
  -H 'Content-Type: application/json' \
  -d '{"mediaNames":["logo.png","banner.jpg"],"sourceFolder":"images","targetFolder":"archive"}' \
  'https://cms.example.com/tenant-a/api/media/files:move-batch'
```

`200 OK`:

```json
{
  "mediaNames": [
    "logo.png",
    "banner.jpg"
  ],
  "sourceFolder": "images",
  "targetFolder": "archive",
  "files": [
    {
      "filePath": "archive/logo.png",
      "url": "https://cms.example.com/tenant-a/media/archive/logo.png?v=abc123"
    },
    {
      "filePath": "archive/banner.jpg",
      "url": "https://cms.example.com/tenant-a/media/archive/banner.jpg?v=def456"
    }
  ]
}
```

`files` contains each successful destination path and direct URL; the existing
`mediaNames`, `sourceFolder`, and `targetFolder` fields are retained.

Other responses: `400` Validation Problem Details when one or more store moves fail; `401`; `403`; `404` for a missing/empty file list or folder. Successful moves before a failure are not rolled back.

### Get a completed TUS upload

```http
GET /api/media/uploads/{uploadId}
```

| Path parameter | Type | Required | Constraints |
| --- | --- | --- | --- |
| `uploadId` | string | Yes | Identifier created by the TUS service. The Media TUS Uploads feature supplies the required metadata store. |

```bash
curl -H 'Authorization: Bearer ACCESS_TOKEN' \
  'https://cms.example.com/tenant-a/api/media/uploads/01J6G8NQH4Y7'
```

`200 OK` returns the completed file representation. Reading a successful result consumes its temporary upload metadata, so a second lookup returns `404`.

Other responses: `401`, `403`, `404` when metadata is absent/incomplete or its media file no longer exists.

## Legacy Media API relationship

The module also maps older `api/media/*` routes such as `GetMediaItems`, `CreateFolder`, `Upload`, and `MoveMediaList`. They are marked `ExcludeFromDescription()` and are not remote-management/OpenAPI operations. They use the configurable `MediaApi` cookie-or-bearer policy, retain legacy parameter and response shapes, and mutation routes apply the legacy antiforgery behavior. Remote clients should use the routes documented above.

The TUS protocol route `/api/media/tus` is supplied by `OrchardCore.Media.Tus`; it is not an OpenAPI-discovered management operation. `/api/media/uploads/{uploadId}` only resolves a completed TUS upload.

## Endpoint coverage and sources

This page covers all **18 distinct OpenAPI-discovered Media route/method operations** mapped by `OrchardCore.Media`. When `OrchardCore.Media.Tus` is enabled, its startup currently registers the completed-upload route a second time.

Source-derived from:

- `OrchardCore.Media/Startup.cs`, `Manifest.cs`, `PermissionProvider.cs`, and `Services/MediaRemoteManagementCapabilityProvider.cs`
- every file under `OrchardCore.Media/Endpoints/Api/`
- `OrchardCore.Media/ViewModels/MediaApiViewModels.cs` and `MediaManagementApiViewModels.cs`
- `OrchardCore.Media.Core/MediaPermissions.cs`
- `MediaAndQueriesOpenApiDiscoveryTests.cs` and `MediaEndpointHelpersTests.cs`

!!! note
    The source currently maps `GET /api/media/uploads/{uploadId}` from both the base Media startup and the TUS feature startup, although its handler dependency is registered by the TUS feature. The documented wire operation is one route/method pair and requires the TUS feature.

The management folder-creation route checks `ManageMedia` before processing
the folder name. An absent or blank name returns a validation problem (`400`)
for an authorized caller; callers without the permission receive `403`.
