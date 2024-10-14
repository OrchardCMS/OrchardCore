using OrchardCore.Redis.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace OrchardCore.Tests.Modules.OrchardCore.Redis;

public class RedisCacheWrapperTests
{
    [Fact]
    public void RedisCacheWrapperMustNotDisposeInnerCache()
    {
        var redisCache = new Mock<IDistributedCache>();
        var disposable = redisCache.As<IDisposable>();

        object testObject = new RedisCacheWrapper(redisCache.Object);

        // If the RedisCacheWrapper ever implements IDisposable, make sure it does 'not'
        // dispose the inner cache object. 
        (testObject as IDisposable)?.Dispose();

        disposable.Verify(disposableMock => disposableMock.Dispose(), Times.Never());
    }
}
