using System.Text.Encodings.Web;
using Cysharp.Text;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types;

public sealed class ContentItemType : ObjectGraphType<ContentItem>
{
    private readonly GraphQLContentOptions _options;

    public ContentItemType(IOptions<GraphQLContentOptions> optionsAccessor, IStringLocalizer<ContentItemType> S)
    {
        _options = optionsAccessor.Value;

        Name = "ContentItemType";

        Field(ci => ci.ContentItemId)
            .Description(S["Content item id"]);

        Field(ci => ci.ContentItemVersionId, nullable: true)
            .Description(S["The content item version id"]);

        Field(ci => ci.ContentType)
            .Description(S["Type of content"]);

        Field(ci => ci.DisplayText, nullable: true)
            .Description(S["The display text of the content item"]);

        Field(ci => ci.Published)
            .Description(S["Is the published version"]);

        Field(ci => ci.Latest)
            .Description(S["Is the latest version"]);

        Field<DateTimeGraphType>("modifiedUtc")
            .Resolve(ci => ci.Source.ModifiedUtc)
            .Description(S["The date and time of modification"]);

        Field<DateTimeGraphType>("publishedUtc")
            .Resolve(ci => ci.Source.PublishedUtc)
            .Description(S["The date and time of publication"]);

        Field<DateTimeGraphType>("createdUtc")
            .Resolve(ci => ci.Source.CreatedUtc)
            .Description(S["The date and time of creation"]);

        Field(ci => ci.Owner, nullable: true)
            .Description(S["The owner of the content item"]);

        Field(ci => ci.Author)
            .Description(S["The author of the content item"]);

        Field<StringGraphType, string>("render")
            .ResolveLockedAsync(RenderShapeAsync);

        Interface<ContentItemInterface>();

        IsTypeOf = IsContentType;
    }

    private bool IsContentType(object obj)
    {
        return obj is ContentItem item && item.ContentType == Name;
    }

    public override FieldType AddField(FieldType fieldType)
    {
        if (!_options.ShouldSkip(GetType(), fieldType.Name))
        {
            return base.AddField(fieldType);
        }

        return null;
    }

    private static async ValueTask<string> RenderShapeAsync(IResolveFieldContext<ContentItem> context)
    {
        var serviceProvider = context.RequestServices;

        // Build shape
        var displayManager = serviceProvider.GetRequiredService<IContentItemDisplayManager>();
        var updateModelAccessor = serviceProvider.GetRequiredService<IUpdateModelAccessor>();
        var model = await displayManager.BuildDisplayAsync(context.Source, updateModelAccessor.ModelUpdater);

        var displayHelper = serviceProvider.GetRequiredService<IDisplayHelper>();
        var htmlEncoder = serviceProvider.GetRequiredService<HtmlEncoder>();

        using var sw = new ZStringWriter();
        var htmlContent = await displayHelper.ShapeExecuteAsync(model);
        htmlContent.WriteTo(sw, htmlEncoder);

        return sw.ToString();
    }
}
