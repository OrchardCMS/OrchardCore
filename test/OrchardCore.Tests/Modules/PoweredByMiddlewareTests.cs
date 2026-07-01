using OrchardCore.Modules;

namespace OrchardCore.Tests.Modules;

public class PoweredByMiddlewareTests
{
    [Fact]
    public async Task InjectPoweredByHeader_Default_Succeeds()
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
    }

    [Fact]
    public async Task DoNotInjectPoweredByHeaderIfDisabled_Default_Succeeds()
    {
        // Arrange
        string key = "X-Powered-By", value = "OrchardCore";
        var poweredByOptions = Options.Create(new PoweredByOptions
        {
            Enabled = false,
            HeaderName = key,
            HeaderValue = value,
        });

        var httpResponseMock = new Mock<HttpResponse>();
#pragma warning disable ASP0019 // Suggest using IHeaderDictionary.Append or the indexer
        _ = httpResponseMock.Setup(r => r.Headers.Add(key, value));
#pragma warning restore ASP0019 // Suggest using IHeaderDictionary.Append or the indexer

        Func<Task> dueTask = null;
        httpResponseMock.Setup(r => r.OnStarting(It.IsAny<Func<Task>>()))
                        .Callback<Func<Task>>((f) => dueTask = f);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.Response).Returns(httpResponseMock.Object);

        static Task requestDelegate(HttpContext context) => Task.CompletedTask;
        var middleware = new PoweredByMiddleware(next: requestDelegate, poweredByOptions);

        // Act
        await middleware.Invoke(httpContextMock.Object);

        // Assert
        Assert.Null(dueTask);
#pragma warning disable ASP0019 // Suggest using IHeaderDictionary.Append or the indexer
        httpResponseMock.Verify(r => r.Headers.Add(key, value), Times.Never);
#pragma warning restore ASP0019 // Suggest using IHeaderDictionary.Append or the indexer
    }
}
