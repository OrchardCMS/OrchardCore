# Audit Trail management API

The **Audit Trail** feature (`OrchardCore.AuditTrail`) lets a remote client search the metadata of the recorded events. It advertises the `audit-trail` remote-management capability and contributes the `pomi audit-trail` commands and MCP tools.

These operations are read-only: they don't create, change, trim or delete events. What gets recorded depends on the enabled features and the Audit Trail settings. See the [Audit Trail module](../../modules/AuditTrail/README.md).

## Authentication and permissions

Every operation requires an `Api` scheme bearer token, **Access remote management API** (`AccessRemoteManagement`) and `ViewAuditTrail`. An absent or invalid token returns `401`, a missing permission returns `403`.

## Base route

`/api/audit-trail/events`

## Endpoints

| Method | Endpoint | Operation ID | Pomi command |
| --- | --- | --- | --- |
| `GET` | `/api/audit-trail/events` | `ApiListAuditTrailEvents` | `audit-trail events list` |
| `GET` | `/api/audit-trail/events/by-id?eventId={id}` | `ApiGetAuditTrailEvent` | `audit-trail events show <eventId>` |

Both operations are MCP tools. See [MCP server](../../modules/RemoteManagement/README.md#mcp-server).

## Event representation

```json
{
  "eventId": "7d2k9m4qx",
  "name": "Created",
  "category": "User",
  "correlationId": "3f8a1c6e",
  "userId": "4r7t2w9zb",
  "userName": "admin",
  "createdUtc": "2026-01-15T10:00:00Z"
}
```

The stored event properties, the content snapshots and the client IP addresses are never returned.

## List events

`GET /api/audit-trail/events`

| Location | Name | Type | Required | Description |
| --- | --- | --- | --- | --- |
| Query | `q` | string | No | A filter in the syntax of the admin list, at most 4096 characters. |
| Query | `skip` | integer | No | Zero-based offset. Default `0`, minimum `0`. |
| Query | `take` | integer | No | Page size. Default `50`, range `1`–`200`. |

`q` accepts the terms `id` (correlation identifier), `category`, `event`, `date`, `username`, `userid` and `sort`, and enabled features can add others. Filtering and ordering behave as in the admin list, including the time zone of date expressions:

```bash
pomi audit-trail events list --q 'category:User event:Created' --skip 0 --take 50
pomi audit-trail events list --q 'id:correlation-id'
```

```json
{
  "skip": 0,
  "take": 50,
  "totalCount": 1,
  "items": [
    {
      "eventId": "7d2k9m4qx",
      "name": "Created",
      "category": "User",
      "correlationId": "3f8a1c6e",
      "userId": "4r7t2w9zb",
      "userName": "admin",
      "createdUtc": "2026-01-15T10:00:00Z"
    }
  ]
}
```

Paging values out of range, an offset that doesn't fit a 32-bit integer, or a longer `q`, return `400`. The query service reads whole pages, so a `skip` that isn't a multiple of `take` costs two queries, and new events between them can shift the results.

## Show an event

`GET /api/audit-trail/events/by-id?eventId=7d2k9m4qx`

`200` returns the event. An `eventId` that is missing or longer than 256 characters returns `400`, and an absent event `404`.

## Errors

Failures use Problem Details.

## Sources

- [Endpoint mappings and handlers](https://github.com/OrchardCMS/OrchardCore/blob/main/src/OrchardCore.Modules/OrchardCore.AuditTrail/Endpoints/AuditTrailEndpoints.cs)
- [Audit Trail module](../../modules/AuditTrail/README.md)
