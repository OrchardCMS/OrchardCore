using System.Threading.Tasks;
using Moq;
using Xunit;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Modules;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using System;

namespace OrchardCore.Tests.OrchardCore.ContentManagement.Handlers
{
    public class ContentsHandlerTests
    {
        [Fact]
        public async Task CreatingContentItem_NotIgnoreModifiedUtcAndPublishedUtc()
        {
            var clock = new Mock<IClock>();
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            var contentHandler = new ContentsHandler(clock.Object, httpContextAccessor.Object);
            var modifiedUtc = "09/13/2019";
            var publishedUtc = "09/14/2019";
            var contentItem = new ContentItem {
                ContentType = "BlogPost",
                ContentItemId = "6bffd87d-27e0-4383-a8d1-140c5a027dfb",
                DisplayText = "Man must explore, and this is exploration at its greatest",
                Latest = true,
                Published = true,
                Owner = "admin",
                Author = "admin",
                ModifiedUtc = DateTime.Parse(modifiedUtc),
                PublishedUtc = DateTime.Parse(publishedUtc)
            };
            var contentContext = new CreateContentContext(contentItem);
            
            await contentHandler.CreatingAsync(contentContext);
            
            Assert.Equal(modifiedUtc, contentContext.ContentItem.ModifiedUtc.Value.ToString("MM/dd/yyyy"));
            Assert.Equal(publishedUtc, contentContext.ContentItem.PublishedUtc.Value.ToString("MM/dd/yyyy"));
        }
    }
}
