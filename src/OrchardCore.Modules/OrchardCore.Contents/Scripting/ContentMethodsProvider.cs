using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
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
                    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
                    var contentManager = httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IContentManager>();
                    var contentItem = contentManager.NewAsync(contentType).GetAwaiter().GetResult();

                    return contentItem;
                })
            };

            _createContentItemMethod = new GlobalMethod
            {
                Name = "createContentItem",
                Method = serviceProvider => (Func<string, bool?, object, IContent>)((contentType, publish, properties) =>
                {
                    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
                    var contentManager = httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IContentManager>();
                    var contentItem = contentManager.NewAsync(contentType).GetAwaiter().GetResult();
                    var props = JObject.FromObject(properties);
                    var content = (JObject)contentItem.ContentItem.Content;

                    content.Merge(props);
                    contentManager.CreateAsync(contentItem.ContentItem, publish == true ? VersionOptions.Published : VersionOptions.Draft).GetAwaiter().GetResult();

                    return contentItem;
                })
            };

            _updateContentItemMethod = new GlobalMethod
            {
                Name = "updateContentItem",
                Method = serviceProvider => (Action<IContent, object>)((contentItem, properties) =>
                {
                    var props = JObject.FromObject(properties);
                    var content = (JObject)contentItem.ContentItem.Content;

                    content.Merge(props);
                })
            };
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            return new[] { _newContentItemMethod, _createContentItemMethod, _updateContentItemMethod };
        }
    }
}
