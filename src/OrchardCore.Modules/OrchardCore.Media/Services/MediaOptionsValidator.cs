using Microsoft.Extensions.Options;

namespace OrchardCore.Media.Services;

internal sealed class MediaOptionsValidator : IValidateOptions<MediaOptions>
{
    public ValidateOptionsResult Validate(string name, MediaOptions options)
    {
        var overlappingExtensions = options.AllowedFileExtensions
            .Intersect(options.AllowedFileExtensionsWithPermission, StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (overlappingExtensions.Length > 0)
        {
            return ValidateOptionsResult.Fail(
                $"The OrchardCore_Media settings AllowedFileExtensions and AllowedFileExtensionsWithPermission must not overlap. " +
                $"Remove these extensions from one of the lists: {string.Join(", ", overlappingExtensions)}.");
        }

        return ValidateOptionsResult.Success;
    }
}
