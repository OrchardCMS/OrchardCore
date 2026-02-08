using System.Text.Json.Nodes;
using OrchardCore.Media.Models;
using OrchardCore.Media.Services;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Media.Recipes;

/// <summary>
/// This recipe step creates or updates a media profile.
/// </summary>
[Obsolete("Implement IRecipeDeploymentStep instead. This class will be removed in a future version.", false)]
#pragma warning disable CS0618 // Type or member is obsolete
public sealed class MediaProfileStep : NamedRecipeStepHandler
#pragma warning restore CS0618
{
    private readonly MediaProfilesManager _mediaProfilesManager;

    public MediaProfileStep(MediaProfilesManager mediaProfilesManager)
        : base("MediaProfiles")
    {
        _mediaProfilesManager = mediaProfilesManager;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<MediaProfileStepModel>();

        foreach (var mediaProfile in model.MediaProfiles)
        {
            await _mediaProfilesManager.UpdateMediaProfileAsync(mediaProfile.Key, mediaProfile.Value);
        }
    }
}

public sealed class MediaProfileStepModel
{
    public Dictionary<string, MediaProfile> MediaProfiles { get; set; }
}
