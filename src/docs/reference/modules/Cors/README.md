# CORS (`OrchardCore.Cors`)

The CORS Configuration module configures [Cross-Origin Resource Sharing (CORS)](https://developer.mozilla.org/en-US/docs/Web/HTTP/CORS) policies for a tenant. CORS controls whether a browser can make a cross-origin request to the tenant and expose the response to client-side code.

CORS is enforced by browsers. It is not an authentication mechanism, an authorization mechanism, or a restriction on server-to-server clients.

## Enable the module

Enable the **CORS Configuration** feature (`OrchardCore.Cors`) for each tenant that needs CORS. In the admin, go to **Tools** > **Features**.

After enabling the feature, go to **Settings** > **Security** > **Cross-Origin Resource Sharing**. Access to this page requires the security-critical **Managing CORS Settings** permission (`ManageCorsSettings`), which is granted to the Administrator role by default.

CORS settings are stored in the current tenant's site settings. Policies are isolated between tenants, and the module does not define an `appsettings.json` configuration section.

## Create and apply a policy

Select **Add a policy**, configure the policy, and select **Save**. Saving the settings reloads the current tenant shell so that the updated policies are registered.

The module adds every configured policy to ASP.NET Core CORS by name. It applies one policy as the tenant's default:

- The policy marked **Set as default policy** is applied globally by the CORS middleware.
- If no policy is marked as the default, the first configured policy becomes the default.
- If there are no policies, the middleware does not add CORS response headers.

Define exactly one default policy when configuring multiple policies so that their application does not depend on list order.

A custom module can select another named policy for a controller or action by using ASP.NET Core endpoint metadata:

```csharp
using Microsoft.AspNetCore.Cors;

[EnableCors("PartnerApi")]
public sealed class PartnerApiController : Controller
{
}
```

Use `[DisableCors]` on an endpoint that must not use the global default policy. See [Enable CORS in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/cors) for endpoint-routing behavior.

## Policy options

| Option | Behavior |
| --- | --- |
| **Policy name** | Registers the ASP.NET Core CORS policy under this name. Custom endpoints use this value when selecting a named policy. Use a unique, stable name. |
| **Set as default policy** | Applies this policy globally through the tenant's CORS middleware. Selecting it in the admin clears the default flag from the other policies. |
| **Allow any origin** | Allows every origin and ignores **Allowed origins**. Do not use this option for credentialed requests. |
| **Allowed origins** | Allows only the listed origins when **Allow any origin** is disabled. An origin consists of a scheme, host, and optional port, for example `https://app.example.com` or `https://app.example.com:8443`. Do not include a path or a trailing slash. |
| **Allow any method** | Allows every HTTP method and ignores **Allowed methods**. |
| **Allowed methods** | Allows only the listed methods when **Allow any method** is disabled. Enter HTTP method names such as `GET`, `POST`, `PUT`, or `DELETE`. |
| **Allow any header** | Allows every request header in a CORS preflight request and ignores **Allowed headers**. |
| **Allowed headers** | Allows only the listed request headers when **Allow any header** is disabled, for example `Content-Type` or `Authorization`. |
| **Allow credentials** | Allows a browser to include credentials such as cookies or HTTP authentication information. The client must also opt in to credentialed requests, and normal cookie, authentication, and authorization rules still apply. |
| **Exposed headers** | Adds the listed response headers to `Access-Control-Expose-Headers`, which makes them readable by browser client code. CORS-safelisted response headers are readable without listing them. |

An empty allowed-origins, allowed-methods, or allowed-headers list permits none of those categories when the corresponding **Allow any** option is disabled.

!!! warning
    Using **Allow credentials** and **Allow any origin** together is insecure. The admin rejects this combination, including an `AllowedOrigins` entry of `*`, and does not save the settings. At runtime, a policy configured with both **AllowCredentials** and **AllowAnyOrigin** is not loaded.

## Recipe and deployment support

The policies can be provisioned with the `settings` recipe step because `CorsSettings` is a tenant site-settings property. Enable the feature before importing the settings:

```json
{
  "steps": [
    {
      "name": "feature",
      "enable": [ "OrchardCore.Cors" ]
    },
    {
      "name": "settings",
      "CorsSettings": {
        "Policies": [
          {
            "Name": "Frontend",
            "AllowAnyOrigin": false,
            "AllowedOrigins": [
              "https://app.example.com"
            ],
            "AllowAnyHeader": false,
            "AllowedHeaders": [
              "Content-Type",
              "Authorization"
            ],
            "AllowAnyMethod": false,
            "AllowedMethods": [
              "GET",
              "POST"
            ],
            "AllowCredentials": true,
            "IsDefaultPolicy": true,
            "ExposedHeaders": [
              "X-Pagination-Total"
            ]
          }
        ]
      }
    }
  ]
}
```

When the **Deployment** feature is enabled, add the **Cors settings** step to a deployment plan to export these settings. The generated recipe uses the same `Settings` step and `CorsSettings` structure.

Recipe and deployment imports write site settings directly and do not use the admin form's validation. Validate imported policies, define no more than one default, and reload an already-running tenant shell after changing settings outside the admin UI.

## Troubleshooting

When a browser reports a CORS failure:

1. Confirm that the request is cross-origin. Same-origin requests do not require CORS.
2. Confirm that `OrchardCore.Cors` and the intended policy are configured for the tenant selected by the request's hostname.
3. Confirm that the expected policy is the default, or that the endpoint selects the intended named policy.
4. Match the browser's `Origin` header exactly against **Allowed origins**, including the scheme and port. Remove paths and trailing slashes.
5. For a preflight request, allow the requested method and every header named by `Access-Control-Request-Headers`.
6. If credentials are used, configure a specific origin, enable **Allow credentials**, and configure the browser client and cookies for cross-site use.

Test a preflight response independently of browser error messages:

```bash
curl -i -X OPTIONS "https://cms.example.com/api/items" \
  -H "Origin: https://app.example.com" \
  -H "Access-Control-Request-Method: POST" \
  -H "Access-Control-Request-Headers: authorization,content-type"
```

A rejected CORS request commonly appears to client-side code as a generic network error. Inspect the preflight and actual request in the browser's network tools and check the tenant log for policy-loading warnings.

## Security guidance

- Allow only origins, methods, and request headers required by the client.
- Use HTTPS origins in production and list each trusted origin explicitly when credentials are enabled.
- Treat every allowed credentialed origin as trusted to act with the user's browser credentials.
- Continue to enforce authentication, authorization, input validation, and anti-forgery protections. CORS does not replace them.
- Expose only response headers that client-side code needs.
- Do not use CORS as a way to block non-browser clients; they can send requests without enforcing CORS.

For protocol details, see [Cross-Origin Resource Sharing (CORS) on MDN](https://developer.mozilla.org/en-US/docs/Web/HTTP/CORS) and [Enable CORS in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/cors).

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/OYXFvKWyVGo" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
