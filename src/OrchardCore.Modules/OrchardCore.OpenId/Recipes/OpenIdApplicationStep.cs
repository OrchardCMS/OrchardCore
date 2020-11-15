using System;
using System.Linq;
using System.Threading.Tasks;
using OpenIddict.Abstractions;
using OrchardCore.OpenId.Abstractions.Descriptors;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.ViewModels;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.OpenId.Recipes
{
    public class OpenIdApplicationStep : IRecipeStepHandler
    {
        private readonly IOpenIdApplicationManager _applicationManager;

        /// <summary>
        /// This recipe step adds an OpenID Connect app.
        /// </summary>
        public OpenIdApplicationStep(IOpenIdApplicationManager applicationManager)
        {
            _applicationManager = applicationManager;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, "OpenIdApplication", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<OpenIdApplicationStepModel>();

            var descriptor = new OpenIdApplicationDescriptor
            {
                ClientId = model.ClientId,
                ClientSecret = model.ClientSecret,
                ConsentType = model.ConsentType,
                DisplayName = model.DisplayName,
                Type = model.Type
            };

            if (model.AllowLogoutEndpoint)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Logout);

                if (model.PostLogoutRedirectUris.Any())
                {
                    descriptor.PostLogoutRedirectUris.UnionWith(
                        from uri in model.PostLogoutRedirectUris
                        select new Uri(uri, UriKind.Absolute));
                }
            }
            if (model.AllowAuthorizationCodeFlow)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode);
            }
            if (model.AllowClientCredentialsFlow)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials);

                if (model.Roles.Any())
                {
                    descriptor.Roles.UnionWith(model.Roles);
                }
            }
            if (model.AllowImplicitFlow)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.Implicit);
            }
            if (model.AllowPasswordFlow)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.Password);
            }
            if (model.AllowRefreshTokenFlow)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.RefreshToken);
            }
            if (model.AllowAuthorizationCodeFlow || model.AllowImplicitFlow)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Authorization);
            }
            if (model.AllowAuthorizationCodeFlow || model.AllowClientCredentialsFlow ||
                model.AllowPasswordFlow || model.AllowRefreshTokenFlow)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Token);
            }

            if (model.AllowAuthorizationCodeFlow)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.Code);
            }
            if (model.AllowImplicitFlow)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.IdToken);

                if (string.Equals(model.Type, OpenIddictConstants.ClientTypes.Public, StringComparison.OrdinalIgnoreCase))
                {
                    descriptor.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.IdTokenToken);
                    descriptor.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.Token);
                }
            }
            if (model.AllowHybridFlow)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.CodeIdToken);

                if (string.Equals(model.Type, OpenIddictConstants.ClientTypes.Public, StringComparison.OrdinalIgnoreCase))
                {
                    descriptor.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.CodeIdTokenToken);
                    descriptor.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.CodeToken);
                }
            }

            if (model.AllowAuthorizationCodeFlow || model.AllowHybridFlow || model.AllowImplicitFlow)
            {
                if (model.RedirectUris.Any())
                {
                    descriptor.RedirectUris.UnionWith(
                    from uri in model.RedirectUris
                    select new Uri(uri, UriKind.Absolute));
                }
            }

            var application = await _applicationManager.FindByClientIdAsync(descriptor.ClientId);
            if (application != null)
            {
                await _applicationManager.UpdateAsync(application, descriptor);
            }
            else
            {
                await _applicationManager.CreateAsync(descriptor);
            }
        }
    }
}
