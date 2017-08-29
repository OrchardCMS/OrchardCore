using System;
using Assent;
using JsonApiSerializer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
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
    public class ResourceObjectApiItemSerializationTests
    {
        private static JsonApiSerializerSettings Settings = new JsonApiSerializerSettings { Formatting = Formatting.Indented };

        [Fact]
        // http://jsonapi.org/format/#document-resource-object-identification
        public void ShouldContainAtLeastIdAndTypeAsTopLevelItems()
        {
            var item = new ApiItem(new ContentItem { ContentItemId = "ABC", ContentType = "article" }, null);

            var itemSerialized = JsonConvert.SerializeObject(
                item,
                Settings);

            this.Assent(itemSerialized);
        }

        [Fact]
        public void ShouldErrorIfIdIsNotSupplied()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ApiItem(new ContentItem { ContentType = "article" }, null));
        }

        [Fact]
        public void ShouldErrorIfContentTypeIsNotSupplied()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ApiItem(new ContentItem {ContentItemId = "ABC" }, null));
        }

        [Fact]
        public void ShouldErrorIfNeitherContentTypeOrIdIsSupplied()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ApiItem(new ContentItem(), null));
        }

        [Fact]
        public void ShouldCreateJsonOutputWithDefaultValues()
        {
            var item = new ApiItem(
                new ContentItem {
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

            var itemSerialized = JsonConvert.SerializeObject(
                item,
                Settings);

            this.Assent(itemSerialized);
        }

        [Fact]
        public void ShouldAddContentPartAsNestedComplexObjectInDataAttributes()
        {
            var item = new ApiItem(
                new ContentItem
                {
                    ContentItemId = "ABC",
                    ContentType = "article"
                }, null);

            item.AddPart(new StubContentPart { Name = "FuBar" });

            var itemSerialized = JsonConvert.SerializeObject(
                item,
                Settings);

            this.Assent(itemSerialized);
        }

        [Fact]
        public void ShouldCreateLinkToSelf()
        {
            var contentItemId = "ABC";

            var expectedUrl = "http:\\fu.com\\" + contentItemId;
            var urlHelper = GetMockUrlHelper("Api.GetContents.ByType", expectedUrl);

            var item = new ApiItem(
                new ContentItem
                {
                    ContentItemId = contentItemId,
                    ContentType = "article"
                }, urlHelper);

            var itemSerialized = JsonConvert.SerializeObject(
                item,
                Settings);

            Assert.Single(item.Links);
            Assert.Equal(item.Links["self"].Href, "http:\\fu.com\\" + contentItemId);

            this.Assent(itemSerialized);
        }

        [Fact]
        public void ShouldCreateARelationshipToAnotherItem()
        {
            var articles = new ContentItem
            {
                ContentItemId = "1",
                ContentType = "articles"
            };

            var item = new ApiItem(
                articles, null);

            var person = new ContentItem
            {
                ContentItemId = "9",
                ContentType = "people"
            };

            item.AddRelationship(new ApiRelationshipItem(articles, person.ContentItem, null));

            var itemSerialized = JsonConvert.SerializeObject(
                item,
                Settings);

            this.Assent(itemSerialized);
        }

        private static IUrlHelper GetMockUrlHelper(string routeName, string returnValue)
        {
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(o => o.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == routeName))).Returns(returnValue);

            return urlHelper.Object;
        }
    }

    public class StubContentPart : ContentPart
    {
        public string Name { get; set; }
    }
}
