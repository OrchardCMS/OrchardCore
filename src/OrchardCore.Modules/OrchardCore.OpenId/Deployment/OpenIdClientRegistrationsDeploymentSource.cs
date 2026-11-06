using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.OpenId.Recipes;
using OrchardCore.OpenId.Services;

namespace OrchardCore.OpenId.Deployment;

public sealed class OpenIdClientRegistrationsDeploymentSource
    : DeploymentSourceBase<OpenIdClientRegistrationsDeploymentStep>
{
    private readonly IOpenIdClientRegistrationService _registrationService;

    public OpenIdClientRegistrationsDeploymentSource(IOpenIdClientRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    protected override async Task ProcessAsync(OpenIdClientRegistrationsDeploymentStep step, DeploymentPlanResult result)
    {
        var registrations = new JsonArray();

        foreach (var registration in (await _registrationService.GetRegistrationsAsync()).OrderBy(registration => registration.Name))
        {
            // The client secret is protected with the keys of this tenant, so it's never exported: the target keeps
            // its own secret, and a secret stored in the Secrets module is referenced by its name.
            registrations.Add(JObject.FromObject(new OpenIdClientRegistrationStepModel
            {
                Name = registration.Name,
                DisplayName = registration.DisplayName,
                Authority = registration.Authority?.AbsoluteUri,
                ClientId = registration.ClientId,
                ClientSecretSecretName = registration.ClientSecretSecretName,
                SigningKeySecretName = registration.SigningKeySecretName,
                CallbackPath = registration.CallbackPath,
                SignedOutCallbackPath = registration.SignedOutCallbackPath,
                SignedOutRedirectUri = registration.SignedOutRedirectUri,
                Scopes = registration.Scopes is null ? null : string.Join(' ', registration.Scopes),
                ResponseType = registration.ResponseType,
                ResponseMode = registration.ResponseMode,
                StoreExternalTokens = registration.StoreExternalTokens,
                Parameters = registration.Parameters,
            }));
        }

        result.Steps.Add(new JsonObject
        {
            ["name"] = OpenIdClientRegistrationsStep.RecipeStepName,
            ["Registrations"] = registrations,
        });
    }
}
