using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public MediaProfileStep(MediaProfilesManager mediaProfilesManager)
        {
            _mediaProfilesManager = mediaProfilesManager;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "MediaProfiles", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<MediaProfileStepModel>();

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
