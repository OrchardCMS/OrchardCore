#nullable enable

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.Media.Models;

namespace OrchardCore.Media.Processing;

internal sealed class MediaCommandParser
{
    private readonly IMediaTokenService _tokenService;
    private readonly IOptions<MediaOptions> _mediaOptions;

    public MediaCommandParser(IMediaTokenService tokenService, IOptions<MediaOptions> mediaOptions)
    {
        _tokenService = tokenService;
        _mediaOptions = mediaOptions;
    }

    /// <summary>
    /// Parses the query string from the request into a <see cref="MediaCommands"/> object.
    /// Returns <see langword="null"/> when no image processing should occur.
    /// </summary>
    public MediaCommands? Parse(HttpContext context)
    {
        var query = context.Request.Query;
        if (query.Count == 0)
        {
            return null;
        }

        var commands = new MediaCommands();
        var rawCommandPairs = new List<KeyValuePair<string, string>>(query.Count);

        // Collect all known command values in query-string order (order matters for token HMAC).
        foreach (var key in query.Keys)
        {
            var value = query[key].ToString();
            switch (key)
            {
                case MediaCommands.WidthCommand:            commands.Width            = value; rawCommandPairs.Add(new(key, value)); break;
                case MediaCommands.HeightCommand:           commands.Height           = value; rawCommandPairs.Add(new(key, value)); break;
                case MediaCommands.ResizeModeCommand:       commands.ResizeMode       = value; rawCommandPairs.Add(new(key, value)); break;
                case MediaCommands.ResizeFocalPointCommand: commands.ResizeFocalPoint = value; rawCommandPairs.Add(new(key, value)); break;
                case MediaCommands.FormatCommand:           commands.Format           = value; rawCommandPairs.Add(new(key, value)); break;
                case MediaCommands.BackgroundColorCommand:  commands.BackgroundColor  = value; rawCommandPairs.Add(new(key, value)); break;
                case MediaCommands.QualityCommand:          commands.Quality          = value; rawCommandPairs.Add(new(key, value)); break;
                case MediaCommands.AutoOrientCommand:       commands.AutoOrient       = value; rawCommandPairs.Add(new(key, value)); break;
            }
        }

        var tokenValue = query[MediaCommands.TokenCommand].ToString();
        var versionValue = query[MediaCommands.VersionCommand].ToString();

        // Nothing useful was parsed.
        if (rawCommandPairs.Count == 0 && string.IsNullOrEmpty(versionValue))
        {
            return null;
        }

        // Only a version command → no processing, just cache-busting.
        if (rawCommandPairs.Count == 0 && !string.IsNullOrEmpty(versionValue))
        {
            return null;
        }

        var mediaOptions = _mediaOptions.Value;

        if (mediaOptions.UseTokenizedQueryString)
        {
            if (!string.IsNullOrEmpty(tokenValue))
            {
                // Validate HMAC of the processing commands against the token.
                if (!_tokenService.TryValidateToken(rawCommandPairs, tokenValue))
                {
                    // Invalid token — serve unprocessed image.
                    return null;
                }
                // Token is valid — all commands are allowed, including focal point and bgcolor.
            }
            else
            {
                // No token — strip commands not allowed without tokenization.
                commands.ResizeFocalPoint = null;
                commands.BackgroundColor = null;

                EnforceSupportedSizes(commands, mediaOptions);
            }
        }
        else
        {
            // Tokenization disabled — apply same restrictions as tokenless mode.
            commands.ResizeFocalPoint = null;
            commands.BackgroundColor = null;

            EnforceSupportedSizes(commands, mediaOptions);
        }

        // Default to max mode when no mode is specified.
        if (string.IsNullOrEmpty(commands.ResizeMode))
        {
            commands.ResizeMode = "max";
        }

        // Return null if no meaningful processing remains after validation.
        if (string.IsNullOrEmpty(commands.Width) &&
            string.IsNullOrEmpty(commands.Height) &&
            string.IsNullOrEmpty(commands.Format) &&
            string.IsNullOrEmpty(commands.Quality))
        {
            return null;
        }

        return commands;
    }

    private static void EnforceSupportedSizes(MediaCommands commands, MediaOptions mediaOptions)
    {
        if (!string.IsNullOrEmpty(commands.Width) &&
            int.TryParse(commands.Width, out var width) &&
            Array.BinarySearch(mediaOptions.SupportedSizes, width) < 0)
        {
            commands.Width = null;
        }

        if (!string.IsNullOrEmpty(commands.Height) &&
            int.TryParse(commands.Height, out var height) &&
            Array.BinarySearch(mediaOptions.SupportedSizes, height) < 0)
        {
            commands.Height = null;
        }
    }
}
