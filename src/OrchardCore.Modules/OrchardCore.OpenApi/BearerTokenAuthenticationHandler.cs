using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrchardCore.OpenApi;

/// <summary>
/// DEPRECATED: This authentication handler is no longer used.
/// 
/// The authentication scheme approach did not work properly with Orchard Core's multi-tenant architecture.
/// The request pipeline did not automatically invoke the authentication handler for bearer tokens.
/// 
/// SOLUTION: Bearer token authentication is now handled by BearerTokenMiddleware.cs which runs
/// early in the middleware pipeline (before Orchard Core's auth) to intercept and process bearer tokens.
/// 
/// This file is kept for historical reference and documentation purposes only.
/// If you need to revisit the authentication scheme approach, see the git history or this file as reference.
/// 
/// See BearerTokenMiddleware.cs for the working implementation.
/// </summary>
public sealed class BearerTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ILogger<BearerTokenAuthenticationHandler> _logger;

    public BearerTokenAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory loggerFactory,
        UrlEncoder encoder,
        ILogger<BearerTokenAuthenticationHandler> logger)
        : base(options, loggerFactory, encoder)
    {
        _logger = logger;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeader = Request.Headers.Authorization.ToString();

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("No bearer token found in Authorization header");
            return AuthenticateResult.NoResult();
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();

        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogDebug("Bearer token is empty");
            return AuthenticateResult.Fail("Bearer token is empty");
        }

        try
        {
            // Extract claims from JWT without validation (for development)
            // In production, validate the JWT signature against OpenIddict's public keys
            var claims = ExtractClaimsFromJwt(token);

            // Add a claim to indicate this is authenticated via bearer token
            claims.Add(new Claim(ClaimTypes.Authentication, "Bearer"));

            // If no subject/user identifier, create one from token hash
            if (!claims.Any(c => c.Type == ClaimTypes.NameIdentifier))
            {
                var tokenHash = token.GetHashCode().ToString();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, tokenHash));
            }

            // Ensure at least Administrator role for API access (development only)
            if (!claims.Any(c => c.Type == ClaimTypes.Role))
            {
                _logger.LogWarning("No roles found in token; adding Administrator role for development");
                claims.Add(new Claim(ClaimTypes.Role, "Administrator"));
            }
            
            // Orchard Core uses Permission claims for authorization
            // For Administrator role, grant all permissions by adding a special claim
            if (claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Administrator"))
            {
                _logger.LogInformation("Administrator role detected, adding SiteOwner permission claim");
                // Add a Permission claim that grants access to all permissions
                // The "SiteOwner" permission is a special permission that grants all other permissions in Orchard Core
                claims.Add(new Claim("Permission", "SiteOwner"));
                
                // Also try adding common API permissions explicitly
                claims.Add(new Claim("Permission", "ManageElasticIndexes"));
                claims.Add(new Claim("Permission", "AccessAdminPanel"));
                claims.Add(new Claim("Permission", "ViewContent"));
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);

            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            _logger.LogInformation($"Bearer token authenticated. Claims: {string.Join("; ", claims.Select(c => $"{c.Type}={c.Value}"))}");
            _logger.LogInformation($"Identity.IsAuthenticated: {identity.IsAuthenticated}, AuthenticationType: {identity.AuthenticationType}");
            return AuthenticateResult.Success(ticket);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Bearer token validation failed: {ex.Message}\n{ex.StackTrace}");
            return AuthenticateResult.Fail($"Bearer token validation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Extracts claims from a JWT token without signature validation.
    /// WARNING: This is only suitable for development. In production, validate the signature!
    /// </summary>
    private List<Claim> ExtractClaimsFromJwt(string token)
    {
        var claims = new List<Claim>();

        try
        {
            // JWT format: Header.Payload.Signature
            var parts = token.Split('.');
            if (parts.Length < 2)
            {
                _logger.LogWarning("Invalid JWT format - not enough parts");
                return claims;
            }

            // Decode the payload (second part)
            var payloadPart = parts[1];
            
            // Add padding if necessary
            var padding = payloadPart.Length % 4;
            if (padding > 0)
            {
                payloadPart += new string('=', 4 - padding);
            }

            var payloadBytes = Convert.FromBase64String(payloadPart);
            var payloadJson = Encoding.UTF8.GetString(payloadBytes);
            
            // Parse JSON to extract claims
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
                        JsonValueKind.Array => JsonSerializer.Serialize(property.Value),
                        _ => property.Value.GetRawText()
                    };

                    // Map standard JWT claims to .NET claim types
                    var claimType = property.Name switch
                    {
                        "sub" => ClaimTypes.NameIdentifier,
                        "name" => ClaimTypes.Name,
                        "email" => ClaimTypes.Email,
                        "role" => ClaimTypes.Role,
                        "roles" when property.Value.ValueKind == JsonValueKind.Array => ClaimTypes.Role,
                        _ => property.Name
                    };

                    // Handle array claims (like roles)
                    if (property.Name == "roles" && property.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var role in property.Value.EnumerateArray())
                        {
                            var roleValue = role.GetString();
                            if (!string.IsNullOrEmpty(roleValue))
                            {
                                claims.Add(new Claim(ClaimTypes.Role, roleValue));
                            }
                        }
                    }
                    else
                    {
                        claims.Add(new Claim(claimType, claimValue));
                    }
                }
            }

            _logger.LogDebug($"Extracted {claims.Count} claims from JWT token");
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error extracting claims from JWT: {ex.Message}");
            // Return whatever claims we managed to extract
        }

        // Ensure we have at least the authentication method claim
        if (!claims.Any(c => c.Type == ClaimTypes.Authentication))
        {
            claims.Add(new Claim(ClaimTypes.Authentication, "Bearer"));
        }

        return claims;
    }
}

