using Microsoft.Extensions.Options;

namespace OrchardCore.Media.Services;

internal sealed class MediaOptionsValidator : IValidateOptions<MediaOptions>
{
    public ValidateOptionsResult Validate(string name, MediaOptions options)
    {
        if (!MediaFileStorePathHelper.IsValidRelativePath(options.AssetsPath?.TrimEnd(PathExtensions.PathSeparators)))
        {
            return ValidateOptionsResult.Fail(
                "The OrchardCore:Media:AssetsPath setting must be a relative subdirectory of the tenant's data directory, " +
                "without empty, '.' or '..' segments, drive prefixes, or segments ending in a dot or space.");
        }

        var overlappingExtensions = options.AllowedFileExtensions
            .Intersect(options.RestrictedFileExtensions, StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (overlappingExtensions.Length > 0)
        {
            return ValidateOptionsResult.Fail(
                $"The OrchardCore:Media:AllowedFileExtensions and OrchardCore:Media:RestrictedFileExtensions settings must not overlap. " +
                $"Remove these extensions from one of the lists: {string.Join(", ", overlappingExtensions)}.");
        }

        return ValidateOptionsResult.Success;
    }
}
