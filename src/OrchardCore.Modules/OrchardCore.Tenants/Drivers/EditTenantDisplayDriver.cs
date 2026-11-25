using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Tenants.ViewModels;

namespace OrchardCore.Tenants.Drivers;

/// <summary>
/// Builds the editor of a tenant, used to create and to edit it, whose fields are grouped in cards by their placement.
/// </summary>
internal sealed class EditTenantDisplayDriver : DisplayDriver<EditTenantViewModel>
{
    internal readonly IStringLocalizer S;

    public EditTenantDisplayDriver(IStringLocalizer<EditTenantDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(EditTenantViewModel model, BuildEditorContext context)
    {
        var isNew = context.IsNew;

        var results = new List<IDisplayResult>
        {
            Initialize<EditTenantViewModel>("TenantGeneral_Edit", m => Populate(model, m, isNew))
                .Location(l => l.Zone("Content", "1").Card(S["General"], "10")),
            Initialize<EditTenantViewModel>("TenantUrl_Edit", m => Populate(model, m, isNew))
                .Location(l => l.Zone("Content", "2").Card(S["URL"], "20")),
        };

        // The setup presets of an existing tenant can only be changed until it is set up.
        if (isNew || model.Recipes is not null)
        {
            results.Add(Initialize<EditTenantViewModel>("TenantSetup_Edit", m => Populate(model, m, isNew))
                .Location(l => l.Zone("Content", "3").Card(S["Setup presets"], "30")));
        }

        if (model.FeatureProfilesItems?.Count > 0)
        {
            results.Add(Initialize<EditTenantViewModel>("TenantFeatures_Edit", m => Populate(model, m, isNew))
                .Location(l => l.Zone("Content", "4").Card(S["Features"], "40")));
        }

        return Combine(results);
    }

    public override async Task<IDisplayResult> UpdateAsync(EditTenantViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix,
            m => m.Name,
            m => m.Description,
            m => m.Category,
            m => m.RequestUrlPrefix,
            m => m.RequestUrlHost,
            m => m.RecipeName,
            m => m.DatabaseProvider,
            m => m.TablePrefix,
            m => m.ConnectionString,
            m => m.Schema,
            m => m.FeatureProfiles);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller validates and the scripts of the page select.
    protected override void BuildPrefix(EditTenantViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }

    private static void Populate(EditTenantViewModel source, EditTenantViewModel model, bool isNew)
    {
        model.Name = source.Name;
        model.Description = source.Description;
        model.Category = source.Category;
        model.DatabaseProvider = source.DatabaseProvider;
        model.RequestUrlPrefix = source.RequestUrlPrefix;
        model.RequestUrlHost = source.RequestUrlHost;
        model.ConnectionString = source.ConnectionString;
        model.TablePrefix = source.TablePrefix;
        model.Schema = source.Schema;
        model.RecipeName = source.RecipeName;
        model.FeatureProfiles = source.FeatureProfiles;
        model.IsNewTenant = isNew;
        model.FeatureProfilesItems = source.FeatureProfilesItems;
        model.Recipes = source.Recipes;
        model.CanEditDatabasePresets = source.CanEditDatabasePresets;
        model.DatabaseConfigurationPreset = source.DatabaseConfigurationPreset;
        model.DatabaseProviderPreset = source.DatabaseProviderPreset;
        model.RequireTablePrefix = source.RequireTablePrefix;
        model.HasTablePrefixPattern = source.HasTablePrefixPattern;
        model.ResolvedTablePrefix = source.ResolvedTablePrefix;
        model.HasSchemaPattern = source.HasSchemaPattern;
        model.ResolvedSchema = source.ResolvedSchema;
    }
}
