using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Media.Services;

/// <summary>
/// Describes the breadcrumb trails of the media profiles and media cache screens.
/// </summary>
public sealed class MediaProfilesBreadcrumbProvider : IBreadcrumbProvider
{
    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "OrchardCore.Media" },
    };

    internal readonly IStringLocalizer S;

    public MediaProfilesBreadcrumbProvider(IStringLocalizer<MediaProfilesBreadcrumbProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
    {
        switch (builder.Name)
        {
            case MediaProfilesConstants.List:
                AddList(builder);
                break;

            case MediaProfilesConstants.Create:
                AddList(builder);
                builder.Add(S["Create Media Profile"], item => item.Id("MediaProfile"));
                break;

            case MediaProfilesConstants.Edit:
                AddList(builder);
                builder.Add(S["Edit Media Profile"], item => item.Id("MediaProfile"));
                break;

            case MediaProfilesConstants.Cache:
                builder.Add(S["Asset Cache"], item => item
                    .Id("MediaCache")
                    .Action("Index", "MediaCache", s_routeValues)
                    .Permission(MediaPermissions.ManageAssetCache));
                break;
        }

        return ValueTask.CompletedTask;
    }

    private void AddList(BreadcrumbBuilder builder)
        => builder.Add(S["Media Profiles"], item => item
            .Id("MediaProfiles")
            .Action("Index", "MediaProfiles", s_routeValues)
            .Permission(MediaPermissions.ManageMediaProfiles));
}
