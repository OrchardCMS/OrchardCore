using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Razor;

#pragma warning disable CA1050 // Declare types in namespaces
public static class OrchardRazorHelperExtensions
#pragma warning restore CA1050 // Declare types in namespaces
{
    public static async Task<IHtmlContent> DisplayAsync(this IOrchardDisplayHelper orchardDisplayHelper, ContentItem content, string displayType = "", string groupId = "", IUpdateModel updater = null)
    {
        var displayManager = orchardDisplayHelper.HttpContext.RequestServices.GetService<IContentItemDisplayManager>();
        var shape = await displayManager.BuildDisplayAsync(content, updater, displayType, groupId);

        return await orchardDisplayHelper.DisplayHelper.ShapeExecuteAsync(shape);
    }

    /// <summary>
    /// Renders an object in the browser's console.
    /// </summary>
    /// <param name="orchardHelper">The <see cref="IOrchardHelper"/>.</param>
    /// <param name="content">The object to render.</param>
    /// <returns>The encoded script rendering the object to the console.</returns>
    public static IHtmlContent ConsoleLog(this IOrchardHelper orchardHelper, object content)
    {
        var builder = new HtmlContentBuilder(3);

        builder.AppendHtml("<script>console.log(");

        var env = orchardHelper.HttpContext.RequestServices.GetRequiredService<IHostEnvironment>();

        if (content == null || env.IsProduction())
        {
            builder.AppendHtml("null");
        }
        else if (content is string stringContent)
        {
            builder.AppendHtml("\"").Append(stringContent).AppendHtml("\"");
        }
        else if (content is JToken jTokenContent)
        {
            builder.AppendHtml(jTokenContent.ToString());
        }
        else if (content is ContentItem contentItem)
        {
            builder.AppendHtml(ConvertContentItem(contentItem).ToString());
        }
        else if (content is IShape shape)
        {
            builder.AppendHtml(shape.ShapeToJson().ToString());
        }
        else
        {
            builder.AppendHtml(JsonConvert.SerializeObject(content));
        }

        builder.AppendHtml(")</script>");

        return builder;
    }

    internal static JObject ConvertContentItem(ContentItem contentItem)
    {
        var o = new JObject
        {
            // Write all well-known properties.
            new JProperty(nameof(ContentItem.ContentItemId), contentItem.ContentItemId),
            new JProperty(nameof(ContentItem.ContentItemVersionId), contentItem.ContentItemVersionId),
            new JProperty(nameof(ContentItem.ContentType), contentItem.ContentType),
            new JProperty(nameof(ContentItem.DisplayText), contentItem.DisplayText),
            new JProperty(nameof(ContentItem.Latest), contentItem.Latest),
            new JProperty(nameof(ContentItem.Published), contentItem.Published),
            new JProperty(nameof(ContentItem.ModifiedUtc), contentItem.ModifiedUtc),
            new JProperty(nameof(ContentItem.PublishedUtc), contentItem.PublishedUtc),
            new JProperty(nameof(ContentItem.CreatedUtc), contentItem.CreatedUtc),
            new JProperty(nameof(ContentItem.Owner), contentItem.Owner),
            new JProperty(nameof(ContentItem.Author), contentItem.Author),
            new JProperty(nameof(ContentItem.Content), (JObject)contentItem.Content),
        };

        return o;
    }
}
