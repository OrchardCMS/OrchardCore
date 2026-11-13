using Microsoft.Extensions.DependencyInjection;
using Moq;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Indexing.SQL;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Records;
using OrchardCore.DataPipelines.Indexes;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.SharedLinks;
using OrchardCore.Lists.Indexes;
using OrchardCore.Lists.Models;
using OrchardCore.Notifications;
using OrchardCore.Notifications.Indexes;
using OrchardCore.Notifications.Models;
using YesSql.Indexes;

namespace OrchardCore.Tests.ContentManagement;

public class IndexTextBoundaryTests
{
    [Theory]
    [InlineData("😀")]
    [InlineData("e\u0301")]
    [InlineData("🇺🇸")]
    [InlineData("👩‍💻")]
    public async Task Describe_ContentItem_PreservesWholeTextElements(string element)
    {
        var prefix = new string('a', ContentItemIndex.MaxDisplayTextSize - 1);
        var value = prefix + element + "more";
        var item = new ContentItem { ContentType = value, Owner = value, Author = value, DisplayText = value, Latest = true };
        item.Apply(new ContainedPart());

        var content = Assert.IsType<ContentItemIndex>(await MapAsync(new ContentItemIndexProvider(), item));
        Assert.Equal(prefix, content.ContentType);
        Assert.Equal(prefix, content.Owner);
        Assert.Equal(prefix, content.Author);
        Assert.Equal(prefix, content.DisplayText);

        var contained = Assert.IsType<ContainedPartIndex>(await MapAsync(new ContainedPartIndexProvider(), item));
        Assert.Equal(prefix, contained.DisplayText);
        Assert.Equal(value, item.DisplayText);
    }

    [Theory]
    [InlineData("😀")]
    [InlineData("e\u0301")]
    [InlineData("🇺🇸")]
    [InlineData("👩‍💻")]
    public async Task Describe_Notification_PreservesWholeTextElements(string element)
    {
        var prefix = new string('a', NotificationConstants.NotificationIndexContentLength - 1);
        var notification = new Notification { Summary = $"<p>{prefix}{element}more</p>" };

        var index = Assert.IsType<NotificationIndex>(await MapAsync(new NotificationIndexProvider(), notification));

        Assert.Equal(prefix, index.Content);
    }

    [Theory]
    [InlineData("😀")]
    [InlineData("e\u0301")]
    [InlineData("🇺🇸")]
    [InlineData("👩‍💻")]
    public async Task Describe_PipelineAndSharedLink_PreserveWholeTextElements(string element)
    {
        var name = new string('a', DataPipelineIndex.MaxNameLength - 1);
        var pipeline = Assert.IsType<DataPipelineIndex>(await MapAsync(new DataPipelineIndexProvider(), new DataPipeline { Name = name + element + "more" }));
        Assert.Equal(name, pipeline.Name);

        var correlation = new string('a', DataPipelineRunIndex.MaxCorrelationIdLength - 1);
        var run = Assert.IsType<DataPipelineRunIndex>(await MapAsync(new DataPipelineRunIndexProvider(), new DataPipelineRun { CorrelationId = correlation + element + "more" }));
        Assert.Equal(correlation, run.CorrelationId);

        var title = new string('a', SharedLinkIndex.MaxTitleLength - 1);
        var link = Assert.IsType<SharedLinkIndex>(await MapAsync(new SharedLinkIndexProvider(), new SharedLink { Title = title + element + "more", SourceName = title + element + "more" }));
        Assert.Equal(title, link.Title);
        Assert.Equal(title, link.SourceName);
    }

    public static TheoryData<string, string> FieldsAndElements => new(
        from fieldName in new[] { nameof(TextField), nameof(MultiTextField), nameof(LinkField) }
        from element in new[] { "😀", "e\u0301", "🇺🇸", "👩‍💻" }
        select (fieldName, element));

    [Theory]
    [MemberData(nameof(FieldsAndElements))]
    public async Task Describe_ContentFields_PreserveWholeElementsAndFullValues(string fieldName, string element)
    {
        var field = new ContentPartFieldDefinition(new ContentFieldDefinition(fieldName), "Value", []);
        var part = new ContentPartDefinition("Part", [field], []);
        var definition = new ContentTypeDefinition("Type", "Type", [new ContentTypePartDefinition("Part", part, [])], []);
        var manager = new Mock<IContentDefinitionManager>();
        manager.Setup(m => m.GetTypeDefinitionAsync("Type")).ReturnsAsync(definition);
        using var services = new ServiceCollection().AddSingleton(manager.Object).BuildServiceProvider();
        var prefix = new string('a', TextFieldIndex.MaxTextSize - 1);
        var value = prefix + element + "more";
        var item = new ContentItem { ContentType = "Type", Latest = true };
        item.Merge(new { Part = new { Value = new { Text = value, Url = value, Values = new[] { value } } } });

        switch (fieldName)
        {
            case nameof(TextField):
                var text = Assert.IsType<TextFieldIndex>(await MapAsync(new TextFieldIndexProvider(services), item));
                Assert.Equal(prefix, text.Text);
                Assert.Equal(value, text.BigText);
                break;
            case nameof(MultiTextField):
                var multi = Assert.IsType<MultiTextFieldIndex>(await MapAsync(new MultiTextFieldIndexProvider(services), item));
                Assert.Equal(prefix, multi.Value);
                Assert.Equal(value, multi.BigValue);
                break;
            default:
                var link = Assert.IsType<LinkFieldIndex>(await MapAsync(new LinkFieldIndexProvider(services), item));
                Assert.Equal(prefix, link.Text);
                Assert.Equal(prefix, link.Url);
                Assert.Equal(value, link.BigText);
                Assert.Equal(value, link.BigUrl);
                break;
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("short😀e\u0301")]
    public async Task Describe_WithinLimits_LeavesValuesUnchanged(string value)
    {
        var content = Assert.IsType<ContentItemIndex>(await MapAsync(new ContentItemIndexProvider(), new ContentItem { DisplayText = value, Author = value }));
        Assert.Equal(value, content.DisplayText);
        Assert.Equal(value, content.Author);
        var pipeline = Assert.IsType<DataPipelineIndex>(await MapAsync(new DataPipelineIndexProvider(), new DataPipeline { Name = value }));
        Assert.Equal(value, pipeline.Name);
        var link = Assert.IsType<SharedLinkIndex>(await MapAsync(new SharedLinkIndexProvider(), new SharedLink { Title = value, SourceName = value }));
        Assert.Equal(value, link.Title);
        Assert.Equal(value, link.SourceName);
    }

    private static async Task<IIndex> MapAsync<T>(IndexProvider<T> provider, T value) where T : class
    {
        var context = new DescribeContext<T>();
        provider.Describe(context);
        var descriptor = Assert.Single(context.Describe([typeof(T)]));

        return Assert.Single(await descriptor.Map(value, CancellationToken.None));
    }
}
