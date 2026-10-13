using System.Text.Json.Nodes;
using OrchardCore.Environment.Options;
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
    private readonly IOptionsUpdateNotifier _optionsUpdateNotifier;

    public MicrosoftAccountSettingsStep(IMicrosoftAccountService microsoftAccountService, IOptionsUpdateNotifier optionsUpdateNotifier)
        : base(nameof(MicrosoftAccountSettings))
    {
        _microsoftAccountService = microsoftAccountService;
        _optionsUpdateNotifier = optionsUpdateNotifier;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<MicrosoftAccountSettingsStepModel>();
        var settings = await _microsoftAccountService.LoadSettingsAsync();

        settings.AppId = model.AppId;
        settings.AppSecret = model.AppSecret;
        settings.CallbackPath = model.CallbackPath;

        await _microsoftAccountService.UpdateSettingsAsync(settings);

        _optionsUpdateNotifier.RequestMicrosoftAccountUpdate();
    }
}

public sealed class MicrosoftAccountSettingsStepModel
{
    public string AppId { get; set; }
    public string AppSecret { get; set; }
    public string CallbackPath { get; set; }
}
