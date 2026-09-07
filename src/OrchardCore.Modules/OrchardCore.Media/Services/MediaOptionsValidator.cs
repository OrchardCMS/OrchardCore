using Microsoft.Extensions.Options;

namespace OrchardCore.Media.Services;

internal sealed class MediaOptionsValidator : IValidateOptions<MediaOptions>
{
    public ValidateOptionsResult Validate(string name, MediaOptions options)
    {
        if (!MediaFileStorePathHelper.IsValidRelativePath(options.AssetsPath?.TrimEnd(PathExtensions.PathSeparators)))
        {
            return ValidateOptionsResult.Fail(
                "The OrchardCore_Media setting AssetsPath must be a relative subdirectory of the tenant's data directory, " +
                "without empty, '.' or '..' segments, drive prefixes, or segments ending in a dot or space.");
        }

        return ValidateOptionsResult.Success;
    }
}
