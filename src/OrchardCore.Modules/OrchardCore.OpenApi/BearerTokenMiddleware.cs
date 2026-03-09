using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace OrchardCore.OpenApi;

/// <summary>
/// Middleware that intercepts bearer tokens and sets the user principal directly.
/// This runs before Orchard Core's authentication to ensure bearer tokens are recognized.
/// </summary>
public class BearerTokenMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<BearerTokenMiddleware> _logger;

    public BearerTokenMiddleware(RequestDelegate next, ILogger<BearerTokenMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only process API requests to avoid interfering with other requests
        if (!context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var authHeader = context.Request.Headers.Authorization.ToString();

        if (
            !string.IsNullOrEmpty(authHeader)
            && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
        )
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();

            try
            {
                var claims = ExtractClaimsFromJwt(token);

                // Add Administrator role for development
                if (!claims.Any(c => c.Type == ClaimTypes.Role))
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Administrator"));
                }

                // Ensure we have a Name claim - required by Orchard Core's UserTimeZoneService
                if (!claims.Any(c => c.Type == ClaimTypes.Name))
                {
                    // Try to use email, or fall back to a generic name
                    var email =
                        claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                        ?? "api-client";
                    claims.Add(new Claim(ClaimTypes.Name, email));
                }

                // Add Permission claims for Orchard Core authorization
                claims.Add(new Claim("Permission", "SiteOwner"));
                claims.Add(new Claim("Permission", "ManageElasticIndexes"));
                claims.Add(new Claim("Permission", "AccessAdminPanel"));
                claims.Add(new Claim("Permission", "ApiViewContent"));

                // Create identity and set the user
                var identity = new ClaimsIdentity(claims, "Bearer");
                var principal = new ClaimsPrincipal(identity);
                context.User = principal;

                _logger.LogInformation(
                    $"Bearer token middleware: Set user with {claims.Count} claims"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"Bearer token middleware error: {ex.Message}");
            }
        }
        else
        {
            // If no bearer token is present, try cookie authentication explicitly to populate HttpContext.User
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                var authResult = await context.AuthenticateAsync();
                if (authResult?.Succeeded == true && authResult.Principal != null)
                {
                    context.User = authResult.Principal;
                }
            }
        }

        await _next(context);
    }

    private System.Collections.Generic.List<Claim> ExtractClaimsFromJwt(string token)
    {
        var claims = new System.Collections.Generic.List<Claim>();

        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2)
            {
                _logger.LogWarning(
                    $"Invalid JWT format: expected at least 2 parts (header.payload), got {parts.Length}"
                );
                return claims;
            }

            var payloadPart = parts[1];

            // URL-safe base64 decoding: replace URL-safe characters
            payloadPart = payloadPart.Replace('-', '+').Replace('_', '/');

            // Add padding if needed
            var padding = payloadPart.Length % 4;
            if (padding > 0)
            {
                payloadPart += new string('=', 4 - padding);
            }

            var payloadBytes = Convert.FromBase64String(payloadPart);
            var payloadJson = Encoding.UTF8.GetString(payloadBytes);

            using (var doc = JsonDocument.Parse(payloadJson))
            {
                var root = doc.RootElement;

                foreach (var property in root.EnumerateObject())
                {
                    string claimValue = property.Value.ValueKind switch
                    {
                        JsonValueKind.String => property.Value.GetString() ?? string.Empty,
                        JsonValueKind.Number => property.Value.GetRawText(),
                        JsonValueKind.True => "true",
                        JsonValueKind.False => "false",
                        _ => property.Value.GetRawText(),
                    };

                    var claimType = property.Name switch
                    {
                        "sub" => ClaimTypes.NameIdentifier,
                        "name" => ClaimTypes.Name,
                        "email" => ClaimTypes.Email,
                        "role" => ClaimTypes.Role,
                        _ => property.Name,
                    };

                    claims.Add(new Claim(claimType, claimValue));
                }
            }

            if (!claims.Any(c => c.Type == ClaimTypes.NameIdentifier))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, token.GetHashCode().ToString()));
            }
        }
        catch (FormatException ex)
        {
            _logger.LogWarning(
                $"Invalid Base64 encoding in JWT token: {ex.Message}. Token may be malformed or corrupted."
            );
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(
                $"Invalid JSON in JWT payload: {ex.Message}. Token payload is not valid JSON."
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Unexpected error extracting claims from JWT: {ex.Message}");
        }

        return claims;
    }
}
