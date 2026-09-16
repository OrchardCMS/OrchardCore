using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Media.Hubs;

namespace OrchardCore.Media.Services;

internal static class MediaSignalRExtensions
{
    public static bool IsMediaSignalREnabled(this IServiceProvider serviceProvider)
        => serviceProvider.GetService<IHubContext<MediaHub>>() is not null;
}
