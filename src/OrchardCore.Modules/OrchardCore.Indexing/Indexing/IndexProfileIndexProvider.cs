using OrchardCore.Indexing.Core.Indexes;
using OrchardCore.Indexing.Models;
using YesSql.Indexes;

namespace OrchardCore.Indexing.Indexing;

internal sealed class IndexProfileIndexProvider : IndexProvider<IndexProfile>
{
    public override void Describe(DescribeContext<IndexProfile> context)
    {
        context.For<IndexProfileIndex>()
            .Map(indexProfile =>
            {
                return new IndexProfileIndex
                {
                    IndexProfileId = indexProfile.Id,
                    Name = indexProfile.Name,
                    IndexName = indexProfile.IndexName,
                    ProviderName = indexProfile.ProviderName,
                    Type = indexProfile.Type,
                };
            });
    }
}
