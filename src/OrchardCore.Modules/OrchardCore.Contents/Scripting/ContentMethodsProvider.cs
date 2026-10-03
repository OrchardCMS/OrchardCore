using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using System.Text.Json.Settings;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Scripting;

namespace OrchardCore.Contents.Scripting;

public sealed class ContentMethodsProvider : IGlobalMethodProvider
{
    private readonly GlobalMethod _newContentItemMethod;
    private readonly GlobalMethod _createContentItemMethod;
    private readonly GlobalMethod _updateContentItemMethod;
    private readonly GlobalMethod _deleteContentItemMethod;

    public ContentMethodsProvider()
    {
        _newContentItemMethod = new GlobalMethod
        {
            Name = "newContentItem",
            Method = serviceProvider => (Func<string, IContent>)((contentType) =>
                NewContentItemAsync(serviceProvider, contentType).GetAwaiter().GetResult()),
            AsyncMethod = serviceProvider => (Func<string, Task<IContent>>)(contentType =>
                NewContentItemAsync(serviceProvider, contentType)),
        };

        _createContentItemMethod = new GlobalMethod
        {
            Name = "createContentItem",
            Method = serviceProvider => (Func<string, bool?, object, IContent>)((contentType, publish, properties) =>
                CreateContentItemAsync(serviceProvider, contentType, publish, properties).GetAwaiter().GetResult()),
            AsyncMethod = serviceProvider => (Func<string, bool?, object, Task<IContent>>)((contentType, publish, properties) =>
                CreateContentItemAsync(serviceProvider, contentType, publish, properties)),
        };

        _updateContentItemMethod = new GlobalMethod
        {
            Name = "updateContentItem",
            Method = serviceProvider => (Action<ContentItem, object>)((contentItem, properties) =>
                UpdateContentItemAsync(serviceProvider, contentItem, properties).GetAwaiter().GetResult()),
            AsyncMethod = serviceProvider => (Func<ContentItem, object, Task>)((contentItem, properties) =>
                UpdateContentItemAsync(serviceProvider, contentItem, properties)),
        };

        _deleteContentItemMethod = new GlobalMethod
        {
            Name = "deleteContentItem",
            Method = serviceProvider => (Action<ContentItem, object>)((contentItem, properties) =>
                DeleteContentItemAsync(serviceProvider, contentItem).GetAwaiter().GetResult()),
            AsyncMethod = serviceProvider => (Func<ContentItem, object, Task>)((contentItem, properties) =>
                DeleteContentItemAsync(serviceProvider, contentItem)),
        };
    }

    public IEnumerable<GlobalMethod> GetMethods()
    {
        return new[] { _newContentItemMethod, _createContentItemMethod, _updateContentItemMethod, _deleteContentItemMethod };
    }

    private static async Task<IContent> NewContentItemAsync(IServiceProvider serviceProvider, string contentType)
    {
        var contentManager = serviceProvider.GetRequiredService<IContentManager>();

        return await contentManager.NewAsync(contentType);
    }

    private static async Task<IContent> CreateContentItemAsync(IServiceProvider serviceProvider, string contentType, bool? publish, object properties)
    {
        var contentManager = serviceProvider.GetRequiredService<IContentManager>();
        var contentItem = await contentManager.NewAsync(contentType);
        contentItem.Merge(properties);

        var result = await contentManager.ValidateAsync(contentItem);

        if (result.Succeeded)
        {
            await contentManager.CreateAsync(contentItem, publish == true ? VersionOptions.Published : VersionOptions.Draft);

            return contentItem;
        }

        var session = serviceProvider.GetRequiredService<YesSql.ISession>();
        await session.CancelAsync();
        throw new ValidationException(string.Join(", ", result.Errors));
    }

    private static async Task UpdateContentItemAsync(IServiceProvider serviceProvider, ContentItem contentItem, object properties)
    {
        var contentManager = serviceProvider.GetRequiredService<IContentManager>();
        contentItem.Merge(properties, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace });
        await contentManager.UpdateAsync(contentItem);
        var result = await contentManager.ValidateAsync(contentItem);
        if (!result.Succeeded)
        {
            var session = serviceProvider.GetRequiredService<YesSql.ISession>();
            await session.CancelAsync();
            throw new ValidationException(string.Join(", ", result.Errors));
        }
    }

    private static async Task DeleteContentItemAsync(IServiceProvider serviceProvider, ContentItem contentItem)
    {
        var contentManager = serviceProvider.GetRequiredService<IContentManager>();
        await contentManager.RemoveAsync(contentItem);
    }
}
