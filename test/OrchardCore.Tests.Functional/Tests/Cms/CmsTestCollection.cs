using Xunit;

namespace OrchardCore.Tests.Functional;

[CollectionDefinition(Name)]
public sealed class CmsTestCollection : ICollectionFixture<CmsSetupFixture>
{
    public const string Name = "CMS";
}
