using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Media.Services;

public sealed class MediaSlugifyOptionsConfiguration : IConfigureOptions<MediaSlugifyOptions>
{
    private readonly IShellConfiguration _shellConfiguration;

    public MediaSlugifyOptionsConfiguration(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public void Configure(MediaSlugifyOptions options)
    {
        // The 'OrchardCore_Media_Slugify' section is deprecated and will be removed in a future major version, use 'Media:Slugify' instead.
        var section = _shellConfiguration.GetSectionCompat("Media:Slugify", "OrchardCore_Media_Slugify");

        options.Transliterate = section.GetValue(nameof(options.Transliterate), true);
    }
}
