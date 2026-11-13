using Microsoft.Extensions.DependencyInjection;
using OrchardCore.AuditTrail.Indexes;
using OrchardCore.ContentLocalization.Indexes;
using OrchardCore.ContentLocalization.Records;
using OrchardCore.ContentManagement.Records;
using OrchardCore.DataPipelines.Indexes;
using OrchardCore.Lists.Indexes;
using OrchardCore.Notifications.Indexes;
using OrchardCore.Taxonomies.Indexing;
using OrchardCore.Users.Indexes;
using OrchardCore.Workflows.Indexes;
using YesSql.Indexes;

namespace OrchardCore.Tests.Data;

/// <summary>
/// Indexes with a single record per document declare it, so that YesSql doesn't group the queries that only join such indexes, and
/// counts them from the index tables. A map that starts returning several records would make saving throw, and should drop these
/// declarations instead.
/// </summary>
public class IndexDeclarationsTests
{
    public static TheoryData<Type, Type> SingleRecordIndexes => new()
    {
        { typeof(ContentItemIndexProvider), typeof(ContentItemIndex) },
        { typeof(ContainedPartIndexProvider), typeof(ContainedPartIndex) },
        { typeof(LocalizedContentItemIndexProvider), typeof(LocalizedContentItemIndex) },
        { typeof(UserIndexProvider), typeof(UserIndex) },
        { typeof(WorkflowIndexProvider), typeof(WorkflowIndex) },
        { typeof(DataPipelineRunIndexProvider), typeof(DataPipelineRunIndex) },
        { typeof(AuditTrailEventIndexProvider), typeof(AuditTrailEventIndex) },
        { typeof(NotificationIndexProvider), typeof(NotificationIndex) },
    };

    public static TheoryData<Type, Type> MultiRecordIndexes => new()
    {
        { typeof(TaxonomyIndexProvider), typeof(TaxonomyIndex) },
        { typeof(WorkflowIndexProvider), typeof(WorkflowBlockingActivitiesIndex) },
    };

    [Theory]
    [MemberData(nameof(SingleRecordIndexes))]
    public void Describe_SingleRecordIndex_DeclaresSingleRecordAndExclusive(Type providerType, Type indexType)
    {
        var descriptor = Describe(providerType, indexType);

        Assert.True(descriptor.IsSingleRecordPerDocument);
        Assert.True(descriptor.IsExclusiveToDocumentType);
    }

    [Theory]
    [MemberData(nameof(MultiRecordIndexes))]
    public void Describe_MultiRecordIndex_DeclaresNothing(Type providerType, Type indexType)
    {
        var descriptor = Describe(providerType, indexType);

        Assert.False(descriptor.IsSingleRecordPerDocument);
    }

    private static IndexDescriptor Describe(Type providerType, Type indexType)
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        var provider = (IIndexProvider)ActivatorUtilities.CreateInstance(services, providerType);
        var context = (IDescriptor)Activator.CreateInstance(typeof(DescribeContext<>).MakeGenericType(provider.ForType()));
        provider.Describe(context);

        return Assert.Single(context.Describe(provider.ForType()), descriptor => descriptor.IndexType == indexType);
    }
}
