using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Email.Services;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Email.Endpoints;

internal static class EmailManagementEndpoints
{
    public static void AddEmailManagementEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("api/email/test", TestAsync)
            .WithName("ApiTestEmailDelivery")
            .WithTags("Email")
            .WithSummary("Sends one bounded plain-text test message using an enabled email provider. Retrying sends another message.")
            .WithCliCommand(new CliOperationMetadata(["email"], "test")
            {
                Capability = "email", InputMode = CliInputMode.Json,
            })
            .DisableAntiforgery()
            .RequireAuthorization(policy => policy.AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api)
                .RequireAuthenticatedUser().AddRequirements(
                    new OrchardCore.Security.PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement),
                    new OrchardCore.Security.PermissionRequirement(EmailPermissions.ManageEmailSettings)))
            .Produces(204).ProducesValidationProblem().ProducesProblem(401).ProducesProblem(403).ProducesProblem(502);
    }

    internal static async Task<IResult> TestAsync(HttpContext context,
        [FromServices] IAuthorizationService authorization,
        [FromServices] IEmailAddressValidator validator,
        [FromServices] IEmailService email,
        [FromBody] EmailDeliveryTestRequest input)
    {
        if (!await authorization.AuthorizeAsync(context.User, EmailPermissions.ManageEmailSettings))
        {
            return context.ApiForbidProblem();
        }
        var errors = new Dictionary<string, string[]>();
        if (input is null)
        {
            return TypedResults.Problem("A test message is required.", statusCode: 400);
        }
        if (string.IsNullOrWhiteSpace(input.To) || input.To.Length > 320 || !validator.Validate(input.To)
            || input.To.IndexOfAny(['\r', '\n', ',', ';']) >= 0)
        {
            errors["to"] = ["Provide one valid recipient email address."];
        }
        if (string.IsNullOrWhiteSpace(input.Subject) || input.Subject.Length > 256 || input.Subject.IndexOfAny(['\r', '\n']) >= 0)
        {
            errors["subject"] = ["Provide a subject of 1 to 256 characters without line breaks."];
        }
        if (string.IsNullOrWhiteSpace(input.Body) || input.Body.Length > 16384)
        {
            errors["body"] = ["Provide a plain-text body of 1 to 16384 characters."];
        }
        if (input.Provider?.Length > 256)
        {
            errors["provider"] = ["The provider name must be at most 256 characters."];
        }
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }
        try
        {
            var result = await email.SendAsync(new MailMessage
            {
                To = input.To, Subject = input.Subject, TextBody = input.Body,
            }, input.Provider, context.RequestAborted);
            // Provider diagnostics can contain credentials or connection details; never echo them.
            return result.Succeeded ? TypedResults.NoContent()
                : TypedResults.Problem("The provider could not send the test message.", statusCode: 502);
        }
        catch (InvalidEmailProviderException)
        {
            return TypedResults.Problem("The selected provider is invalid or disabled.", statusCode: 400);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return TypedResults.Problem("The provider could not send the test message.", statusCode: 502);
        }
    }
}

/// <summary>A bounded delivery test using the same email service as the admin test form.</summary>
public sealed class EmailDeliveryTestRequest
{
    /// <summary>Gets or sets the provider name; omit to use the configured default provider.</summary>
    public string Provider { get; set; }
    /// <summary>Gets or sets the single recipient email address.</summary>
    [Required, StringLength(320)]
    public string To { get; set; }
    /// <summary>Gets or sets the subject, limited to 256 characters.</summary>
    [Required, StringLength(256)]
    public string Subject { get; set; }
    /// <summary>Gets or sets the plain-text message, limited to 16384 characters.</summary>
    [Required, StringLength(16384)]
    public string Body { get; set; }
}
