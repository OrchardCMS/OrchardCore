using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.OpenId.ViewModels;

namespace OrchardCore.OpenId.Drivers;

/// <summary>
/// Builds the editor of an existing OpenID application, whose fields are grouped in cards by their placement.
/// </summary>
internal sealed class EditOpenIdApplicationDisplayDriver : DisplayDriver<EditOpenIdApplicationViewModel>
{
    internal readonly IStringLocalizer S;

    public EditOpenIdApplicationDisplayDriver(IStringLocalizer<EditOpenIdApplicationDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(EditOpenIdApplicationViewModel model, BuildEditorContext context)
    {
        return Combine(
            Initialize<EditOpenIdApplicationViewModel>("EditOpenIdApplicationGeneral_Edit", m => Populate(model, m))
                .Location(l => l.Zone("Content", "1").Card(S["General"], "10")),
            Initialize<EditOpenIdApplicationViewModel>("EditOpenIdApplicationFlows_Edit", m => Populate(model, m))
                .Location(l => l.Zone("Content", "2").Card(S["Flows"], "20")),

            // The scripts of the page show the redirect settings when a flow uses them.
            Initialize<EditOpenIdApplicationViewModel>("EditOpenIdApplicationAuthorization_Edit", m => Populate(model, m))
                .Location(l => l.Zone("Content", "3").Card(S["Authorization"], "30")),
            Initialize<EditOpenIdApplicationViewModel>("EditOpenIdApplicationScopes_Edit", m => Populate(model, m))
                .Location(l => l.Zone("Content", "4").Card(S["Allowed scopes"], "40")),

            // The scripts of the page show the roles when the client credentials flow is allowed.
            Initialize<EditOpenIdApplicationViewModel>("EditOpenIdApplicationRoles_Edit", m => Populate(model, m))
                .Location(l => l.Zone("Content", "5").Card(S["Client credentials roles"], "50")));
    }

    public override async Task<IDisplayResult> UpdateAsync(EditOpenIdApplicationViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix,
            m => m.DisplayName,
            m => m.Type,
            m => m.ClientId,
            m => m.ClientSecret,
            m => m.AllowIntrospectionEndpoint,
            m => m.AllowRevocationEndpoint,
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
    protected override void BuildPrefix(EditOpenIdApplicationViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }

    private static void Populate(EditOpenIdApplicationViewModel source, EditOpenIdApplicationViewModel model)
    {
        model.Id = source.Id;
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
