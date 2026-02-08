using OrchardCore.Recipes.Schema;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Twitter.Services;
using OrchardCore.Twitter.Settings;

namespace OrchardCore.Twitter.Recipes;

public sealed class TwitterSettingsRecipeStep : RecipeImportStep<TwitterSettingsRecipeStep.TwitterSettingsStepModel>
{
    private readonly ITwitterSettingsService _twitterService;

    public TwitterSettingsRecipeStep(ITwitterSettingsService twitterService)
    {
        _twitterService = twitterService;
    }

    public override string Name => nameof(TwitterSettings);

    protected override JsonSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title("Twitter Settings")
            .Description("Imports Twitter / X authentication settings.")
            .Required("name")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("ConsumerKey", new RecipeStepSchemaBuilder().TypeString()),
                ("ConsumerSecret", new RecipeStepSchemaBuilder().TypeString()),
                ("AccessToken", new RecipeStepSchemaBuilder().TypeString()),
                ("AccessTokenSecret", new RecipeStepSchemaBuilder().TypeString()))
            .AdditionalProperties(true)
            .Build();
    }

    protected override async Task ImportAsync(TwitterSettingsStepModel model, RecipeExecutionContext context)
    {
        var settings = await _twitterService.LoadSettingsAsync();

        settings.ConsumerKey = model.ConsumerKey;
        settings.ConsumerSecret = model.ConsumerSecret;
        settings.AccessToken = model.AccessToken;
        settings.AccessTokenSecret = model.AccessTokenSecret;

        await _twitterService.UpdateSettingsAsync(settings);
    }

    public sealed class TwitterSettingsStepModel
    {
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string AccessToken { get; set; }
        public string AccessTokenSecret { get; set; }
    }
}
