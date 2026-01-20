using System.Text.Encodings.Web;
using Fluid.Values;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Email;
using OrchardCore.Liquid;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Endpoints.EmailAuthenticator;

public static class SendCode
{
    public const string RouteName = "EmailAuthenticatorSendCode";

    public static IEndpointRouteBuilder AddEmailSendCodeEndpoint<T>(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("TwoFactor-Authenticator/EmailSendCode", HandleAsync<T>)
            .AllowAnonymous()
            .WithName(RouteName)
            .DisableAntiforgery();

        return builder;
    }

    private static async Task<IResult> HandleAsync<T>(
        SignInManager<IUser> signInManager,
        UserManager<IUser> userManager,
        ISiteService siteService,
        IEmailService emailService,
        ILiquidTemplateManager liquidTemplateManager,
        HtmlEncoder htmlEncoder,
        IStringLocalizer<T> S)
    {
        var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
        var errorMessage = S["The email could not be sent. Please attempt to request the code at a later time."];

        if (user == null)
        {
            return TypedResults.BadRequest(new
            {
                success = false,
                message = errorMessage.Value,
            });
        }

        var settings = await siteService.GetSettingsAsync<EmailAuthenticatorLoginSettings>();
        var code = await userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);

        var to = await userManager.GetEmailAsync(user);
        var subject = await GetSubjectAsync(settings, user, code, liquidTemplateManager, htmlEncoder);
        var body = await GetBodyAsync(settings, user, code, liquidTemplateManager, htmlEncoder);
        var result = await emailService.SendAsync(to, subject, body);

        return TypedResults.Ok(new
        {
            success = result.Succeeded,
            message = result.Succeeded
                ? S["A verification code has been sent via email. Please check your email for the code."].Value
                : errorMessage.Value,
        });
    }

    private static Task<string> GetSubjectAsync(
        EmailAuthenticatorLoginSettings settings,
        IUser user,
        string code,
        ILiquidTemplateManager liquidTemplateManager,
        HtmlEncoder htmlEncoder)
    {
        var message = string.IsNullOrWhiteSpace(settings.Subject)
        ? EmailAuthenticatorLoginSettings.DefaultSubject
        : settings.Subject;

        return GetContentAsync(message, user, code, liquidTemplateManager, htmlEncoder);
    }

    private static Task<string> GetBodyAsync(
        EmailAuthenticatorLoginSettings settings,
        IUser user,
        string code,
        ILiquidTemplateManager liquidTemplateManager,
        HtmlEncoder htmlEncoder)
    {
        var message = string.IsNullOrWhiteSpace(settings.Body)
        ? EmailAuthenticatorLoginSettings.DefaultBody
        : settings.Body;

        return GetContentAsync(message, user, code, liquidTemplateManager, htmlEncoder);
    }

    private static async Task<string> GetContentAsync(
        string message,
        IUser user,
        string code,
        ILiquidTemplateManager liquidTemplateManager,
        HtmlEncoder htmlEncoder)
    {
        var result = await liquidTemplateManager.RenderHtmlContentAsync(message, htmlEncoder, null,
            new Dictionary<string, FluidValue>()
            {
                ["User"] = new ObjectValue(user),
                ["Code"] = new StringValue(code),
            });

        using var writer = new StringWriter();
        result.WriteTo(writer, htmlEncoder);

        return writer.ToString();
    }
}
