using GraphQL.Types;
using Microsoft.Extensions.Localization;
using OrchardCore.Markdown.Fields;

namespace OrchardCore.Markdown.GraphQL
{
    public class MarkdownFieldQueryObjectType : ObjectGraphType<MarkdownField>
    {
        public MarkdownFieldQueryObjectType(IStringLocalizer<MarkdownFieldQueryObjectType> T)
        {
            Name = nameof(MarkdownField);
            Description = T["Content stored as Markdown. You can also query the HTML interpreted version of Markdown."];

            Field("markdown", x => x.Markdown, nullable: true)
                .Description(T["the markdown value"])
                .Type(new StringGraphType())
                ;

            Field("html", x => ToHtml(x.Markdown), nullable: true)
                .Description(T["the HTML representation of the markdown content"])
                .Type(new StringGraphType())
                ;
        }

        private static string ToHtml(string markdown)
        {
            return Markdig.Markdown.ToHtml(markdown ?? "");
        }
    }
}
