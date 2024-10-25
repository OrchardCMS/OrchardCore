using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using OrchardCore.AdminMenu.Services;
using OrchardCore.Json;
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

    public AdminMenuStep(
        IAdminMenuService adminMenuService,
        IOptions<DocumentJsonSerializerOptions> serializationOptions)
        : base("AdminMenu")
    {
        _adminMenuService = adminMenuService;

        // The recipe step contains polymorphic types (menu items) which need to be resolved.
        _serializationOptions = serializationOptions.Value.SerializerOptions;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<AdminMenuStepModel>(_serializationOptions);

        foreach (var token in model.Data.Cast<JsonObject>())
        {
            var adminMenu = token.ToObject<Models.AdminMenu>(_serializationOptions);

            // When the id is not supplied generate an id, otherwise replace the menu if it exists, or create a new menu.
            if (string.IsNullOrEmpty(adminMenu.Id))
            {
                adminMenu.Id = IdGenerator.GenerateId();
            }

            await _adminMenuService.SaveAsync(adminMenu);
        }
    }
}

public class AdminMenuStepModel
{
    public JsonArray Data { get; set; }
}
