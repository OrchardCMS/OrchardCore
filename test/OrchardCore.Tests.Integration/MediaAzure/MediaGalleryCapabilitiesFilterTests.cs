using Azure.Storage.Blobs;
using Azure.Storage.Files.DataLake;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using System.Reflection;
using Moq;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.FileStorage.AzureBlob;
using OrchardCore.Media.Azure.Filters;
using OrchardCore.Media.Controllers;
using OrchardCore.Modules;
using OrchardCore.Tests.Integration.AzureBlob;
using Xunit;

namespace OrchardCore.Tests.Integration.MediaAzure;

/// <summary>
/// Verifies that <see cref="MediaGalleryCapabilitiesFilter"/> warns admins from the Media Library
/// page when the backing store is (or may be) an ADLS Gen2 account, and stays silent otherwise.
/// </summary>
public sealed class MediaGalleryCapabilitiesFilterTests
{
    private static async Task<BlobFileStore> CreateStoreAsync(bool createAsGen2, bool? useHierarchicalNamespaceOverride)
    {
        var connectionString = System.Environment.GetEnvironmentVariable("AZURITE_CONNECTION_STRING");
        var containerName = $"filter-test-{Guid.NewGuid():N}";

        if (createAsGen2)
        {
            await new DataLakeServiceClient(connectionString).GetFileSystemClient(containerName).CreateIfNotExistsAsync();
        }
        else
        {
            await new BlobContainerClient(connectionString, containerName).CreateIfNotExistsAsync();
        }

        var options = new FilterTestBlobStorageOptions
        {
            ConnectionString = connectionString,
            ContainerName = containerName,
            BasePath = "",
            UseHierarchicalNamespace = useHierarchicalNamespaceOverride,
        };

        var clock = Mock.Of<IClock>(c => c.UtcNow == DateTime.UtcNow);
        var store = new BlobFileStore(options, clock, new FileExtensionContentTypeProvider());
        await store.EnsureCapabilitiesAsync();

        return store;
    }

    private static ActionExecutingContext CreateContext(Type controllerType, string actionName)
    {
        var actionDescriptor = new ControllerActionDescriptor
        {
            ControllerTypeInfo = controllerType.GetTypeInfo(),
            ActionName = actionName,
        };

        var actionContext = new Microsoft.AspNetCore.Mvc.ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            actionDescriptor,
            new ModelStateDictionary());

        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object>(),
            controller: null);
    }

    private static (Mock<INotifier> Notifier, MediaGalleryCapabilitiesFilter Filter) CreateFilter(BlobFileStore store)
    {
        var notifier = new Mock<INotifier>();
        var htmlLocalizer = new Mock<IHtmlLocalizer<MediaGalleryCapabilitiesFilter>>();
        htmlLocalizer.Setup(l => l[It.IsAny<string>()]).Returns((string name) => new LocalizedHtmlString(name, name));

        return (notifier, new MediaGalleryCapabilitiesFilter(store, notifier.Object, htmlLocalizer.Object));
    }

    private static Task<ActionExecutedContext> NoopNext() => Task.FromResult<ActionExecutedContext>(null);

    [AzuriteFact]
    public async Task WarnsWhenGen2Detected()
    {
        var store = await CreateStoreAsync(createAsGen2: true, useHierarchicalNamespaceOverride: null);
        var (notifier, filter) = CreateFilter(store);
        var context = CreateContext(typeof(AdminController), nameof(AdminController.Index));

        await filter.OnActionExecutionAsync(context, NoopNext);

        notifier.Verify(n => n.AddAsync(NotifyType.Warning, It.IsAny<LocalizedHtmlString>()), Times.Once);
    }

    [AzuriteFact]
    public async Task DoesNotWarnWhenGen1Confirmed()
    {
        var store = await CreateStoreAsync(createAsGen2: false, useHierarchicalNamespaceOverride: false);
        var (notifier, filter) = CreateFilter(store);
        var context = CreateContext(typeof(AdminController), nameof(AdminController.Index));

        await filter.OnActionExecutionAsync(context, NoopNext);

        notifier.Verify(n => n.AddAsync(It.IsAny<NotifyType>(), It.IsAny<LocalizedHtmlString>()), Times.Never);
    }

    [AzuriteFact]
    public async Task DoesNotWarnForUnrelatedAction()
    {
        var store = await CreateStoreAsync(createAsGen2: true, useHierarchicalNamespaceOverride: null);
        var (notifier, filter) = CreateFilter(store);
        var context = CreateContext(typeof(AdminController), "SomeOtherAction");

        await filter.OnActionExecutionAsync(context, NoopNext);

        notifier.Verify(n => n.AddAsync(It.IsAny<NotifyType>(), It.IsAny<LocalizedHtmlString>()), Times.Never);
    }

    [AzuriteFact]
    public async Task DoesNotWarnForUnrelatedController()
    {
        var store = await CreateStoreAsync(createAsGen2: true, useHierarchicalNamespaceOverride: null);
        var (notifier, filter) = CreateFilter(store);
        var context = CreateContext(typeof(MediaGalleryCapabilitiesFilterTests), nameof(AdminController.Index));

        await filter.OnActionExecutionAsync(context, NoopNext);

        notifier.Verify(n => n.AddAsync(It.IsAny<NotifyType>(), It.IsAny<LocalizedHtmlString>()), Times.Never);
    }
}

internal sealed class FilterTestBlobStorageOptions : BlobStorageOptions;
