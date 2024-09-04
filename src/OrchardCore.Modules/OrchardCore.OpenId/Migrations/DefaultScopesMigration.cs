using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OpenIddict.Abstractions;
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

            if (await scopeManager.FindByNameAsync(OpenIddictConstants.Scopes.Profile) == null)
            {
                var descriptor = new OpenIdScopeDescriptor
                {
                    DisplayName = S["Profile"],
                    Name = OpenIddictConstants.Scopes.Profile,
                    Description = S["Requests access to the user's default profile information."]
                };

                await scopeManager.CreateAsync(descriptor);
            }

            if (await scopeManager.FindByNameAsync(OpenIddictConstants.Scopes.Email) == null)
            {
                var descriptor = new OpenIdScopeDescriptor
                {
                    DisplayName = S["Email"],
                    Name = OpenIddictConstants.Scopes.Email,
                    Description = S["Requests access to the user's email address. This scope provides the email and email_verified claims, which indicate the user's email address and whether it has been verified."]
                };

                await scopeManager.CreateAsync(descriptor);
            }

            if (await scopeManager.FindByNameAsync(OpenIddictConstants.Scopes.Phone) == null)
            {
                var descriptor = new OpenIdScopeDescriptor
                {
                    DisplayName = S["Phone"],
                    Name = OpenIddictConstants.Scopes.Phone,
                    Description = S["Requests access to the user's phone number. This scope includes the phone_number and phone_number_verified claims, which provide the user's phone number and indicate whether it has been verified."]
                };

                await scopeManager.CreateAsync(descriptor);
            }
        });

        return 1;
    }
}
