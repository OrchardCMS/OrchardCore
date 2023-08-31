using OrchardCore.Modules;

namespace OrchardCore.Tests.Modules
{
    public class PoweredByMiddlewareTests
    {
        [Fact]
        public async Task InjectPoweredByHeader()
        {
            // Arrange
            string key = "X-Powered-By", value = "OrchardCore";
            var headersArray = new Dictionary<string, StringValues>() { { key, String.Empty } };
            var headersDic = new HeaderDictionary(headersArray);
            var httpResponseMock = new Mock<HttpResponse>();
            httpResponseMock.SetupGet(r => r.Headers).Returns(headersDic);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.Response).Returns(httpResponseMock.Object);

            var optionsMock = new Mock<IPoweredByMiddlewareOptions>();
            optionsMock.SetupGet(o => o.Enabled).Returns(true);
            optionsMock.SetupGet(o => o.HeaderName).Returns(key);
            optionsMock.SetupGet(o => o.HeaderValue).Returns(value);
            static Task requestDelegate(HttpContext context) => Task.CompletedTask;
            var middleware = new PoweredByMiddleware(next: requestDelegate, options: optionsMock.Object);

            // Act
            await middleware.Invoke(httpContextMock.Object);

            // Assert
            Assert.Equal(value, headersArray[key]);
            httpResponseMock.Verify(r => r.Headers, Times.Once);
        }

        [Fact]
        public async Task DoNotInjectPoweredByHeaderIfDisabled()
        {
            // Arrange
            string key = "X-Powered-By", value = "OrchardCore";
            var httpResponseMock = new Mock<HttpResponse>();
            httpResponseMock.Setup(r => r.Headers.Add(key, value));

            Func<Task> dueTask = null;
            httpResponseMock.Setup(r => r.OnStarting(It.IsAny<Func<Task>>()))
                            .Callback<Func<Task>>((f) => dueTask = f);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.Response).Returns(httpResponseMock.Object);

            var optionsMock = new Mock<IPoweredByMiddlewareOptions>();
            optionsMock.SetupGet(o => o.Enabled).Returns(false);
            optionsMock.SetupGet(o => o.HeaderName).Returns(key);
            optionsMock.SetupGet(o => o.HeaderValue).Returns(value);
            static Task requestDelegate(HttpContext context) => Task.CompletedTask;
            var middleware = new PoweredByMiddleware(next: requestDelegate, options: optionsMock.Object);

            // Act
            await middleware.Invoke(httpContextMock.Object);

            // Assert
            Assert.Null(dueTask);
            httpResponseMock.Verify(r => r.Headers.Add(key, value), Times.Never);
        }
    }
}
