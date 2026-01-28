using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;
using Microsoft.Extensions.Options;
using OrchardCore.AdminMenu.Services;
using OrchardCore.Contents.Core;
using OrchardCore.Json;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.AdminMenu.Recipes;

public sealed class AdminMenuRecipeStep : RecipeImportStep<AdminMenuStepModel>
{
    private readonly IAdminMenuService _adminMenuService;
    private readonly JsonSerializerOptions _serializationOptions;

    public AdminMenuRecipeStep(
        IAdminMenuService adminMenuService,
        IOptions<DocumentJsonSerializerOptions> serializationOptions)
    {
        _adminMenuService = adminMenuService;

        // The recipe step contains polymorphic types (menu items) which need to be resolved.
        _serializationOptions = serializationOptions.Value.SerializerOptions;
    }

    public override string Name => "AdminMenu";

    protected override JsonSchema BuildSchema()
    {
        return new JsonSchemaBuilder()
            .Schema(MetaSchemas.Draft202012Id)
            .Type(SchemaValueType.Object)
            .Title("Admin Menu")
            .Description("Imports admin menu definitions.")
            .Required("name", "data")
            .Properties(
                ("name", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("data", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Array)
                    .Description("Array of admin menu definitions.")
                    .Items(new JsonSchemaBuilder()
                        .Type(SchemaValueType.Object)
                        .Required("Id", "Name", "MenuItems")
                        .Properties(
                            ("Id", new JsonSchemaBuilder()
                                .Type(SchemaValueType.String)
                                .Description("The unique identifier for the admin menu item.")),
                            ("Name", new JsonSchemaBuilder()
                                .Type(SchemaValueType.String)
                                .Description("The display name of the admin menu.")),
                            ("MenuItems", new JsonSchemaBuilder()
                                .Type(SchemaValueType.Array)
                                .Description("The list of content items under this menu.")
                                .Items(ContentCommonSchemas.ContentItemSchema))
                        )
                        .AdditionalProperties(JsonSchema.True)
                        .Build()))
            )
            .AdditionalProperties(JsonSchema.False)
            .Build();
    }

    protected override async Task ImportAsync(AdminMenuStepModel model, RecipeExecutionContext context)
    {
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
