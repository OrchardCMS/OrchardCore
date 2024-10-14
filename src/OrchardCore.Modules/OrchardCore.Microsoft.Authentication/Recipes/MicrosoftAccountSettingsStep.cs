using System.Text.Json.Nodes;
using OrchardCore.Microsoft.Authentication.Services;
using OrchardCore.Microsoft.Authentication.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Microsoft.Authentication.Recipes;

/// <summary>
/// This recipe step sets Microsoft Account settings.
/// </summary>
public sealed class MicrosoftAccountSettingsStep : NamedRecipeStepHandler
{
    private readonly IMicrosoftAccountService _microsoftAccountService;

    public MicrosoftAccountSettingsStep(IMicrosoftAccountService microsoftAccountService)
        : base(nameof(MicrosoftAccountSettings))
    {
        _microsoftAccountService = microsoftAccountService;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<MicrosoftAccountSettingsStepModel>();
        var settings = await _microsoftAccountService.LoadSettingsAsync();

        settings.AppId = model.AppId;
        settings.AppSecret = model.AppSecret;
        settings.CallbackPath = model.CallbackPath;

        await _microsoftAccountService.UpdateSettingsAsync(settings);
    }
}

public sealed class MicrosoftAccountSettingsStepModel
{
    public string AppId { get; set; }
    public string AppSecret { get; set; }
    public string CallbackPath { get; set; }
}
