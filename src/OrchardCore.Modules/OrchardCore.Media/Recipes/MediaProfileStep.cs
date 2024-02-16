using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Media.Models;
using OrchardCore.Media.Services;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Media.Recipes
{
    /// <summary>
    /// This recipe step creates or updates a media profile.
    /// </summary>
    public class MediaProfileStep : IRecipeStepHandler
    {
        private readonly MediaProfilesManager _mediaProfilesManager;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public MediaProfileStep(
            MediaProfilesManager mediaProfilesManager,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _mediaProfilesManager = mediaProfilesManager;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, "MediaProfiles", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<MediaProfileStepModel>(_jsonSerializerOptions);

            foreach (var mediaProfile in model.MediaProfiles)
            {
                await _mediaProfilesManager.UpdateMediaProfileAsync(mediaProfile.Key, mediaProfile.Value);
            }
        }
    }

    public class MediaProfileStepModel
    {
        public Dictionary<string, MediaProfile> MediaProfiles { get; set; }
    }
}
