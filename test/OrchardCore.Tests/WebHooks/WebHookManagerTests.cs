using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using OrchardCore.Environment.Shell;
using OrchardCore.WebHooks.Abstractions.Models;
using OrchardCore.WebHooks.Abstractions.Services;
using OrchardCore.WebHooks.Expressions;
using OrchardCore.WebHooks.Services;
using Xunit;

namespace OrchardCore.Tests.WebHooks
{
    public class WebHookManagerTests
    {
        [Fact]
        public async Task CanSendNotification()
        {
            // Setup
            var webHookList = new WebHookList
            {
                WebHooks = new List<WebHook>
                {
                    new WebHook
                    {
                        Id = "TestId",
                        Name = "Test WebHook #1",
                        Url = "https://test.org",
                        Secret = "123456789",
                        Events = new HashSet<string>(new [] {"article.published", "article.created", "article.updated"})
                    }
                }
            };

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync", 
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                // prepare the expected response 
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NoContent
                })
                .Verifiable();

            var mockClient = new HttpClient(handlerMock.Object);
            var clientFactory = new Mock<IHttpClientFactory>();
            clientFactory.Setup(x => x.CreateClient("webhooks")).Returns(mockClient);

            var expressionEvaluator = new Mock<IWebHookExpressionEvaluator>();
            var shellSettings = new ShellSettings { Name = "TestTenant" };
            var senderLogger = new Mock<ILogger<WebHookSender>>();     
            var sender = new WebHookSender(clientFactory.Object, expressionEvaluator.Object, shellSettings, senderLogger.Object);

            var store = new Mock<IWebHookStore>();
            store.Setup(x => x.GetAllWebHooksAsync()).ReturnsAsync(webHookList);
            var managerLogger = new Mock<ILogger<WebHookManager>>(); 

            var manager = new WebHookManager(sender, store.Object, managerLogger.Object);

            // Act
            await manager.NotifyAsync("article.created", JObject.Parse("{\"Test\":1}"), null);

            // Assert
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req => AssertRequest(req)),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        private bool AssertRequest(HttpRequestMessage request)
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("https://test.org/", request.RequestUri.ToString());
            Assert.Equal("TestTenant", request.Headers.GetValues("X-Orchard-Tenant").First());
            Assert.Equal("article.created", request.Headers.GetValues("X-Orchard-Event").First());
            Assert.Equal("TestId", request.Headers.GetValues("X-Orchard-Id").First());
            Assert.Equal("sha256=04498DF165202E5FA48A78E3F496DFE883CFCA21524E97BBA11A7BD9223C4778", request.Headers.GetValues("X-Orchard-Signature").First());
            Assert.Equal("{\"Test\":1}", request.Content.ReadAsStringAsync().Result);

            return true;
        }
    }
}