using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.OpenId.Services;
using System.Security.Cryptography.X509Certificates;
using static Orchard.OpenId.Settings.OpenIdSettings;
using System.Collections.Generic;
using Orchard.OpenId.Models;
using CryptoHelper;

namespace Orchard.OpenId.Recipes
{
    public class OpenIdApplicationStep : RecipeExecutionStep
    {
        private readonly IOpenIdApplicationManager _openIdApplicationManager;
        
        public OpenIdApplicationStep(IOpenIdApplicationManager openIdApplicationManager,
            ILogger<OpenIdApplicationStep> logger,
            IStringLocalizer<OpenIdApplicationStep> localizer) : base(logger, localizer)
        {
            _openIdApplicationManager = openIdApplicationManager;
        }

        public override string Name
        {
            get { return "OpenIdApplication"; }
        }

        public override async Task ExecuteAsync(RecipeExecutionContext recipeContext)
        {
            var model = recipeContext.RecipeStep.Step.ToObject<OpenIdApplicationStepModel>();
            var application = new OpenIdApplication();
            if (model.Id != 0)
            {
                application = await _openIdApplicationManager.FindByIdAsync(model.Id.ToString());
            }
            application.ClientId = model.ClientId;
            if (model.ClientSecret != null)
            {
                application.ClientSecret = Crypto.HashPassword(model.ClientSecret);
            }
            application.DisplayName = model.DisplayName;
            application.AllowAuthorizationCodeFlow = model.AllowAuthorizationCodeFlow;
            application.AllowClientCredentialsFlow = model.AllowClientCredentialsFlow;
            application.AllowHybridFlow = model.AllowHybridFlow;
            application.AllowImplicitFlow = model.AllowImplicitFlow;
            application.AllowPasswordFlow = model.AllowPasswordFlow;
            application.AllowRefreshTokenFlow = model.AllowRefreshTokenFlow;
            application.LogoutRedirectUri = model.LogoutRedirectUri;
            application.RedirectUri = model.RedirectUri;
            application.RoleNames = model.RoleNames;
            application.SkipConsent = model.SkipConsent;
            application.Type = model.Type;

            await _openIdApplicationManager.CreateAsync(application);
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