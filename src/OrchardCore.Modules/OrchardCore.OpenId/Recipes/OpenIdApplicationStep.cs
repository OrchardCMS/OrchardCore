using System;
using System.Linq;
using System.Threading.Tasks;
using OpenIddict.Abstractions;
using OrchardCore.OpenId.Abstractions.Descriptors;
using OrchardCore.OpenId.Abstractions.Managers;
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

            var app = await _applicationManager
                .FindByClientIdAsync(model.ClientId);

            var descriptor = new OpenIdApplicationDescriptor();

            var isNew = true;

            if (app != null)
            {
                isNew = false;
                await _applicationManager.PopulateAsync(app, descriptor);
            }

            descriptor.ClientId = model.ClientId;
            descriptor.ClientSecret = model.ClientSecret;
            descriptor.ConsentType = model.ConsentType;
            descriptor.DisplayName = model.DisplayName;
            descriptor.Type = model.Type;

            if (model.AllowAuthorizationCodeFlow)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode);
            }
            if (model.AllowClientCredentialsFlow)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials);
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
            if (model.AllowLogoutEndpoint)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Logout);
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

            if (!string.IsNullOrWhiteSpace(model.PostLogoutRedirectUris))
            {
                descriptor.PostLogoutRedirectUris.UnionWith(
                    model.PostLogoutRedirectUris
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Select(u => new Uri(u, UriKind.Absolute)));
            }

            if (!string.IsNullOrWhiteSpace(model.RedirectUris))
            {
                descriptor.RedirectUris.UnionWith(
                    model.RedirectUris
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Select(u => new Uri(u, UriKind.Absolute)));
            }

            if (model.RoleEntries != null)
            {
                descriptor.Roles.UnionWith(
                    model.RoleEntries
                        .Select(role => role.Name));
            }

            if (model.ScopeEntries != null)
            {
                descriptor.Permissions.UnionWith(
                    model.ScopeEntries
                        .Select(scope => OpenIddictConstants.Permissions.Prefixes.Scope + scope.Name));
            }

            if (isNew)
            {
                await _applicationManager.CreateAsync(descriptor);
            }
            else
            {
                await _applicationManager.UpdateAsync(app, descriptor);
            }
        }
    }
}
