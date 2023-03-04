using OrchardCore.Testing.Infrastructure;

namespace OrchardCore.Testing.Apis
{
    public interface ISiteContext<TSiteStartup> : ISiteContext where TSiteStartup : class
    {
        static OrchardCoreTestFixture<TSiteStartup> Site { get; }
    }
}
