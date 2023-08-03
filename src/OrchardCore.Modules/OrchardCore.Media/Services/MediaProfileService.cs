using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Media.Processing;

namespace OrchardCore.Media.Services
{
    public class MediaProfileService : IMediaProfileService
    {
        private static readonly IDictionary<string, string> _nullProfile = new Dictionary<string, string>();
        private readonly MediaProfilesManager _mediaProfilesManager;

        public MediaProfileService(MediaProfilesManager mediaProfilesManager)
        {
            _mediaProfilesManager = mediaProfilesManager;
        }

        public async Task<IDictionary<string, string>> GetMediaProfileCommands(string name)
        {
            var mediaProfilesDocument = await _mediaProfilesManager.GetMediaProfilesDocumentAsync();

            if (mediaProfilesDocument.MediaProfiles.TryGetValue(name, out var mediaProfile))
            {
                var commands = new Dictionary<string, string>();
                if (mediaProfile.Width > 0)
                {
                    commands["width"] = mediaProfile.Width.ToString();
                }

                if (mediaProfile.Height > 0)
                {
                    commands["height"] = mediaProfile.Height.ToString();
                }

                if (mediaProfile.Mode != ResizeMode.Undefined)
                {
                    commands["rmode"] = mediaProfile.Mode.ToString().ToLower();
                }

                if (mediaProfile.Format != Format.Undefined)
                {
                    commands["format"] = mediaProfile.Format.ToString().ToLower();
                }

                if (mediaProfile.Quality > 0 && mediaProfile.Quality < 100)
                {
                    commands["quality"] = mediaProfile.Quality.ToString();
                }

                if (!String.IsNullOrEmpty(mediaProfile.BackgroundColor))
                {
                    commands["bgcolor"] = mediaProfile.BackgroundColor;
                }

                return commands;
            }
            else
            {
                return _nullProfile;

            }
        }
    }
}
