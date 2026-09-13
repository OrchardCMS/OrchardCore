using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.OpenId.ViewModels;

namespace OrchardCore.OpenId.Controllers;

/// <summary>Accepts automatic registration of public MCP OAuth clients.</summary>
[Feature("OrchardCore.OpenId.RemoteManagement.Mcp")]
public sealed class McpClientRegistrationController : Controller
{
    private readonly McpClientRegistrationService _registration;
    private readonly McpClientRegistrationLimiter _limiter;
    private readonly RemoteManagementConfigurationService _configuration;
    private readonly IOptions<RemoteManagementMcpOptions> _options;

    /// <summary>Initializes the McpClientRegistrationController service.</summary>
    public McpClientRegistrationController(McpClientRegistrationService registration, McpClientRegistrationLimiter limiter, RemoteManagementConfigurationService configuration, IOptions<RemoteManagementMcpOptions> options)
    {
        _registration = registration;
        _limiter = limiter;
        _configuration = configuration;
        _options = options;
    }

    /// <summary>Registers client metadata using RFC 7591; user authorization happens separately.</summary>
    [HttpPost("connect/mcp/register"), AllowAnonymous, IgnoreAntiforgeryToken]
    [Consumes("application/json"), RequestSizeLimit(16384)]
    public async Task<IActionResult> Register([FromBody] McpClientRegistrationRequest request, CancellationToken cancellationToken)
    {
        Response.Headers.CacheControl = "no-store";
        Response.Headers.Pragma = "no-cache";
        if (!_options.Value.AllowDynamicClientRegistration)
        {
            return NotFound();
        }

        using var permit = _limiter.AttemptAcquire();
        if (!permit.IsAcquired)
        {
            Response.Headers.RetryAfter = "60";
            return StatusCode(StatusCodes.Status429TooManyRequests, new { error = "temporarily_unavailable", error_description = "Try _registration again later." });
        }

        if (!(await _configuration.GetStatusAsync()).IsReady)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "temporarily_unavailable", error_description = "An administrator must configure Remote Management authentication first." });
        }

        if (!ModelState.IsValid || request is null)
        {
            return BadRequest(new { error = "invalid_client_metadata", error_description = "Provide valid MCP client metadata as a JSON object." });
        }

        try
        {
            var application = await _registration.RegisterAsync(request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, new
            {
                client_id = application.ClientId,
                client_id_issued_at = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                client_name = application.DisplayName,
                redirect_uris = application.RedirectUris.Select(uri => uri.AbsoluteUri).ToArray(),
                grant_types = request.GrantTypes,
                response_types = request.ResponseTypes,
                token_endpoint_auth_method = "none",
                scope = request.Scope ?? "orchardcore.management",
            });
        }
        catch (McpRedirectUriException exception)
        {
            return BadRequest(new { error = "invalid_redirect_uri", error_description = exception.Message });
        }
        catch (ValidationException exception)
        {
            return BadRequest(new { error = "invalid_client_metadata", error_description = exception.Message });
        }
        catch (McpRegistrationCapacityException)
        {
            Response.Headers.RetryAfter = "60";
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "temporarily_unavailable", error_description = "Client _registration capacity is unavailable. Contact the tenant administrator." });
        }
    }
}
