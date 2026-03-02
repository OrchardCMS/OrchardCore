using System.Text.Json.Nodes;
using OrchardCore.Facebook.Services;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Facebook.Recipes;

/// <summary>
/// This recipe step sets general Facebook Login settings.
/// </summary>
[Obsolete("Implement IRecipeDeploymentStep instead. This class will be removed in a future version.", false)]
#pragma warning disable CS0618 // Type or member is obsolete
public sealed class FacebookSettingsStep : NamedRecipeStepHandler
#pragma warning restore CS0618
{
    private readonly IFacebookService _facebookService;

    public FacebookSettingsStep(IFacebookService facebookService)
        : base("FacebookCoreSettings")
    {
        _facebookService = facebookService;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        if (!string.Equals(context.Name, "FacebookCoreSettings", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var model = context.Step.ToObject<FacebookCoreSettingsStepModel>();
        var settings = await _facebookService.GetSettingsAsync();

        settings.AppId = model.AppId;
        settings.AppSecret = model.AppSecret;
        settings.SdkJs = model.SdkJs ?? "sdk.js";
        settings.FBInit = model.FBInit;
        settings.FBInitParams = model.FBInitParams;
        settings.Version = model.Version ?? "3.2";

        await _facebookService.UpdateSettingsAsync(settings);
    }
}
