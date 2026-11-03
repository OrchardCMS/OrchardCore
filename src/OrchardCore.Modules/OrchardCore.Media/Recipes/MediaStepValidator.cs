using System.Text.Json.Nodes;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Media.Services;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Media.Recipes;

/// <summary>
/// Checks the files that a <see cref="MediaStep"/> would import, applying its rules to the target paths and resolving
/// the source paths like it does. It doesn't decode content or fetch URLs.
/// </summary>
public sealed class MediaStepValidator : IRecipeStepValidator
{
    private readonly MediaOptions _mediaOptions;

    internal readonly IStringLocalizer S;

    public MediaStepValidator(IOptions<MediaOptions> options, IStringLocalizer<MediaStepValidator> stringLocalizer)
    {
        _mediaOptions = options.Value;
        S = stringLocalizer;
    }

    /// <inheritdoc />
    public string StepName => "media";

    /// <inheritdoc />
    public Task ValidateAsync(RecipeValidationContext context)
    {
        var step = context.Step.ToObject<MediaStep.MediaStepModel>();

        if (step.Files is null)
        {
            context.AddError("media-no-files", S["A media step requires a 'Files' array."]);

            return Task.CompletedTask;
        }

        foreach (var file in step.Files)
        {
            ValidateTarget(file, context);
            ValidateSource(file, context);
        }

        return Task.CompletedTask;
    }

    private void ValidateTarget(MediaStep.MediaStepFile file, RecipeValidationContext context)
    {
        if (RecipeValidationContext.IsScript(file.TargetPath))
        {
            return;
        }

        if (!MediaFileStorePathHelper.IsValidRelativePath(file.TargetPath))
        {
            context.AddError("media-target-invalid", S["Media target path must be a relative file path without traversal segments: '{0}'", file.TargetPath]);
        }
        else if (!_mediaOptions.IsFileExtensionAllowed(Path.GetExtension(file.TargetPath), hasAdditionalPermission: true))
        {
            context.AddError("media-extension-not-allowed", S["File extension not allowed: '{0}'", file.TargetPath]);
        }
    }

    private void ValidateSource(MediaStep.MediaStepFile file, RecipeValidationContext context)
    {
        if (!string.IsNullOrWhiteSpace(file.Base64))
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(file.SourcePath))
        {
            // A file outside of the provider's root is reported as missing, like any other.
            if (!RecipeValidationContext.IsScript(file.SourcePath)
                && !context.Recipe.FileProvider.GetRelativeFileInfo(context.Recipe.BasePath, file.SourcePath).Exists)
            {
                context.AddError("file-not-found", S["The file '{0}' doesn't exist in the recipe's files.", file.SourcePath]);
            }
        }
        else if (!string.IsNullOrWhiteSpace(file.SourceUrl))
        {
            context.AddInfo("media-url-not-fetched", S["The URL of '{0}' isn't fetched. The step silently skips the file if the request fails.", file.TargetPath]);
        }
        else
        {
            context.AddWarning("media-no-source", S["The file '{0}' has no 'Base64', 'SourcePath' or 'SourceUrl'. The step would skip it.", file.TargetPath]);
        }
    }
}
