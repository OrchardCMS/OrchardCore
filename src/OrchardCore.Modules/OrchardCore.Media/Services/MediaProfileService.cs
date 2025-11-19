using System.Diagnostics.CodeAnalysis;
using OrchardCore.Media.Fields;
using OrchardCore.Media.Models;
using Format = OrchardCore.Media.Processing.Format;
using ResizeMode = OrchardCore.Media.Processing.ResizeMode;

namespace OrchardCore.Media.Services;

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
            var mediaCommands = new MediaCommands();

            if (mediaProfile.Width > 0)
            {
                mediaCommands.Width = mediaProfile.Width.ToString();
            }

            if (mediaProfile.Height > 0)
            {
                mediaCommands.Height = mediaProfile.Height.ToString();
            }

            if (mediaProfile.Mode != ResizeMode.Undefined)
            {
                mediaCommands.ResizeMode = mediaProfile.Mode.ToString().ToLower();
            }

            if (mediaProfile.Format != Format.Undefined)
            {
                mediaCommands.Format = mediaProfile.Format.ToString().ToLower();
            }

            if (mediaProfile.Quality > 0 && mediaProfile.Quality < 100)
            {
                mediaCommands.Quality = mediaProfile.Quality.ToString();
            }

            if (!string.IsNullOrEmpty(mediaProfile.BackgroundColor))
            {
                mediaCommands.BackgroundColor = mediaProfile.BackgroundColor;
            }

            return new Dictionary<string, string>(mediaCommands.GetValues());
        }
        else
        {
            return _nullProfile;

        }
    }
}

