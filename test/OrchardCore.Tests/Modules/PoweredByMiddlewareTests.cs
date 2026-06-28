using OrchardCore.Modules;

namespace OrchardCore.Tests.Modules;

public class PoweredByMiddlewareTests
{
    [Fact]
    public async Task InjectPoweredByHeader()
    {
        // Arrange
        string key = "X-Powered-By", value = "OrchardCore";
        var poweredByOptions = Options.Create(new PoweredByOptions
        {
            HeaderName = key,
            HeaderValue = value,
        });

        var headersArray = new Dictionary<string, StringValues>() { { key, string.Empty } };
        var headersDic = new HeaderDictionary(headersArray);
        var httpResponseMock = new Mock<HttpResponse>();
        httpResponseMock.SetupGet(r => r.Headers)
            .Returns(headersDic);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.Response)
            .Returns(httpResponseMock.Object);

        static Task requestDelegate(HttpContext context) => Task.CompletedTask;

        var middleware = new PoweredByMiddleware(requestDelegate, poweredByOptions);

        // Act
        await middleware.Invoke(httpContextMock.Object);

        // Assert
        Assert.Equal(value, headersArray[key]);

        httpResponseMock.Verify(r => r.Headers, Times.Once);

        var applicationBuilderMock = new Mock<IApplicationBuilder>();
    }
}
