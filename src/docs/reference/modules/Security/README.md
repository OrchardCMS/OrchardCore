# Security (`OrchardCore.Security`)

This module adds `HTTP` headers to follow security best practices.

## Security Settings

Enabling the `OrchardCore.Security` module will allow the user to set the following settings:

| Setting                 | Description                                             |
|-------------------------|---------------------------------------------------------|
| `ContentSecurityPolicy` | Gets or sets the `Content-Security-Policy` HTTP header. |
| `ContentTypeOptions`    | The module emits `X-Content-Type-Options: nosniff`. |
| `PermissionsPolicy`     | Gets or sets the `Permissions-Policy` HTTP header.      |
| `ReferrerPolicy`        | Gets or sets the `Referrer-Policy` HTTP header.         |

!!! note
    The `Content-Security-Policy` HTTP header contains the `frame-ancestors` directive which obsoleted the `X-Frame-Options` HTTP header.

## Recipe Configuration

Security settings can be configured using the `Settings` recipe step:

```json
{
  "steps": [
    {
      "name": "settings",
      "SecuritySettings": {
        "ContentTypeOptions": "nosniff",
        "ReferrerPolicy": "no-referrer",
        "ContentSecurityPolicy": {
          "default-src": "'self'",
          "script-src": "'self' 'unsafe-inline'"
        },
        "PermissionsPolicy": {
          "camera": "()",
          "microphone": "()"
        }
      }
    }
  ]
}
```

| Property                | Type   | Description                                                       |
|-------------------------|--------|-------------------------------------------------------------------|
| `ContentTypeOptions`    | String | The X-Content-Type-Options header value.                          |
| `ReferrerPolicy`        | String | The Referrer-Policy header value.                                 |
| `ContentSecurityPolicy` | Object | The Content-Security-Policy header directives as key-value pairs. |
| `PermissionsPolicy`     | Object | The Permissions-Policy header directives as key-value pairs.      |

## Security Settings Configuration

The `OrchardCore.Security` module allows the user to use configuration values to override the `AdminSettings` by calling `ConfigureSecuritySettings()` extension method.

The following configuration values can be customized:

```json
{
  "OrchardCore_Security": {
    "ContentSecurityPolicy": {},
    "PermissionsPolicy": {
      "fullscreen": "self"
    },
    "ReferrerPolicy": "no-referrer"
  }
}
```

For more information please refer to [Configuration](../Configuration/README.md).

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/nYfNq8sTIAg" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

## Remote management

The `security-headers` [typed settings section](../../api/settings/README.md)
is available when `OrchardCore.Security` is enabled. Reading, discovering and updating
it requires `ManageSecurityHeadersSettings`, in addition to remote-management access.
The existing admin editor and the section share validation and change detection.
A changed update requests a tenant reload; an unchanged retry does not.

```bash
pomi settings sections schema security-headers
pomi settings sections show security-headers
pomi settings sections update security-headers --stdin <<'JSON'
{
  "contentSecurityPolicy": {
    "default-src": "'self'",
    "img-src": "'self' data:"
  },
  "permissionsPolicy": {
    "camera": "self"
  },
  "referrerPolicy": "strict-origin"
}
JSON
```

All three properties are optional. Omission preserves the current value. A supplied
policy object replaces its entire map; `{}` removes that policy. A null policy
object is invalid. Unknown top-level properties and incorrect value types return
validation errors without saving any part of the update.

CSP directive values are strings or null. The existing settings model removes null
values except for `sandbox` and `upgrade-insecure-requests`, where null enables the
flag directive. `upgrade-insecure-requests` always normalizes to null. Permissions
policy values must be strings; the existing editor's `()` sentinel removes that
directive from stored settings. An omitted permissions directive is subject to the
browser's default behavior; omission does not explicitly deny the permission.

Directive names start with an ASCII letter and contain ASCII letters, digits or
hyphens. Values use printable ASCII without control characters or the map's
separator (`;` for CSP, `,` for Permissions Policy). This validation does not check
browser support for individual directives or assess the strength of a policy.
Referrer policy accepts `no-referrer`, `no-referrer-when-downgrade`, `origin`,
`origin-when-cross-origin`, `same-origin`, `strict-origin`,
`strict-origin-when-cross-origin`, or `unsafe-url`.

The section exposes only these three policy properties. The module continues to
emit `X-Content-Type-Options: nosniff`; this fixed header is not remotely editable.
With `ConfigureSecuritySettings()`, readback contains the effective configured
policies, `source: configuration` and `isReadOnly: true`. Remote updates return
HTTP 409. The admin editor retains its existing ability to save underlying tenant
settings while configuration takes precedence.

The generated CLI and MCP tools use the same settings endpoints and permissions.
Disabling the Security feature removes this section without removing other enabled
settings sections. Verify the response headers after applying a policy, since the
policy also applies to the tenant's admin and API responses.
