using Microsoft.Extensions.Options;
using OrchardCore.Media.Core.Processing;
using OrchardCore.Media.Models;
using OrchardCore.Media.ViewModels;

namespace OrchardCore.Media.Services;

internal sealed class MediaProfileManagementService
{
    private readonly MediaProfilesManager _manager;
    private readonly MediaOptions _options;

    public MediaProfileManagementService(MediaProfilesManager manager, IOptions<MediaOptions> options)
    {
        _manager = manager;
        _options = options.Value;
    }

    public async Task<MediaProfileMutationResult> SaveAsync(string name, MediaProfile profile, string sourceName = null)
    {
        var errors = Validate(name, profile, _options);
        if (errors.Count > 0) { return new() { Errors = errors }; }
        name = name.Trim().ToLowerInvariant();
        var document = await _manager.LoadMediaProfilesDocumentAsync();
        if (sourceName is not null && !document.MediaProfiles.ContainsKey(sourceName)) { return new() { NotFound = true }; }
        if (document.MediaProfiles.TryGetValue(name, out var existing))
        {
            if (sourceName is null && Equal(existing, profile)) { return new() { Name = name, Profile = existing }; }
            if (!string.Equals(sourceName, name, StringComparison.OrdinalIgnoreCase)) { return new() { Conflict = true }; }
            if (Equal(existing, profile)) { return new() { Name = name, Profile = existing }; }
        }
        await _manager.SaveMediaProfileAsync(sourceName, name, profile);
        return new() { Name = name, Profile = profile, Changed = true };
    }

    public static Dictionary<string, string[]> Validate(string name, MediaProfile profile, MediaOptions options)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 128 || name.Any(char.IsControl))
        {
            errors["name"] = ["A profile name of at most 128 characters without control characters is required."];
        }
        if (profile is null)
        {
            errors["profile"] = ["A profile definition is required."];
            return errors;
        }
        if (profile.Width < 0 || (!options.UseTokenizedQueryString && profile.Width != 0 && !options.SupportedSizes.Contains(profile.Width)))
        {
            errors["width"] = ["Width must be nonnegative and supported when tokenized image URLs are disabled."];
        }
        if (profile.Height < 0 || (!options.UseTokenizedQueryString && profile.Height != 0 && !options.SupportedSizes.Contains(profile.Height)))
        {
            errors["height"] = ["Height must be nonnegative and supported when tokenized image URLs are disabled."];
        }
        if (!Enum.IsDefined(profile.Mode)) { errors["mode"] = ["Select a supported resize mode."]; }
        if (profile.Format is not (Format.Undefined or Format.Gif or Format.Jpg or Format.Png or Format.WebP))
        {
            errors["format"] = ["Select Undefined, Gif, Jpg, Png, or WebP."];
        }
        if (profile.Quality < 0 || profile.Quality > 100) { errors["quality"] = ["Quality must be between 0 and 100; zero uses the default."]; }
        if (!string.IsNullOrEmpty(profile.BackgroundColor))
        {
            var hex = profile.BackgroundColor.StartsWith('#') ? profile.BackgroundColor[1..] : profile.BackgroundColor;
            if (hex.Length is not (3 or 6) || hex.Any(character => !Uri.IsHexDigit(character)))
            {
                errors["backgroundColor"] = ["Background color must contain three or six hexadecimal digits, optionally prefixed by #."];
            }
        }
        return errors;
    }

    public static MediaProfile FromEditor(MediaProfileViewModel model, MediaOptions options) => new()
    {
        Hint = model.Hint,
        Width = model.SelectedWidth == 0 || options.SupportedSizes.Contains(model.SelectedWidth) ? model.SelectedWidth : model.CustomWidth,
        Height = model.SelectedHeight == 0 || options.SupportedSizes.Contains(model.SelectedHeight) ? model.SelectedHeight : model.CustomHeight,
        Mode = model.SelectedMode, Format = model.SelectedFormat, Quality = model.Quality,
        BackgroundColor = model.BackgroundColor, AutoOrient = model.AutoOrient,
    };

    private static bool Equal(MediaProfile left, MediaProfile right) => left.Hint == right.Hint && left.Width == right.Width
        && left.Height == right.Height && left.Mode == right.Mode && left.Format == right.Format && left.Quality == right.Quality
        && left.BackgroundColor == right.BackgroundColor && left.AutoOrient == right.AutoOrient;
}

internal sealed class MediaProfileMutationResult
{
    public string Name { get; init; }
    public MediaProfile Profile { get; init; }
    public bool Changed { get; init; }
    public bool NotFound { get; init; }
    public bool Conflict { get; init; }
    public Dictionary<string, string[]> Errors { get; init; } = [];
}
