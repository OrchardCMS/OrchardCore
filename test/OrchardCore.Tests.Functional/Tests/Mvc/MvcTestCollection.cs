using Xunit;

namespace OrchardCore.Tests.Functional;

[CollectionDefinition(Name)]
public sealed class MvcTestCollection : ICollectionFixture<MvcSetupFixture>
{
    public const string Name = "MVC";
}
