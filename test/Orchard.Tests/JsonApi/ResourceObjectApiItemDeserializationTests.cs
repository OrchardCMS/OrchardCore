using System;
using System.IO;
using Assent;
using JsonApiSerializer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Moq;
using Newtonsoft.Json;
using Orchard.ContentManagement;
using Orchard.JsonApi;
using Xunit;

namespace Orchard.Tests.JsonApi
{
    /// <summary>
    /// http://jsonapi.org/format/#document-resource-objects
    /// </summary>
    public class ResourceObjectApiItemDeserializationTests
    {
        private static JsonApiSerializerSettings Settings = new JsonApiSerializerSettings { Formatting = Formatting.Indented };

        [Fact]
        public void ShouldDeserializeSameObjectAsSerialized()
        {
            var item = new ApiItem(
                new ContentItem
                {
                    ContentItemId = "ABC",
                    ContentItemVersionId = "ABCV1",
                    Author = "Nick",
                    ContentType = "article",
                    Latest = false,
                    Number = 2,
                    Published = false,
                    ModifiedUtc = DateTime.MaxValue,
                    PublishedUtc = DateTime.MinValue,
                    CreatedUtc = DateTime.MaxValue,
                    Owner = "NickM"
                }, null);

            item.AddPart(new StubContentPart { Name = "FuBar" });

            var itemSerialized = JsonConvert.SerializeObject(
                item,
                Settings);

            var itemSerialized2 = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ApiItem>(
                itemSerialized, 
                Settings),
                Settings);

            Assert.Equal(itemSerialized, itemSerialized2);
        }

        private static IUrlHelper GetMockUrlHelper(string routeName, string returnValue)
        {
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(o => o.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == routeName))).Returns(returnValue);

            return urlHelper.Object;
        }
    }
}
