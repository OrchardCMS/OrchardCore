using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using OrchardCore.AdminMenu.Services;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Json;
using OrchardCore.Navigation;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.AdminMenu.Recipes;

/// <summary>
/// This recipe step creates a set of admin menus.
/// </summary>
public sealed class AdminMenuStep : NamedRecipeStepHandler
{
    private readonly IAdminMenuService _adminMenuService;
    private readonly JsonSerializerOptions _serializationOptions;
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public AdminMenuStep(
        IAdminMenuService adminMenuService,
        IOptions<DocumentJsonSerializerOptions> serializationOptions,
        IContentDefinitionManager contentDefinitionManager)
        : base("AdminMenu")
    {
        _adminMenuService = adminMenuService;

        // The recipe step contains polymorphic types (menu items) which need to be resolved.
        _serializationOptions = serializationOptions.Value.SerializerOptions;

        _contentDefinitionManager = contentDefinitionManager;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<AdminMenuStepModel>(_serializationOptions);

        foreach (var token in model.Data.Cast<JsonObject>())
        {
            // Migrate admin menus recipes (created with pre-3.0 libraries) to the 3.0 format. Probably to remove on future release.
            await MigrateAdminContentTypesAdminNode(token);

            var adminMenu = token.ToObject<Models.AdminMenu>(_serializationOptions);

            // When the id is not supplied generate an id, otherwise replace the menu if it exists, or create a new menu.
            if (string.IsNullOrEmpty(adminMenu.Id))
            {
                adminMenu.Id = IdGenerator.GenerateId();
            }

            // Migrate admin menus recipes (created with pre-3.0 libraries) to the 3.0 format. Probably to remove on future release.
            MigrateAdminNode(adminMenu);

            await _adminMenuService.SaveAsync(adminMenu);
        }
    }

    private static void MigrateAdminNode(Models.AdminMenu menu)
    {
        foreach (var menuItem in menu.MenuItems)
        {
            if (string.IsNullOrEmpty(menuItem.MenuName))
            {
                menuItem.MenuName = menu.Name;
            }
            MigrateMenuItems(menu, menuItem.Items);
        }
    }

    private static void MigrateMenuItems(Models.AdminMenu menu, List<MenuItem> menuItems)
    {
        foreach (var menuItem in menuItems)
        {
            if (string.IsNullOrEmpty(menuItem.MenuName))
            {
                menuItem.MenuName = menu.Name;
            }

            MigrateMenuItems(menu, menuItem.Items);
        }
    }

    // Migrates 'ContentTypesAdminNode' entries created with pre-3.0 libraries, where only 'ContentTypeId'
    // was set, to the 3.0 format that uses 'ContentTypeName' and 'ContentTypeDisplayName'. This works
    // directly on the raw JSON so that the OrchardCore.Contents types don't need to be referenced here.
    private async Task MigrateAdminContentTypesAdminNode(JsonObject token)
    {
        if (token["MenuItems"] is JsonArray menuItems)
        {
            await MigrateContentTypesNodes(menuItems);
        }
    }

    private async Task MigrateContentTypesNodes(JsonArray nodes)
    {
        foreach (var node in nodes.OfType<JsonObject>())
        {
            // Only 'ContentTypesAdminNode' carries a 'ContentTypes' array.
            if (node["ContentTypes"] is JsonArray contentTypes)
            {
                foreach (var entry in contentTypes.OfType<JsonObject>())
                {
                    var contentTypeId = entry["ContentTypeId"]?.GetValue<string>();

                    if (!string.IsNullOrEmpty(contentTypeId) && string.IsNullOrEmpty(entry["ContentTypeName"]?.GetValue<string>()))
                    {
                        entry["ContentTypeName"] = contentTypeId;

                        if (string.IsNullOrEmpty(entry["ContentTypeDisplayName"]?.GetValue<string>()))
                        {
                            var typedef = await _contentDefinitionManager.GetTypeDefinitionAsync(contentTypeId);
                            entry["ContentTypeDisplayName"] = typedef?.DisplayName ?? contentTypeId;
                        }
                    }
                }
            }

            // Recourse into child nodes.
            if (node["Items"] is JsonArray items)
            {
                await MigrateContentTypesNodes(items);
            }
        }
    }
}

public class AdminMenuStepModel
{
    public JsonArray Data { get; set; }
}
