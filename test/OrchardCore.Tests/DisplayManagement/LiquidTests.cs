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

    public class MyPart : ContentPart
    {
        public string Text { get; set; }
    }

    public class MyField : ContentField
    {
        public int Value { get; set; }
    }
}
