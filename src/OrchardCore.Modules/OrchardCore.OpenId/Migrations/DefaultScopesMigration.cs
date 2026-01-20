using Microsoft.Extensions.DependencyInjection;
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
            var scopeManager = shellScope.ServiceProvider.GetRequiredService<IOpenIdScopeManager>();

            if (await scopeManager.FindByNameAsync(OpenIddictConstants.Scopes.Email) == null)
            {
                var descriptor = new OpenIdScopeDescriptor
                {
                    DisplayName = "Email",
                    Name = OpenIddictConstants.Scopes.Email,
                    Description = "Requests access to the user's email address. This scope provides the email and email_verified claims, which indicate the user's email address and whether it has been verified.",
                };

                await scopeManager.CreateAsync(descriptor);
            }

            if (await scopeManager.FindByNameAsync(OpenIddictConstants.Scopes.Profile) == null)
            {
                var descriptor = new OpenIdScopeDescriptor
                {
                    DisplayName = "Profile",
                    Name = OpenIddictConstants.Scopes.Profile,
                    Description = "Requests access to the user's default profile information.",
                };
                await scopeManager.CreateAsync(descriptor);
            }

            if (await scopeManager.FindByNameAsync(OpenIddictConstants.Scopes.Phone) == null)
            {
                var descriptor = new OpenIdScopeDescriptor
                {
                    DisplayName = "Phone",
                    Name = OpenIddictConstants.Scopes.Phone,
                    Description = "Requests access to the user's phone number. This scope includes the phone_number and phone_number_verified claims, which provide the user's phone number and indicate whether it has been verified.",
                };

                await scopeManager.CreateAsync(descriptor);
            }

            if (await scopeManager.FindByNameAsync(OpenIddictConstants.Scopes.Roles) == null)
            {
                var descriptor = new OpenIdScopeDescriptor
                {
                    DisplayName = "Roles",
                    Name = OpenIddictConstants.Scopes.Roles,
                    Description = "Requests access to the user's roles.",
                };

                await scopeManager.CreateAsync(descriptor);
            }
        });

        return 2;
    }

#pragma warning disable CA1822 // Mark members as static
    public int UpdateFrom1()
#pragma warning restore CA1822 // Mark members as static
    {
        ShellScope.AddDeferredTask(async shellScope =>
        {
            var scopeManager = shellScope.ServiceProvider.GetRequiredService<IOpenIdScopeManager>();

            if (await scopeManager.FindByNameAsync(OpenIddictConstants.Scopes.Roles) == null)
            {
                var descriptor = new OpenIdScopeDescriptor
                {
                    DisplayName = "Roles",
                    Name = OpenIddictConstants.Scopes.Roles,
                    Description = "Requests access to the user's roles.",
                };

                await scopeManager.CreateAsync(descriptor);
            }
        });

        return 2;
    }
}
