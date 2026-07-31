using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.FileStorage.AzureBlob;

namespace OrchardCore.Media.Azure.Filters;

/// <summary>
/// Warns admins, from the Media Library page itself, when the Azure Blob storage account backing it
/// is (or may be) an ADLS Gen2 / Hierarchical Namespace account, since the current Media Library has
/// not been fully verified against Gen2 storage yet.
/// </summary>
public sealed class MediaGalleryCapabilitiesFilter : IAsyncActionFilter
{
    private readonly BlobFileStore _blobFileStore;
    private readonly INotifier _notifier;

    internal readonly IHtmlLocalizer H;

    public MediaGalleryCapabilitiesFilter(
        BlobFileStore blobFileStore,
        INotifier notifier,
        IHtmlLocalizer<MediaGalleryCapabilitiesFilter> htmlLocalizer)
    {
        _blobFileStore = blobFileStore;
        _notifier = notifier;
        H = htmlLocalizer;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionDescriptor is ControllerActionDescriptor descriptor &&
            descriptor.ControllerTypeInfo == typeof(global::OrchardCore.Media.Controllers.AdminController) &&
            descriptor.ActionName == nameof(global::OrchardCore.Media.Controllers.AdminController.Index))
        {
            if (_blobFileStore.IsHierarchicalNamespaceEnabled == true)
            {
                await _notifier.WarningAsync(H["This site's Azure Storage account has Hierarchical Namespace (ADLS Gen2) enabled. The current Media Library has not been fully verified against Gen2 storage yet; some behavior may be affected. Full support is coming with the next Media Library update — until then, prefer a Gen1 (flat-namespace) storage account for this site."]);
            }
            else if (_blobFileStore.IsHierarchicalNamespaceEnabled is null)
            {
                await _notifier.WarningAsync(H["Unable to determine whether this site's Azure Storage account has Hierarchical Namespace (ADLS Gen2) enabled. Falling back to standard flat-namespace operations."]);
            }
        }

        await next();
    }
}
