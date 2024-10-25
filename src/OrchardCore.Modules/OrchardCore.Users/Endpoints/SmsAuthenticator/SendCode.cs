using System.Text.Encodings.Web;
using Fluid.Values;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Liquid;
using OrchardCore.Settings;
using OrchardCore.Sms;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Endpoints.SmsAuthenticator;

public static class SendCode
{
    public const string RouteName = "SmsAuthenticatorSendCode";

    public static IEndpointRouteBuilder AddSmsSendCodeEndpoint<T>(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("TwoFactor-Authenticator/SmsSendCode", HandleAsync<T>)
            .AllowAnonymous()
            .WithName(RouteName)
            .DisableAntiforgery();

        return builder;
    }

    private static async Task<IResult> HandleAsync<T>(
        SignInManager<IUser> signInManager,
        UserManager<IUser> userManager,
        ISiteService siteService,
        ISmsService smsService,
        IOptions<IdentityOptions> identityOptions,
        ILiquidTemplateManager liquidTemplateManager,
        HtmlEncoder htmlEncoder,
        IStringLocalizer<T> S)
    {
        var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
        var errorMessage = S["The SMS message could not be sent. Please attempt to request the code at a later time."];

        if (user == null)
        {
            return TypedResults.BadRequest(new
            {
                success = false,
                message = errorMessage.Value,
            });
        }

        var settings = await siteService.GetSettingsAsync<SmsAuthenticatorLoginSettings>();
        var code = await userManager.GenerateTwoFactorTokenAsync(user, identityOptions.Value.Tokens.ChangePhoneNumberTokenProvider);

        var message = new SmsMessage()
        {
            To = await userManager.GetPhoneNumberAsync(user),
            Body = await GetBodyAsync(settings, user, code, liquidTemplateManager, htmlEncoder),
        };

        var result = await smsService.SendAsync(message);

        return TypedResults.Ok(new
        {
            success = result.Succeeded,
            message = result.Succeeded ? S["A verification code has been sent to your phone number. Please check your device for the code."].Value
            : errorMessage.Value,
        });
    }

    private static Task<string> GetBodyAsync(
        SmsAuthenticatorLoginSettings settings,
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
