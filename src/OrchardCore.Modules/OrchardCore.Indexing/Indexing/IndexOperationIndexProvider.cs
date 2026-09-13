using OrchardCore.Indexing.Core.Indexes;
using OrchardCore.Indexing.Core.Operations;
using YesSql.Indexes;

namespace OrchardCore.Indexing.Indexing;

internal sealed class IndexOperationIndexProvider : IndexProvider<IndexOperation>
{
    public override void Describe(DescribeContext<IndexOperation> context)
        => context.For<IndexOperationIndex>().Map(operation => new IndexOperationIndex { OperationId = operation.OperationId });
}
