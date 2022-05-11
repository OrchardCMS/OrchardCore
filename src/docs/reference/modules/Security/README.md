# Security (`OrchardCore.Security`)

This module adds `HTTP` headers to follow security best practices.

## Security Settings

Enabling the `OrchardCore.Security` module will allow the user to set the following settings:

| Setting | Description |
| --- | --- |
| `ContentSecurityPolicy` | Gets or sets the `Content-Security-Policy` HTTP header. |
| `ContentTypeOptions` | Gets or sets the `X-Content-Type-Options` HTTP header. |
| `FrameOptions` | Gets or sets the `XFrame-Options` HTTP header. |
| `PermissionsPolicy` | Gets or sets the `Permissions-Policy` HTTP header. |
| `ReferrerPolicy` | Gets or sets the `Referrer-Policy` HTTP header. |

!!! note
    Setting the `frame-ancestors` directive in `ContentSecurityPolicy` HTTP header will overrides `FrameOptions`.
