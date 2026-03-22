using OrchardCore.Media.Core.Processing;
using OrchardCore.Media.Models;

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
            var commands = new Dictionary<string, string>();

            if (mediaProfile.Width > 0)
            {
                commands[MediaCommands.WidthCommand] = mediaProfile.Width.ToString();
            }

            if (mediaProfile.Height > 0)
            {
                commands[MediaCommands.HeightCommand] = mediaProfile.Height.ToString();
            }

            if (mediaProfile.Mode != ResizeMode.Undefined)
            {
                commands[MediaCommands.ResizeModeCommand] = mediaProfile.Mode.ToString().ToLower();
            }

            if (mediaProfile.Format != Format.Undefined)
            {
                commands[MediaCommands.FormatCommand] = mediaProfile.Format.ToString().ToLower();
            }

            if (mediaProfile.Quality > 0 && mediaProfile.Quality < 100)
            {
                commands[MediaCommands.QualityCommand] = mediaProfile.Quality.ToString();
            }

            if (!string.IsNullOrEmpty(mediaProfile.BackgroundColor))
            {
                commands[MediaCommands.BackgroundColorCommand] = mediaProfile.BackgroundColor;
            }

            return commands;
        }
        else
        {
            return _nullProfile;
        }
    }
}

