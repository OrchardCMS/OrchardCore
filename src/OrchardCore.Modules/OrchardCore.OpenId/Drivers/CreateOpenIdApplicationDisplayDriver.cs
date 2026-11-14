using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.OpenId.ViewModels;

namespace OrchardCore.OpenId.Drivers;

/// <summary>
/// Builds the editor of a new OpenID application, whose fields are grouped in cards by their placement.
/// </summary>
internal sealed class CreateOpenIdApplicationDisplayDriver : DisplayDriver<CreateOpenIdApplicationViewModel>
{
    internal readonly IStringLocalizer S;

    public CreateOpenIdApplicationDisplayDriver(IStringLocalizer<CreateOpenIdApplicationDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(CreateOpenIdApplicationViewModel model, BuildEditorContext context)
    {
        return Combine(
            Initialize<CreateOpenIdApplicationViewModel>("CreateOpenIdApplicationGeneral_Edit", m => Populate(model, m))
                .Location(l => l.Zone("Content", "1").Card(S["General"], "10")),
            Initialize<CreateOpenIdApplicationViewModel>("CreateOpenIdApplicationFlows_Edit", m => Populate(model, m))
                .Location(l => l.Zone("Content", "2").Card(S["Flows"], "20")),

            // The scripts of the page show the redirect settings when a flow uses them.
            Initialize<CreateOpenIdApplicationViewModel>("CreateOpenIdApplicationAuthorization_Edit", m => Populate(model, m))
                .Location(l => l.Zone("Content", "3").Card(S["Authorization"], "30")),
            Initialize<CreateOpenIdApplicationViewModel>("CreateOpenIdApplicationScopes_Edit", m => Populate(model, m))
                .Location(l => l.Zone("Content", "4").Card(S["Allowed scopes"], "40")),

            // The scripts of the page show the roles when the client credentials flow is allowed.
            Initialize<CreateOpenIdApplicationViewModel>("CreateOpenIdApplicationRoles_Edit", m => Populate(model, m))
                .Location(l => l.Zone("Content", "5").Card(S["Client credentials roles"], "50")));
    }

    public override async Task<IDisplayResult> UpdateAsync(CreateOpenIdApplicationViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix,
            m => m.DisplayName,
            m => m.Type,
            m => m.ClientId,
            m => m.ClientSecret,
            m => m.AllowAuthorizationCodeFlow,
            m => m.AllowImplicitFlow,
            m => m.AllowHybridFlow,
            m => m.AllowPasswordFlow,
            m => m.AllowClientCredentialsFlow,
            m => m.AllowRefreshTokenFlow,
            m => m.RedirectUris,
            m => m.AllowLogoutEndpoint,
            m => m.PostLogoutRedirectUris,
            m => m.RequireProofKeyForCodeExchange,
            m => m.RequirePushedAuthorizationRequests,
            m => m.ConsentType,
            m => m.ScopeEntries,
            m => m.RoleEntries);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller validates and the scripts of the page select.
    protected override void BuildPrefix(CreateOpenIdApplicationViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }

    private static void Populate(CreateOpenIdApplicationViewModel source, CreateOpenIdApplicationViewModel model)
    {
        model.ClientId = source.ClientId;
        model.DisplayName = source.DisplayName;
        model.RedirectUris = source.RedirectUris;
        model.PostLogoutRedirectUris = source.PostLogoutRedirectUris;
        model.Type = source.Type;
        model.ConsentType = source.ConsentType;
        model.ClientSecret = source.ClientSecret;
        model.RoleEntries.AddRange(source.RoleEntries);
        model.ScopeEntries.AddRange(source.ScopeEntries);
        model.AllowPasswordFlow = source.AllowPasswordFlow;
        model.AllowClientCredentialsFlow = source.AllowClientCredentialsFlow;
        model.AllowAuthorizationCodeFlow = source.AllowAuthorizationCodeFlow;
        model.AllowRefreshTokenFlow = source.AllowRefreshTokenFlow;
        model.AllowHybridFlow = source.AllowHybridFlow;
        model.AllowImplicitFlow = source.AllowImplicitFlow;
        model.AllowLogoutEndpoint = source.AllowLogoutEndpoint;
        model.AllowIntrospectionEndpoint = source.AllowIntrospectionEndpoint;
        model.AllowRevocationEndpoint = source.AllowRevocationEndpoint;
        model.RequireProofKeyForCodeExchange = source.RequireProofKeyForCodeExchange;
        model.RequirePushedAuthorizationRequests = source.RequirePushedAuthorizationRequests;
    }
}
