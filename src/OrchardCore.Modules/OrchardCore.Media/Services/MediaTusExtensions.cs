namespace OrchardCore.Media.Services;

internal static class MediaTusExtensions
{
    public static bool IsMediaTusEnabled(this IServiceProvider serviceProvider)
        => serviceProvider.GetService(typeof(MediaTusMarker)) is not null;
}
