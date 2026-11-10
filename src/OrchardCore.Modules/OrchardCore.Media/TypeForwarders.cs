using System.Runtime.CompilerServices;
using OrchardCore.Media.Processing;
using OrchardCore.Media.Services;

// These types moved to the OrchardCore.Media.ImageSharp assembly, the only one referencing the
// Six Labors packages, with the same namespaces.
[assembly: TypeForwardedTo(typeof(BackwardsCompatibleCacheKey))]
[assembly: TypeForwardedTo(typeof(ImageVersionProcessor))]
[assembly: TypeForwardedTo(typeof(MediaImageSharpConfiguration))]
[assembly: TypeForwardedTo(typeof(MediaResizingFileProvider))]
[assembly: TypeForwardedTo(typeof(MediaTokenOptions))]
[assembly: TypeForwardedTo(typeof(MediaTokenService))]
[assembly: TypeForwardedTo(typeof(ResizedMediaCacheBackgroundTask))]
[assembly: TypeForwardedTo(typeof(TokenCommandProcessor))]
