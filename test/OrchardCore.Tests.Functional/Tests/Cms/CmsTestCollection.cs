using OrchardCore.Tests.Functional.Tests.Cms;
using Xunit;

namespace OrchardCore.Tests.Functional;

[CollectionDefinition(Name)]
public sealed class CmsTestCollection : ICollectionFixture<SaasFixture>
{
    public const string Name = "CMS";
}
