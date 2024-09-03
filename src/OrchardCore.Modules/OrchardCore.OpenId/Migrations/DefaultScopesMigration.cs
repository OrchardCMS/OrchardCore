using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.OpenId.Abstractions.Descriptors;
using OrchardCore.OpenId.Abstractions.Managers;

namespace OrchardCore.OpenId.Migrations;

public sealed class DefaultScopesMigration : DataMigration
{
#pragma warning disable CA1822 // Mark members as static
    public int Create()
#pragma warning restore CA1822 // Mark members as static
    {
        ShellScope.AddDeferredTask(async shellScope =>
        {
            var S = shellScope.ServiceProvider.GetService<IStringLocalizer<DefaultScopesMigration>>();
            var scopeManager = shellScope.ServiceProvider.GetRequiredService<IOpenIdScopeManager>();

            var existingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            await foreach (var scope in scopeManager.ListAsync())
            {
                existingNames.Add(await scopeManager.GetNameAsync(scope));
            }

            if (!existingNames.Contains("openid"))
            {
                var descriptor = new OpenIdScopeDescriptor
                {
                    DisplayName = S["OpenID"],
                    Name = "openid",
                    Description = S["This scope is required for all OpenID Connect requests. It indicates that the client wants to authenticate the user and obtain an ID token. This scope enables the OpenID Connect protocol and allows for the retrieval of the user's identity information."]
                };

                await scopeManager.CreateAsync(descriptor);
            }

            if (!existingNames.Contains("profile"))
            {
                var descriptor = new OpenIdScopeDescriptor
                {
                    DisplayName = S["Profile"],
                    Name = "profile",
                    Description = S["Requests access to the user's default profile information."]
                };

                await scopeManager.CreateAsync(descriptor);
            }

            if (!existingNames.Contains("email"))
            {
                var descriptor = new OpenIdScopeDescriptor
                {
                    DisplayName = S["Email"],
                    Name = "email",
                    Description = S["Requests access to the user's email address. This scope provides the email and email_verified claims, which indicate the user's email address and whether it has been verified."]
                };

                await scopeManager.CreateAsync(descriptor);
            }

            if (!existingNames.Contains("phone"))
            {
                var descriptor = new OpenIdScopeDescriptor
                {
                    DisplayName = S["Phone"],
                    Name = "phone",
                    Description = S["Requests access to the user's phone number. This scope includes the phone_number and phone_number_verified claims, which provide the user's phone number and indicate whether it has been verified."]
                };

                await scopeManager.CreateAsync(descriptor);
            }

            if (!existingNames.Contains("offline_access"))
            {
                var descriptor = new OpenIdScopeDescriptor
                {
                    DisplayName = S["Offline Access"],
                    Name = "offline_access",
                    Description = S["Requests a refresh token to allow the client to obtain new access tokens without requiring user interaction. This scope is used to obtain long-lived refresh tokens, enabling the client to access resources even when the user is not actively interacting with the application."]
                };

                await scopeManager.CreateAsync(descriptor);
            }
        });

        return 1;
    }
}
