using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Scripting;

namespace OrchardCore.Contents.Scripting
{
    public class ContentMethodsProvider : IGlobalMethodProvider
    {
        private readonly GlobalMethod _newContentItemMethod;
        private readonly GlobalMethod _createContentItemMethod;
        private readonly GlobalMethod _updateContentItemMethod;

        public ContentMethodsProvider()
        {
            _newContentItemMethod = new GlobalMethod
            {
                Name = "newContentItem",
                Method = serviceProvider => (Func<string, IContent>)((contentType) =>
                {
                    var contentManager = serviceProvider.GetRequiredService<IContentManager>();
                    var contentItem = contentManager.NewAsync(contentType).GetAwaiter().GetResult();

                    return contentItem;
                })
            };

            _createContentItemMethod = new GlobalMethod
            {
                Name = "createContentItem",
                Method = serviceProvider => (Func<string, bool?, object, IContent>)((contentType, publish, properties) =>
                {
                    var contentManager = serviceProvider.GetRequiredService<IContentManager>();
                    var contentItem = contentManager.NewAsync(contentType).GetAwaiter().GetResult();

                    var json = JsonConvert.SerializeObject(properties);
                    contentItem.Apply(JsonConvert.DeserializeObject<ContentItem>(json));
                    if (!string.IsNullOrWhiteSpace(contentItem.Content?.TitlePart?.Title?.ToString()))
                        contentItem.DisplayText = contentItem.Content.TitlePart.Title?.ToString();

                    contentManager.CreateAsync(contentItem.ContentItem, publish == true ? VersionOptions.Published : VersionOptions.Draft).GetAwaiter().GetResult();

                    return contentItem;
                })
            };

            _updateContentItemMethod = new GlobalMethod
            {
                Name = "updateContentItem",
                Method = serviceProvider => (Action<ContentItem, object>)((contentItem, properties) =>
                {
                    var contentManager = serviceProvider.GetRequiredService<IContentManager>();

                    var json = JsonConvert.SerializeObject(properties);
                    contentItem.Apply(JsonConvert.DeserializeObject<ContentItem>(json));
                    if (!string.IsNullOrWhiteSpace(contentItem.Content?.TitlePart?.Title?.ToString()))
                        contentItem.DisplayText = contentItem.Content.TitlePart.Title?.ToString();

                    contentManager.UpdateAsync(contentItem);
                })
            };
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            return new[] { _newContentItemMethod, _createContentItemMethod, _updateContentItemMethod };
        }
    }
}
