using System;
using System.Threading;
using System.Threading.Tasks;
using OpenIddict.Core;
using Orchard.OpenId.Models;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using System.Collections.Generic;

namespace Orchard.OpenId.Recipes
{
    public class OpenIdApplicationStep : IRecipeStepHandler
    {
        private readonly OpenIddictApplicationManager<OpenIdApplication> _applicationManager;

        /// <summary>
        /// This recipe step adds an OpenID Connect app.
        /// </summary>
        public OpenIdApplicationStep(OpenIddictApplicationManager<OpenIdApplication> applicationManager)
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
            var application = new OpenIdApplication();
            if (model.Id != 0)
            {
                application = await _applicationManager.FindByIdAsync(model.Id.ToString(), CancellationToken.None);
            }
            application.ClientId = model.ClientId;
            application.DisplayName = model.DisplayName;
            application.AllowAuthorizationCodeFlow = model.AllowAuthorizationCodeFlow;
            application.AllowClientCredentialsFlow = model.AllowClientCredentialsFlow;
            application.AllowHybridFlow = model.AllowHybridFlow;
            application.AllowImplicitFlow = model.AllowImplicitFlow;
            application.AllowPasswordFlow = model.AllowPasswordFlow;
            application.AllowRefreshTokenFlow = model.AllowRefreshTokenFlow;
            application.LogoutRedirectUri = model.LogoutRedirectUri;
            application.RedirectUri = model.RedirectUri;
            application.SkipConsent = model.SkipConsent;
            application.RoleNames = model.RoleNames;
            application.Type = model.Type;

            if (model.Type == ClientType.Confidential)
            {
                await _applicationManager.CreateAsync(application, model.ClientSecret, CancellationToken.None);
            }

            else
            {
                await _applicationManager.CreateAsync(application, CancellationToken.None);
            }
        }
    }

    public class OpenIdApplicationStepModel
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string DisplayName { get; set; }
        public int Id { get; set; }
        public string LogoutRedirectUri { get; set; }
        public string RedirectUri { get; set; }
        public ClientType Type { get; set; }
        public bool SkipConsent { get; set; }
        public List<string> RoleNames { get; set; } = new List<string>();
        public bool AllowPasswordFlow { get; set; }
        public bool AllowClientCredentialsFlow { get; set; }
        public bool AllowAuthorizationCodeFlow { get; set; }
        public bool AllowRefreshTokenFlow { get; set; }
        public bool AllowImplicitFlow { get; set; }
        public bool AllowHybridFlow { get; set; }
    }
}