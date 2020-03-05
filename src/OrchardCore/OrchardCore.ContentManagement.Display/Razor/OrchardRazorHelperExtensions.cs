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

public static class OrchardRazorHelperExtensions
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
        var o = new JObject();

        // Write all well-known properties
        o.Add(new JProperty(nameof(ContentItem.ContentItemId), contentItem.ContentItemId));
        o.Add(new JProperty(nameof(ContentItem.ContentItemVersionId), contentItem.ContentItemVersionId));
        o.Add(new JProperty(nameof(ContentItem.ContentType), contentItem.ContentType));
        o.Add(new JProperty(nameof(ContentItem.DisplayText), contentItem.DisplayText));
        o.Add(new JProperty(nameof(ContentItem.Latest), contentItem.Latest));
        o.Add(new JProperty(nameof(ContentItem.Published), contentItem.Published));
        o.Add(new JProperty(nameof(ContentItem.ModifiedUtc), contentItem.ModifiedUtc));
        o.Add(new JProperty(nameof(ContentItem.PublishedUtc), contentItem.PublishedUtc));
        o.Add(new JProperty(nameof(ContentItem.CreatedUtc), contentItem.CreatedUtc));
        o.Add(new JProperty(nameof(ContentItem.Owner), contentItem.Owner));
        o.Add(new JProperty(nameof(ContentItem.Author), contentItem.Author));

        o.Add(new JProperty(nameof(ContentItem.Content), (JObject)contentItem.Content));

        return o;
    }
}
