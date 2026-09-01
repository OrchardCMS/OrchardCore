using Microsoft.AspNetCore.Http;
using OrchardCore.Security.Options;

namespace OrchardCore.Security.Services;

public class PermissionsHeaderPolicyProvider : HeaderPolicyProvider
{
    private string _policy;

    public override void InitializePolicy()
    {
        if (Options.PermissionsPolicy.Length > 0)
        {
            _policy = string.Join(
                SecurityHeaderDefaults.PermissionsPolicySeparator,
                Options.PermissionsPolicy.Select(FormatDirective));
        }
    }

    public override void ApplyPolicy(HttpContext httpContext)
    {
        if (_policy != null)
        {
            httpContext.Response.Headers[SecurityHeaderNames.PermissionsPolicy] = _policy;
        }
    }

    private static string FormatDirective(string directive)
    {
        // Directives built by AddPermissionsPolicy() always contain '=', but the PermissionsPolicy
        // property accepts arbitrary strings. A directive without '=' is passed through unchanged
        // instead of throwing here, which would fail the tenant pipeline construction. It still
        // serializes as a valid RFC 8941 dictionary member (a bare key means an implicit boolean
        // 'true'), so browsers keep parsing the header and simply ignore the directive.
        var separatorIndex = directive.IndexOf('=');
        if (separatorIndex < 0)
        {
            return directive;
        }

        return directive[..separatorIndex] + "=" + FormatAllowList(directive[(separatorIndex + 1)..]);
    }

    // 'Permissions-Policy' is an RFC 8941 structured field: 'self' and '*' are tokens, origins are
    // quoted strings, and an allowlist with multiple items must be a parenthesized inner list, e.g.
    // camera=(self "https://www.domain.com"). Space-separated unquoted origins make browsers
    // reject the whole header. Values that are already parenthesized are kept as-is.
    private static string FormatAllowList(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var trimmedValue = value.Trim();
        if (trimmedValue.StartsWith('('))
        {
            return trimmedValue;
        }

        var items = trimmedValue.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (items.Length == 1 &&
            (items[0] == PermissionsPolicyOriginValue.Any ||
            items[0] == PermissionsPolicyOriginValue.None ||
            items[0] == PermissionsPolicyOriginValue.Self))
        {
            return items[0];
        }

        return "(" + string.Join(' ', items.Select(item =>
            item == PermissionsPolicyOriginValue.Self || item.StartsWith('"')
                ? item
                : "\"" + item + "\"")) + ")";
    }
}
