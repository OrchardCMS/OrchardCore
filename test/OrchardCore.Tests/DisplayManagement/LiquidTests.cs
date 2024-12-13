using System.Text.Json;
using Fluid;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.Liquid;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.DisplayManagement;

public class LiquidTests
{
    [Fact]
    public async Task ComparingTextField_ReturnsCorrectValue()
    {
        var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var template = """
                {% if Model.ContentItem.Content.MyPart.myField.Text == "Some test value" %}true{% else %}false{% endif %}
                """;

            var contentItem = new ContentItem();
            contentItem.GetOrCreate<MyPart>();
            contentItem.Alter<MyPart>(x => x.Text = "Some text");
            contentItem.Alter<MyPart>(x =>
            {
                x.GetOrCreate<TextField>("myField");
                x.Alter<TextField>("myField", f => f.Text = "Some test value");
            });

            var json = JConvert.SerializeObject(contentItem);

            var testModel = JConvert.DeserializeObject<ContentItem>(json);

            var liquidTemplateManager = scope.ServiceProvider.GetRequiredService<ILiquidTemplateManager>();
            var result = await liquidTemplateManager.RenderStringAsync(template,
                NullEncoder.Default,
                testModel);

            Assert.Equal("true", result);
        });
    }

    [Fact]
    public async Task ComparingDateTimeField_ReturnsCorrectValue()
    {
        var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var template = """
                            {% assign myDate = "2024-05-14 08:00:00Z" | date %}
                            {% assign myDateField = Model.ContentItem.Content.MyPart.myField.Value | date %}
                            {% if myDateField == myDate %}true{% else %}false{% endif %}
                            """;

            var contentItem = new ContentItem();
            contentItem.GetOrCreate<MyPart>();
            contentItem.Alter<MyPart>(x => x.Text = "Some text");
            contentItem.Alter<MyPart>(x =>
            {
                x.GetOrCreate<DateTimeField>("myField");
                x.Alter<DateTimeField>("myField", f => f.Value = new DateTime(2024, 5, 14, 8, 0, 0, DateTimeKind.Utc));
            });

            var json = JConvert.SerializeObject(contentItem);

            var testModel = JConvert.DeserializeObject<ContentItem>(json);

            var liquidTemplateManager = scope.ServiceProvider.GetRequiredService<ILiquidTemplateManager>();
            var result = await liquidTemplateManager.RenderStringAsync(template,
                NullEncoder.Default,
                testModel);

            Assert.Contains("true", result);
        });
    }

    [Fact]
    public async Task ComparingNumericField_ReturnsCorrectValue()
    {
        var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var template = """
                {% if Model.ContentItem.Content.MyPart.myField.Value == 123 %}true{% else %}false{% endif %}
                """;

            var contentItem = new ContentItem();
            contentItem.GetOrCreate<MyPart>();
            contentItem.Alter<MyPart>(x => x.Text = "Some text");
            contentItem.Alter<MyPart>(x =>
            {
                x.GetOrCreate<MyField>("myField");
                x.Alter<MyField>("myField", f => f.Value = 123);
            });

            var json = JConvert.SerializeObject(contentItem);

            var testModel = JConvert.DeserializeObject<ContentItem>(json);

            var liquidTemplateManager = scope.ServiceProvider.GetRequiredService<ILiquidTemplateManager>();
            var result = await liquidTemplateManager.RenderStringAsync(template,
                NullEncoder.Default,
                testModel);

            Assert.Equal("true", result);
        });
    }

    [Fact]
    public async Task SortingContentItems_ShouldSortTheArrayOnIntegerField()
    {
        var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var template = """
                unsorted:{% for action in Model %}{{ action.Content.HotActionsPart.Order.Value }}{% endfor %}
                {% assign hotActions = Model | sort: "Content.HotActionsPart.Order.Value" %}
                sorted:{% for action in hotActions %}{{ action.Content.HotActionsPart.Order.Value }}{% endfor %}
                """;

            var json = JConvert.SerializeObject(FakeContentItems);

            var testModel = JConvert.DeserializeObject<ContentItem[]>(json);

            var liquidTemplateManager = scope.ServiceProvider.GetRequiredService<ILiquidTemplateManager>();
            var result = await liquidTemplateManager.RenderStringAsync(template,
                NullEncoder.Default,
                testModel);

            Assert.Equal("unsorted:302040605010sorted:102030405060", result.ReplaceLineEndings(""));
        });
    }

    [Fact]
    public async Task FilteringContentItems_ShouldFilterTheArrayOnBooleanField()
    {
        var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var template = """
                total:{{Model | size}}
                {% assign hotActions = Model | where: "Content.HotActionsPart.AddtoHotActionsMenu.Value", true %}
                filtered:{{ hotActions | size }}
                """;

            var json = JConvert.SerializeObject(FakeContentItems);

            var testModel = JConvert.DeserializeObject<ContentItem[]>(json);

            var liquidTemplateManager = scope.ServiceProvider.GetRequiredService<ILiquidTemplateManager>();
            var result = await liquidTemplateManager.RenderStringAsync(template,
                NullEncoder.Default,
                testModel);

            Assert.Equal("total:6filtered:5", result.ReplaceLineEndings(""));
        });
    }

    [Fact]
    public async Task FilteringAndSortingContentItems_ShouldFilterAndSortTheArrayOnContentFields()
    {
        var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var template = """
                original:{% for action in Model%}{{ action.Content.HotActionsPart.Order.Value }}{% endfor %}
                {% assign hotActions = Model | where: "Content.HotActionsPart.AddtoHotActionsMenu.Value", true | sort: "Content.HotActionsPart.Order.Value" %}
                filtered_sorted:{% for action in hotActions %}{{ action.Content.HotActionsPart.Order.Value }}{% endfor %}
                """;

            var json = JConvert.SerializeObject(FakeContentItems);

            var testModel = JConvert.DeserializeObject<ContentItem[]>(json);

            var liquidTemplateManager = scope.ServiceProvider.GetRequiredService<ILiquidTemplateManager>();
            var result = await liquidTemplateManager.RenderStringAsync(template,
                NullEncoder.Default,
                testModel);

            Assert.Equal("original:302040605010filtered_sorted:1030405060", result.ReplaceLineEndings(""));
        });
    }


    public static ContentItem[] FakeContentItems => new[] { "30_true", "20_false", "40_true", "60_true", "50_true", "10_true" }.Select(x => CreateFakeContentItem(decimal.Parse(x.Split('_')[0]), x.Split('_')[1] == "true")).ToArray();

    public static ContentItem CreateFakeContentItem(decimal order, bool addtoHotActionsMenu)
    {
        var contentItem = new ContentItem();
        contentItem.GetOrCreate<ContentPart>("HotActionsPart");
        contentItem.Alter<ContentPart>("HotActionsPart", x =>
        {
            x.GetOrCreate<BooleanField>("AddtoHotActionsMenu");
            x.Alter<BooleanField>("AddtoHotActionsMenu", f => f.Value = addtoHotActionsMenu);

            x.GetOrCreate<NumericField>("Order");
            x.Alter<NumericField>("Order", f => f.Value = order);

        });
        return contentItem;
    }

    public class MyPart : ContentPart
    {
        public string Text { get; set; }
    }

    public class MyField : ContentField
    {
        public int Value { get; set; }
    }

}
